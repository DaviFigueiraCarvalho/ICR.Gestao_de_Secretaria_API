using System;
using System.Linq;
using ICR.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ICR.Tests.Integration
{
    // A Fábrica customizada é responsável por subir a API de testes substituindo dependências reais 
    // por mocks e fakes (ex: InMemoryDatabase ao invés de PostgreSQL)
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Opcional: Define ambiente como Testing para pular certas verificações no Program.cs
            builder.UseEnvironment("Testing");
            Environment.SetEnvironmentVariable("JWT_KEY", "ChaveSecretaMuitoLongaParaTesteQueNãoVaiPraProducao32Bits");

            builder.ConfigureServices(services =>
            {
                // 1. Localiza a configuração atual do DbContext da infraestrutura principal (EF/Postgres)
                var efDescriptors = services.Where(d => 
                    d.ServiceType == typeof(DbContextOptions<ConnectionContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(System.Data.Common.DbConnection) ||
                    d.ServiceType.Namespace != null && d.ServiceType.Namespace.Contains("EntityFrameworkCore")
                ).ToList();

                // 2. Se achar, remove a injeção atual baseada em banco de dados real
                foreach (var descriptor in efDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Removemos o HostedService (Job) para não rodar paralelamente durante os testes e estragar transações
                // O tipo exato depende do namespace, mas por padrão ele herda de IHostedService.
                var hostedServices = services.Where(s => s.ServiceType == typeof(IHostedService)).ToList();
                foreach (var svc in hostedServices)
                {
                    // Removendo o Job Agendado "MonthlyReferenceJob" que fica em background
                    if (svc.ImplementationType != null && svc.ImplementationType.Name.Contains("MonthlyReferenceJob"))
                    {
                        services.Remove(svc);
                    }
                }

                // 3. Adiciona o DbContext usando o Entity Framework InMemory
                // Gera um nome único por teste para garantir isolamento limpo
                string dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
                services.AddDbContext<ConnectionContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // A inicialização (EnsureCreated) já é feita no Program.cs na proteção de escopo ao subir a aplicação, 
                // então não precisamos forçar o BuildServiceProvider aqui (o que também causa exceções no EF).
            });
        }
    }
}
