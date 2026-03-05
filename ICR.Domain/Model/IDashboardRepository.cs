using ICR.Domain.DTOs;

namespace ICR.Domain.Model.DashboardAggregate
{
    public interface IDashboardRepository
    {
        Task<DashboardStatsDTO> GetNationalStatsAsync();
        Task<DashboardStatsDTO> GetFederationStatsAsync(long federationId);
        Task<DashboardStatsDTO> GetChurchStatsAsync(long churchId);
    }
}