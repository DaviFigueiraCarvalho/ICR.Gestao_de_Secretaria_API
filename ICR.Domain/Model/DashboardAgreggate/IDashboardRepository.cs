using ICR.Domain.DTOs;

namespace ICR.Domain.Model.DashboardAggregate
{
    public interface IDashboardRepository
    {
        Task<DashboardStatsDTO> GetNationalStatsAsync();
        Task<DashboardStatsDTO> GetFederationStatsAsync(long federationId);
        Task<DashboardStatsDTO> GetChurchStatsAsync(long churchId);
        Task<IEnumerable<EnumCountResponseDTO>> GetClassCountsNationalAsync();
        Task<IEnumerable<EnumCountResponseDTO>> GetClassCountsFederationAsync(long federationId);
        Task<IEnumerable<EnumCountResponseDTO>> GetClassCountsChurchAsync(long churchId);
        Task<IEnumerable<EnumCountResponseDTO>> GetMemberRoleCountsNationalAsync();
        Task<IEnumerable<EnumCountResponseDTO>> GetMemberRoleCountsFederationAsync(long federationId);
        Task<IEnumerable<EnumCountResponseDTO>> GetMemberRoleCountsChurchAsync(long churchId);
    }
}