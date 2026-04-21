using AutoMapper;
using ICR.API.Authorization;
using ICR.Application.Mapping;
using ICR.Application.Services;
using ICR.Domain.Model;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.DashboardAggregate;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.MinisterAggregate;
using ICR.Domain.Model.RepassAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using ICR.Infra;
using ICR.Infra.Data.Repositories;
using ICR.Infra.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.AddSingleton<TokenService>();

        // Add services to the container
        builder.Services.AddControllers();

        builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100, // Ajustado para ser mais amig�vel globalmente
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(10)
                    });
            });

            options.AddPolicy("Login", context =>
            {
                var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    ip,
                    partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5, // Limite estrito de 5 tentativas
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1) // Por cada 1 minuto
                    });
            });
        });

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? Array.Empty<string>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowConfigured", policy =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                else if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
                else
                    throw new InvalidOperationException("Cors:AllowedOrigins deve ser configurado em produção.");
            });
        });

        // AutoMapper
        builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DomainToDTOMapping)));

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
            });
        });

        // Connection string (supports ConnectionStrings__DefaultConnection and DATABASE_URL from platform)
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var npgBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = userInfo.Length > 0 ? userInfo[0] : string.Empty,
                Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            connectionString = npgBuilder.ToString();
        }

        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("REDACTED") || connectionString.Contains("CHANGE_ME"))
        {
            if (builder.Environment.IsDevelopment())
            {
                connectionString = "Host=localhost;Port=5432;Database=icr_connect;Username=icradmin;Password=root";
            }
            else
            {
                throw new InvalidOperationException("DefaultConnection não configurada. Defina ConnectionStrings__DefaultConnection (ou DATABASE_URL) antes de iniciar a aplicação.");
            }
        }

        var port = Environment.GetEnvironmentVariable("PORT");
        if (!string.IsNullOrWhiteSpace(port))
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

        builder.Services.AddDbContext<ConnectionContext>(options =>
            options.UseNpgsql(connectionString)
        );
        builder.Services.AddHostedService<MonthlyReferenceJob>();
        builder.Services.AddHealthChecks();

        // Reposit�rio
        builder.Services.AddTransient<IFederationRepository, FederationRepository>();
        builder.Services.AddTransient<IChurchRepository, ChurchRepository>();
        // register concrete repositories (interfaces differ across implementations in this solution)
        builder.Services.AddTransient<ICellRepository, CellRepository>();
        builder.Services.AddTransient<IFamilyRepository, FamilyRepository>();
        builder.Services.AddTransient<IMemberRepository, MemberRepository>();
        builder.Services.AddTransient<IMinisterRepository, MinisterRepository>();
        builder.Services.AddTransient<IRepassRepository, RepassRepository>();
        builder.Services.AddTransient<IUserRoleRepository, UserRoleRepository>();
        builder.Services.AddTransient<IDashboardRepository, DashboardRepository>();


        // JWT
        var secret = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["JWT_KEY"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            if (builder.Environment.IsDevelopment())
                secret = "dev-only-jwt-secret-key-minimum-32-chars!";
            else
                throw new InvalidOperationException(
                    "JWT_KEY não configurado. Defina a variável de ambiente JWT_KEY ou a chave 'JWT_KEY' no arquivo de configuração antes de iniciar a aplicação.");
        }

        var key = Encoding.ASCII.GetBytes(secret);
        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = builder.Environment.IsProduction();
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
        builder.Services.AddAuthorization(options =>
        {
            foreach (User.UserScope scope in Enum.GetValues<User.UserScope>())
            {
                options.AddPolicy(
                    AuthorizeScopeAttribute.BuildPolicyName(scope),
                    policy => policy
                        .RequireAuthenticatedUser()
                        .AddRequirements(new MinimumScopeRequirement(scope)));
            }
        });

        var app = builder.Build();
        var userroot = Environment.GetEnvironmentVariable("ROOTUSERNAME") ?? app.Configuration["ROOTUSERNAME"];
        var rootpass = Environment.GetEnvironmentVariable("ROOTPASSWORD") ?? app.Configuration["ROOTPASSWORD"];
        var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ConnectionContext>();

                if (!context.Database.CanConnect())
                    throw new InvalidOperationException("Não foi possível conectar ao banco de dados.");

                if (applyMigrations && context.Database.IsRelational())
                    context.Database.Migrate();

                if (!context.Set<User>().Any())
                {
                    if (string.IsNullOrWhiteSpace(userroot) || string.IsNullOrWhiteSpace(rootpass))
                    {
                        if (app.Environment.IsDevelopment())
                        {
                            userroot = "admin";
                            rootpass = "admin123";
                        }
                        else
                        {
                            throw new InvalidOperationException("ROOTUSERNAME e ROOTPASSWORD devem ser definidos para inicializar o usuário root.");
                        }
                    }

                    var rootUser = new User(
                        null,
                        userroot,
                        BCrypt.Net.BCrypt.HashPassword(rootpass),
                        User.UserScope.NATIONAL
                    );
                    context.Add(rootUser);
                    context.SaveChanges();

                    var loggerSeed = services.GetRequiredService<ILogger<Program>>();
                    loggerSeed.LogInformation("Usuário root criado com sucesso. Username: {Username}", userroot);
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogCritical(ex, "Falha crítica na inicialização. Abortando.");
                throw;
            }
        }
        app.UseCors("AllowConfigured");

        app.UseForwardedHeaders();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

            if (app.Environment.IsDevelopment())
            {
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
            }
            else
            {
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
            }

            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            await next();
        });

        // Pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            // Adicionado HSTS para aumentar a seguran�a em produ��o
            app.UseHsts();
        }
        else
        {
            app.UseExceptionHandler("/error-development");
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

    
        app.MapHealthChecks("/health");
        app.MapControllers();
        app.Run();
    }
}