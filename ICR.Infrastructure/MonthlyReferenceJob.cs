using ICR.Domain.Model.RepassAggregate;
using ICR.Infra;
using ICR.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class MonthlyReferenceJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonthlyReferenceJob> _logger;

    public MonthlyReferenceJob(IServiceScopeFactory scopeFactory, ILogger<MonthlyReferenceJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Run(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar MonthlyReferenceJob. O serviço continuará ativo.");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
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
        // REMOVE REFERENCES ANTIGAS
        // ===============================
        var oldReferences = await context.References
            .Where(r => r.CompetenceDate <= deleteLimit)
            .ToListAsync(stoppingToken);

        if (oldReferences.Any())
        {
            context.References.RemoveRange(oldReferences);
        }

        try
        {
            await context.SaveChangesAsync(stoppingToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Não foi possível persistir alterações de Reference (provável registro já existente ou referência em uso). A API continuará ativa.");
        }
    }
}
