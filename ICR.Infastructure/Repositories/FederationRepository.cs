using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.FederationAggregate;
using ICRManagement.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Repositories
{
    public class FederationRepository : IFederationRepository
    {
        private readonly ConnectionContext _context;
        private readonly IdSequenceService _idSequenceService;

        public FederationRepository(ConnectionContext context)
        {
            _context = context;
            _idSequenceService = new IdSequenceService(_context);
        }

        

        // CREATE
        public async Task<FederationResponseDTO> AddAsync(FederationDTO dto)
        {
            
            long newId = await _idSequenceService.GetNextIdAsync<Federation>();

            var federation = new Federation(newId, dto.Name, dto.MinisterId);

            await _context.Federations.AddAsync(federation);
            await _context.SaveChangesAsync();

            return new FederationResponseDTO
            {
                Id = federation.Id,
                Name = federation.Name,
                MinisterId = federation.MinisterId,
                ResultMessage = "Federação criada com sucesso."
            };
        }

        // READ DTO
        public async Task<FederationResponseDTO?> GetByIdAsync(long id)
        {
            return await _context.Federations
                .Include(f => f.Minister)
                .Where(f => f.Id == id)
                .Select(f => new FederationResponseDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    MinisterId = f.MinisterId,
                    MinisterName = f.Minister != null ? f.Minister.Member.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FederationResponseDTO>> GetAllFederationsAsync()
        {
            return await _context.Federations
                .Include(f => f.Minister)
                .Select(f => new FederationResponseDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    MinisterId = f.MinisterId,
                    MinisterName = f.Minister != null ? f.Minister.Member.Name : null
                })
                .ToListAsync();
        }

        // UPDATE usando métodos da entidade
        public async Task<FederationResponseDTO> UpdateAsync(long id, FederationPatchDTO dto)
        {
            var federation = await _context.Federations
                .FirstOrDefaultAsync(f => f.Id == id);

            if (federation == null)
            {
                return new FederationResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"Federação de ID:{id} não encontrada."
                };
            }

            string? ministerName = null;

            // PATCH de MinisterId
            if (dto.MinisterId.HasValue)
            {
                if (dto.MinisterId.Value == 0)
                {
                    // remove ministro
                    federation.SetMinisterId(null);
                }
                else
                {
                    var minister = await _context.Ministers
                        .Include(m => m.Member)
                        .FirstOrDefaultAsync(m => m.Id == dto.MinisterId.Value);

                    if (minister == null)
                    {
                        return new FederationResponseDTO
                        {
                            Id = federation.Id,
                            ResultMessage = $"O pastor/presbítero de ID:{dto.MinisterId.Value} não existe"
                        };
                    }

                    federation.SetMinisterId(minister.Id);
                    ministerName = minister.Member.Name;
                }
            }

            // PATCH de Name
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                federation.SetName(dto.Name);
            }

            await SaveAsync();

            return new FederationResponseDTO
            {
                Id = federation.Id,
                Name = federation.Name,
                MinisterId = federation.MinisterId,
                MinisterName = ministerName,
                ResultMessage = $"Federação {federation.Name} atualizada com sucesso."
            };
        }


        // DELETE
        public async Task<FederationResponseDTO> DeleteAsync(long id)
        {
            var federation = await _context.Federations
                .FirstOrDefaultAsync(c => c.Id == id); ;
            if (federation == null)
                return new FederationResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"Federação de ID:{id} não encontrada."
                };

            _context.Federations.Remove(federation);
            await SaveAsync();
            return new FederationResponseDTO
            {
                ResultMessage = $"Federação {federation.Name}, deletada com sucesso."
            };
        }

        // Salva alterações no banco
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
