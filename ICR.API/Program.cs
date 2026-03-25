using AutoMapper;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
                        PermitLimit = 100, // Ajustado para ser mais amigável globalmente
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

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("https://meudominio.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
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

        // ConnectionContext via DI com appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ConnectionContext>(options =>
            options.UseNpgsql(connectionString)
        );
        builder.Services.AddHostedService<MonthlyReferenceJob>();

        // Repositório
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
        var secret = Environment.GetEnvironmentVariable("JWT_KEY");
        if (string.IsNullOrWhiteSpace(secret))
            secret = builder.Configuration["JWT_KEY"];

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

        var app = builder.Build();
        // BLOCO DE PROTEÇÃO: Roda antes de qualquer coisa
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ConnectionContext>();

                // Se houver migrations pendentes, ele aplica. 
                // Se o banco estiver vazio, ele cria a estrutura.
                if (context.Database.IsRelational())
                {
                    context.Database.Migrate();
                }
                else
                {
                    context.Database.EnsureCreated();
                }

                if (!context.Set<User>().Any())
                {
                    var rootUser = new User(
                        null,
                        "admin",
                        BCrypt.Net.BCrypt.HashPassword("admin123"),
                        User.UserScope.NATIONAL
                    );
                    context.Add(rootUser);
                    context.SaveChanges();

                    var loggerSeed = services.GetRequiredService<ILogger<Program>>();
                    loggerSeed.LogInformation("Usuário root criado com sucesso! Login: admin | Senha: admin123");
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogCritical(ex, "O banco de dados não está pronto. Abortando inicialização.");
                throw; // Mata a aplicação antes do MonthlyReferenceJob quebrar tudo
            }
        }
        app.UseCors("AllowAll");

        app.UseForwardedHeaders();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            await next();
        });

        // Pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            // Adicionado HSTS para aumentar a segurança em produção
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

    
        app.MapControllers();
        app.Run();
    }
}