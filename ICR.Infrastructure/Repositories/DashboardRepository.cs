using ICR.Domain.DTOs;
using ICR.Domain.Model.DashboardAggregate;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.MemberAggregate;
using ICR.Infra;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ICR.Infra.Data.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ConnectionContext _context;
        public DashboardRepository(ConnectionContext context) => _context = context;

        private IQueryable<Member> BaseMemberQuery()
        {
            return _context.Members
                .AsNoTracking()
                .Include(m => m.Family)
                    .ThenInclude(f => f.Church);
        }

        private static IEnumerable<EnumCountResponseDTO> BuildEnumCounts<TEnum>(
            IEnumerable<EnumCountResponseDTO> groupedCounts)
            where TEnum : struct, Enum
        {
            var countsById = groupedCounts.ToDictionary(x => x.Id, x => x.Count);

            return Enum.GetValues<TEnum>()
                .Select(value => new EnumCountResponseDTO
                {
                    Id = Convert.ToInt32(value),
                    Name = value.ToString(),
                    Count = countsById.TryGetValue(Convert.ToInt32(value), out var count) ? count : 0
                })
                .OrderBy(x => x.Id)
                .ToList();
        }

        private async Task<IEnumerable<EnumCountResponseDTO>> BuildEnumCountsAsync<TEnum>(
            IQueryable<Member> query,
            Expression<Func<Member, TEnum>> selector)
            where TEnum : struct, Enum
        {
            var grouped = await query
                .GroupBy(selector)
                .Select(g => new EnumCountResponseDTO
                {
                    Id = Convert.ToInt32(g.Key),
                    Name = g.Key.ToString() ?? string.Empty,
                    Count = g.Count()
                })
                .ToListAsync();

            return BuildEnumCounts<TEnum>(grouped);
        }

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

        public async Task<IEnumerable<EnumCountResponseDTO>> GetClassCountsNationalAsync()
        {
            return await BuildEnumCountsAsync(BaseMemberQuery(), m => m.Class);
        }

        public async Task<IEnumerable<EnumCountResponseDTO>> GetClassCountsFederationAsync(long federationId)
        {
            var query = BaseMemberQuery()
                .Where(m => m.Family.Church.FederationId == federationId);

            return await BuildEnumCountsAsync(query, m => m.Class);
        }

        public async Task<IEnumerable<EnumCountResponseDTO>> GetClassCountsChurchAsync(long churchId)
        {
            var query = BaseMemberQuery()
                .Where(m => m.Family.ChurchId == churchId);

            return await BuildEnumCountsAsync(query, m => m.Class);
        }

        public async Task<IEnumerable<EnumCountResponseDTO>> GetMemberRoleCountsNationalAsync()
        {
            return await BuildEnumCountsAsync(BaseMemberQuery(), m => m.Role);
        }

        public async Task<IEnumerable<EnumCountResponseDTO>> GetMemberRoleCountsFederationAsync(long federationId)
        {
            var query = BaseMemberQuery()
                .Where(m => m.Family.Church.FederationId == federationId);

            return await BuildEnumCountsAsync(query, m => m.Role);
        }

        public async Task<IEnumerable<EnumCountResponseDTO>> GetMemberRoleCountsChurchAsync(long churchId)
        {
            var query = BaseMemberQuery()
                .Where(m => m.Family.ChurchId == churchId);

            return await BuildEnumCountsAsync(query, m => m.Role);
        }
    }
}