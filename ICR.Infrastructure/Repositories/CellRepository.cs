using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.MemberAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Data.Repositories
{
    public class CellRepository : ICellRepository
    {
        private readonly ConnectionContext _context;

        public CellRepository(ConnectionContext context)
        {
            _context = context;
        }

        public async Task<CellResponseDTO> AddAsync(CellDTO dto)
        {
            // Busca igreja
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => c.Id == dto.ChurchId);

            if (church == null)
                return new CellResponseDTO
                {
                    Id = 0,
                };

            // Busca responsável
            Member? responsible = null;

            if (dto.ResponsibleId == 0)
            {
                dto.ResponsibleId = null;
            }

            if (dto.ResponsibleId.HasValue)
            {
                responsible = await _context.Members
                    .FirstOrDefaultAsync(m => m.Id == dto.ResponsibleId);
            }

            var cell = new Cell(
                dto.Name,
                dto.Type,
                church.Id,
                responsible?.Id
            );

            _context.Cells.Add(cell);
            await _context.SaveChangesAsync();

            return new CellResponseDTO
            {
                Id = cell.Id,
                Name = cell.Name,
                ChurchId = church.Id,
                ChurchName = church.Name,
                ResponsibleId = responsible?.Id,
                ResponsibleName = responsible?.Name,
            };
        }

        public async Task<CellResponseDTO?> GetByIdAsync(long id)
        {
            return await _context.Cells
                .Include(c => c.Church)
                .Include(c => c.Responsible)
                .Where(c => c.Id == id)
                .Select(c => new CellResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ChurchId = c.ChurchId,
                    ChurchName = c.Church != null ? c.Church.Name : null,
                    ResponsibleId = c.ResponsibleId,
                    ResponsibleName = c.Responsible != null ? c.Responsible.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CellResponseDTO>> GetAllAsync()
        {
            return await _context.Cells
                .Include(c => c.Church)
                .Include(c => c.Responsible)
                .OrderBy(c => c.Id)
                .Select(c => new CellResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ChurchId = c.ChurchId,
                    ChurchName = c.Church != null ? c.Church.Name : null,
                    ResponsibleId = c.ResponsibleId,
                    ResponsibleName = c.Responsible != null ? c.Responsible.Name : null
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CellResponseDTO>> GetFilteredAsync(long? federationId, long? churchId)
        {
            var query = _context.Cells
                .Include(c => c.Church)
                .Include(c => c.Responsible)
                .AsQueryable();

            if (churchId.HasValue)
            {
                query = query.Where(c => c.ChurchId == churchId.Value);
            }
            if (federationId.HasValue)
            {
                query = query.Where(c => c.Church != null && c.Church.FederationId == federationId.Value);
            }

            return await query
                .OrderBy(c => c.Id)
                .Select(c => new CellResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ChurchId = c.ChurchId,
                    ChurchName = c.Church != null ? c.Church.Name : null,
                    ResponsibleId = c.ResponsibleId,
                    ResponsibleName = c.Responsible != null ? c.Responsible.Name : null
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CellResponseDTO>> GetByChurchIdAsync(Member leader)
        {
            return await _context.Cells
                .Include(c => c.Church)
                .Include(c => c.Responsible)
                .Where(c => c.ResponsibleId == leader.Id)
                .Select(c => new CellResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ChurchId = c.ChurchId,
                    ChurchName = c.Church != null ? c.Church.Name : null,
                    ResponsibleId = c.ResponsibleId,
                    ResponsibleName = c.Responsible != null ? c.Responsible.Name : null
                })
                .ToListAsync();
        }

        public async Task<CellResponseDTO> UpdateAsync(long id, CellPatchDTO updatedCell)
        {
            var cell = await _context.Cells
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cell == null)
            {
                return new CellResponseDTO
                {
                    Id = 0,
                };
            }

            // Name
            if (!string.IsNullOrWhiteSpace(updatedCell.Name))
                cell.SetName(updatedCell.Name);

            // ResponsibleId (PATCH de verdade)
            if (updatedCell.ResponsibleId.HasValue)
            {
                if (updatedCell.ResponsibleId.Value == 0)
                {
                    // remove responsável
                    cell.SetResponsible(null);
                }
                else if (updatedCell.ResponsibleId.Value != cell.ResponsibleId)
                {
                    var responsibleExists = await _context.Members
                        .AnyAsync(m => m.Id == updatedCell.ResponsibleId.Value);

                    if (!responsibleExists)
                    {
                        return new CellResponseDTO
                        {
                            Id = cell.Id,
                        };
                    }

                    cell.SetResponsible(updatedCell.ResponsibleId.Value);
                }
            }

            await _context.SaveChangesAsync();

            return new CellResponseDTO
            {
                Id = cell.Id,
                Name = cell.Name,
                ChurchId = cell.ChurchId,
            };
        }

        public async Task<CellResponseDTO?> DeleteWithRelationsAsync(long id, long? targetCellId = null)
        {
            var cell = await _context.Cells.FirstOrDefaultAsync(c => c.Id == id);
            if (cell == null) return null;

            var families = await _context.Families.Where(f => f.CellId == id).ToListAsync();

            if (targetCellId.HasValue)
            {
                foreach (var family in families)
                {
                    family.SetCellId(targetCellId.Value);
                }
            }
            else
            {
                cell.SetResponsible(null);

                var familyIds = families.Select(f => f.Id).ToList();
                var members = await _context.Members.Where(m => familyIds.Contains(m.FamilyId)).ToListAsync();
                var memberIds = members.Select(m => m.Id).ToList();

                var users = await _context.Users.Where(u => u.MemberId != null && memberIds.Contains((long)u.MemberId)).ToListAsync();
                var userIds = users.Select(u => u.Id).ToList();
                var userRoles = await _context.UserRoles.Where(ur => userIds.Contains(ur.UserId)).ToListAsync();

                var ministers = await _context.Ministers.Where(m => memberIds.Contains(m.MemberId)).ToListAsync();

                _context.UserRoles.RemoveRange(userRoles);
                _context.Users.RemoveRange(users);
                _context.Ministers.RemoveRange(ministers);
                _context.Members.RemoveRange(members);
                _context.Families.RemoveRange(families);
            }

            _context.Cells.Remove(cell);
            await _context.SaveChangesAsync();

            return new CellResponseDTO { Id = cell.Id, Name = cell.Name };
        }

        public async Task<CellResponseDTO> DeleteAsync(long id)
        {
            var cell = await _context.Cells
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cell == null)
                return new CellResponseDTO
                {
                    Id = 0,
                };

            _context.Cells.Remove(cell);
            await _context.SaveChangesAsync();

            return new CellResponseDTO
            {
                Id = cell.Id,
                Name = cell.Name,
            };
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
