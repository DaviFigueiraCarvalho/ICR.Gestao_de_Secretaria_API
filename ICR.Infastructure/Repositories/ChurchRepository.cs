using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.MinisterAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Data.Repositories
{
    public class ChurchRepository : IChurchRepository
    {
        private readonly ConnectionContext _context;
        private readonly IdSequenceService _idSequenceService;

        public ChurchRepository(ConnectionContext context)
        {
            _context = context;
            _idSequenceService = new IdSequenceService(context);
        }

        public async Task<ChurchResponseDto?> CreateAsync(ChurchDTO dto)
        {
            // Busca Federação
            var federation = await _context.Federations
                .FirstOrDefaultAsync(f => f.Id == dto.FederationId);

            if (federation == null)
                return new ChurchResponseDto
                {
                    Id = 0,
                    ResultMessage = $"A federação de ID:{dto.FederationId} não existe"
                };

            // Busca Ministro
            Minister? minister = null;

            if (dto.MinisterId.HasValue && dto.MinisterId.Value > 0)
            {
                minister = await _context.Ministers
                    .FirstOrDefaultAsync(m => m.Id == dto.MinisterId.Value);

                if (minister == null)
                    return new ChurchResponseDto
                    {
                        Id = 0,
                        ResultMessage = $"O pastor de ID:{dto.MinisterId} não existe"
                    };
            }

            var newId = await _idSequenceService.GetNextIdAsync<Church>();
            var newCellId = await _idSequenceService.GetNextIdAsync<Cell>();

            var church = new Church(
                newId,
                dto.Name,
                dto.Address,
                federation.Id,
                minister?.Id
            );

            var cell = new Cell(
                newCellId,
                $"Matriz:({dto.Name})",
                newId,
                0
            );

            _context.Churches.Add(church);
            _context.Cells.Add(cell);

            await _context.SaveChangesAsync();

            return new ChurchResponseDto
            {
                Id = church.Id,
                Name = church.Name,
                FederationId = federation.Id,
                FederationName = federation.Name,
                MinisterId = minister?.Id,
                MinisterName =$"{minister?.Member.Role} {minister?.Member.Name}",
                ResultMessage = "Igreja criada com sucesso"
            };
        }


        public async Task<ChurchResponseDto?> GetByIdAsync(long id)
        {
            return await _context.Churches
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                .Where(c => c.Id == id)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    FederationId = c.FederationId,
                    FederationName = c.Federation != null ? c.Federation.Name : null,
                    MinisterId = c.MinisterId,
                    MinisterName = c.Minister != null ? c.Minister.Member.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChurchResponseDto>> GetAllChurchsAsync(int pageNumber, int pageQuantity)
        {
            return await _context.Churches
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    FederationId = c.FederationId,
                    FederationName = c.Federation != null ? c.Federation.Name : null,
                    MinisterId = c.MinisterId,
                    MinisterName = c.Minister != null ? c.Minister.Member.Name : null
                })
                .ToListAsync();
        }


        public async Task<IEnumerable<ChurchResponseDto>> GetChurchsbyFederationId(long id)
        {
            return await _context.Churches
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                .Where(c => c.FederationId == id)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    FederationId = c.FederationId,
                    FederationName = c.Federation != null ? c.Federation.Name : null,
                    MinisterId = c.MinisterId,
                    MinisterName = c.Minister != null ? c.Minister.Member.Name : null
                })
                .ToListAsync();
        }


        public async Task<ChurchResponseDto?> UpdateAsync(long id, ChurchPatchDTO dto)
        {
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => c.Id == id);

            if (church == null)
                return null;

            Federation? federation = null;

            // FederationId (só se vier)
            if (dto.FederationId.HasValue)
            {
                federation = await _context.Federations
                    .FirstOrDefaultAsync(f => f.Id == dto.FederationId.Value);

                if (federation == null)
                {
                    return new ChurchResponseDto
                    {
                        Id = 0,
                        ResultMessage = $"A federação de ID:{dto.FederationId.Value} não existe"
                    };
                }

                church.SetFederationId(federation.Id);
            }
            else
            {
                federation = await _context.Federations
                    .FirstOrDefaultAsync(f => f.Id == church.FederationId);
            }

            Minister? minister = null;

            // MinisterId (só se vier)
            if (dto.MinisterId.HasValue)
            {
                if (dto.MinisterId.Value == 0)
                {
                    church.SetMinisterId(null);
                }
                else
                {
                    minister = await _context.Ministers
                        .Include(m => m.Member)
                        .FirstOrDefaultAsync(m => m.Id == dto.MinisterId.Value);

                    if (minister == null)
                    {
                        return new ChurchResponseDto
                        {
                            Id = church.Id,
                            ResultMessage = $"O pastor/presbítero de ID:{dto.MinisterId.Value} não existe"
                        };
                    }

                    church.SetMinisterId(minister.Id);
                }
            }
            else if (church.MinisterId.HasValue)
            {
                minister = await _context.Ministers
                    .Include(m => m.Member)
                    .FirstOrDefaultAsync(m => m.Id == church.MinisterId.Value);
            }

            // Name
            if (!string.IsNullOrWhiteSpace(dto.Name))
                church.SetName(dto.Name);

            // Address
            if (dto.Address != null)
                church.SetAddress(dto.Address);

            await _context.SaveChangesAsync();

            return new ChurchResponseDto
            {
                Id = church.Id,
                Name = church.Name,
                Address = church.Address,
                FederationId = federation?.Id,
                FederationName = federation?.Name,
                MinisterId = church.MinisterId,
                MinisterName = minister != null
                    ? $"{minister.Member.Role} {minister.Member.Name}"
                    : null,
                ResultMessage = $"Igreja {church.Name} atualizada com sucesso"
            };
        }





        public async Task<ChurchResponseDto?> DeleteAsync(long id)
        {
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => c.Id == id);

            if (church == null)
                return new ChurchResponseDto
                {
                    Id = 0,
                    ResultMessage = $"A igreja de ID:{id} não existe"
                };

            _context.Churches.Remove(church);
            await _context.SaveChangesAsync();

            return new ChurchResponseDto
            {
                Id = church.Id,
                Name = church.Name,
                ResultMessage = $"Igreja {church.Name} deletada com sucesso"

            };
        }


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
