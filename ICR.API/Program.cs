using AutoMapper;
using ICR.Application.Services;
using ICR.Application.Mapping;
using ICR.Infra;
using ICR.Infra.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ICR.Domain.Model.FederationAggregate;
using ICR.Infra.Data.Repositories;
using ICR.Domain.Model.MinisterAggregate;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.RepassAggregate;
using ICR.Domain.Model.UserRoleAgreggate;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.AddSingleton<TokenService>();

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddScoped<IdSequenceService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        builder.WebHost.UseUrls("http://0.0.0.0:8080", "https://0.0.0.0:8081");

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
            x.RequireHttpsMetadata = false;
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
        app.UseCors("AllowAll");

        // Pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error-development");
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/error");
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.Run();
    }
}