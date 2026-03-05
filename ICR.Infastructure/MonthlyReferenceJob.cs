using ICR.Domain.Model.RepassAggregate;
using ICR.Infra;
using ICR.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class MonthlyReferenceJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MonthlyReferenceJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Run(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task Run(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ConnectionContext>();

        var now = DateTime.UtcNow;

        // Competência do mês ATUAL (dia 1, meia-noite UTC)
        var currentCompetence = DateTime.SpecifyKind(
            new DateTime(now.Year, now.Month, 1).AddMonths(-1),
            DateTimeKind.Utc
        );

        // Limite de exclusão: tudo <= 13 meses atrás
        var deleteLimit = currentCompetence.AddMonths(-13);

        // ===============================
        // CRIA REFERENCE SE NÃO EXISTIR
        // ===============================
        var exists = await context.References.AnyAsync(r =>
            r.CompetenceDate == currentCompetence,
            stoppingToken
        );

        if (!exists)
        {

            var reference = new Reference(currentCompetence)
            {
            };

            context.References.Add(reference);
        }

        // ===============================
        // REMOVE REFERENCES ANTIGAS
        // ===============================
        var oldReferences = await context.References
            .Where(r => r.CompetenceDate <= deleteLimit)
            .ToListAsync(stoppingToken);

        if (oldReferences.Any())
        {
            context.References.RemoveRange(oldReferences);
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
