using ICR.Domain.DTOs;
using ICR.Domain.Model.DashboardAggregate;
using ICR.Domain.Model.CellAggregate;
using ICR.Infra;
using Microsoft.EntityFrameworkCore;

namespace ICR.Infra.Data.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ConnectionContext _context;
        public DashboardRepository(ConnectionContext context) => _context = context;

        public async Task<DashboardStatsDTO> GetNationalStatsAsync()
        {
            return new DashboardStatsDTO
            {
                TotalFederations = await _context.Federations.CountAsync(),
                TotalChurches = await _context.Churches.CountAsync(),
                TotalMissionaryCommunities = await _context.Cells.CountAsync(c => c.Type == Cell.CellType.ComunidadeMissionaria),
                TotalFamilies = await _context.Families.CountAsync(),
                TotalCells = await _context.Cells.CountAsync(c => c.Type == Cell.CellType.Celula),
                TotalMembers = await _context.Members.CountAsync()
            };
        }

        public async Task<DashboardStatsDTO> GetFederationStatsAsync(long federationId)
        {
            return new DashboardStatsDTO
            {
                TotalChurches = await _context.Churches.CountAsync(c => c.FederationId == federationId),
                TotalFamilies = await _context.Families.CountAsync(f => f.Church.FederationId == federationId),
                TotalCells = await _context.Cells.CountAsync(c => c.Church.FederationId == federationId && c.Type == Cell.CellType.Celula),
                TotalMissionaryCommunities = await _context.Cells.CountAsync(c => c.Church.FederationId == federationId && c.Type == Cell.CellType.ComunidadeMissionaria),
                TotalMembers = await _context.Members.CountAsync(m => m.Family.Church.FederationId == federationId)
            };
        }

        public async Task<DashboardStatsDTO> GetChurchStatsAsync(long churchId)
        {
            return new DashboardStatsDTO
            {
                TotalFamilies = await _context.Families.CountAsync(f => f.ChurchId == churchId),
                TotalCells = await _context.Cells.CountAsync(c => c.ChurchId == churchId && c.Type == Cell.CellType.Celula),
                TotalMembers = await _context.Members.CountAsync(m => m.Family.ChurchId == churchId),
                TotalMissionaryCommunities = await _context.Cells.CountAsync(c => c.ChurchId == churchId && c.Type == Cell.CellType.ComunidadeMissionaria)
            };
        }
    }
}