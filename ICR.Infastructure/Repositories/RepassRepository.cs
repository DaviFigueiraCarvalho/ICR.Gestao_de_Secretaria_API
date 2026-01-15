using ICR.Domain.DTOs;
using ICR.Domain.Model.RepassAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Repositories
{
    public class RepassRepository : IRepassRepository
    {
        private readonly ConnectionContext _context;

        public RepassRepository(ConnectionContext context)
        {
            _context = context;
        }

        public async Task<RepassResponseDTO> AddAsync(RepassDTO dto)
        {
            var church = await _context.Churches
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == dto.ChurchId);
            var reference = await _context.References
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == dto.Reference);
            if (church == null)
                return new RepassResponseDTO
                {
                    Id=0,
                    ResultMessage = $"A igreja de ID:{dto.ChurchId} não existe"
                };

            var repass = new Repass(
                id: 0,
                churchId: dto.ChurchId,
                reference: dto.Reference,
                amount: dto.Amount
            );

            _context.Repasses.Add(repass);
            await _context.SaveChangesAsync();

            return new RepassResponseDTO
            {
                Id = repass.Id,
                ChurchId = repass.ChurchId,
                Reference = repass.ReferenceId,
                ReferenceName = reference.Name,
                Amount = repass.Amount
            };
        }

        public async Task<RepassResponseDTO?> GetByIdAsync(long id)
        {
            var repass = await _context.Repasses
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
            var reference = await _context.References
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (repass == null)
                return null;

            return new RepassResponseDTO
            {
                Id = repass.Id,
                ChurchId = repass.ChurchId,
                Reference = repass.ReferenceId,
                Amount = repass.Amount
            };
        }

        public async Task<IEnumerable<RepassResponseDTO>> GetAllAsync(int pageNumber, int pageQuantity)
        {
            return await _context.Repasses
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
                .Select(r => new RepassResponseDTO
                {
                    Id = r.Id,
                    ChurchId = r.ChurchId,
                    Reference = r.ReferenceId,
                    Amount = r.Amount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<RepassResponseDTO>> GetByChurchIdAsync(long churchId)
        {
            return await _context.Repasses
               .AsNoTracking()
               .Include(r => r.Church)
               .Where(r => r.ChurchId == churchId)
               .Select(r => new RepassResponseDTO
               {
                   Id = r.Id,
                   ChurchId = r.ChurchId,
                   ChurchName = r.Church.Name,
                   Reference = r.ReferenceId,
                   Amount = r.Amount
               })
               .ToListAsync();
        }

        public async Task<IEnumerable<RepassResponseDTO>> GetByReferenceIdAsync(long reference)
        {
            return await _context.Repasses
                .AsNoTracking()
                .Include(r => r.Church)
                .Where(r => r.ReferenceId == reference)
                .Select(r => new RepassResponseDTO
                {
                    Id = r.Id,
                    ChurchId = r.ChurchId,
                    ChurchName = r.Church.Name,
                    Reference = r.ReferenceId,
                    Amount = r.Amount
                })
                .ToListAsync();
        }

        public async Task<RepassResponseDTO> UpdateAsync(long id, RepassUpdateDTO dto)
        {
            var repass = await _context.Repasses
                .FirstOrDefaultAsync(r => r.Id == id);

            if (repass == null)
            {
                return new RepassResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"o repasse de ID:{id} não existe"
                };
            }

            // ChurchId só atualiza se vier e se existir
            if (dto.ChurchId.HasValue)
            {
                var churchExists = await _context.Churches
                    .AnyAsync(c => c.Id == dto.ChurchId.Value);

                if (!churchExists)
                {
                    return new RepassResponseDTO
                    {
                        Id = repass.Id,
                        ResultMessage = $"a church de ID:{dto.ChurchId.Value} não existe"
                    };
                }

                repass.SetChurchId(dto.ChurchId.Value);
            }

            // Reference opcional
            if (dto.Reference.HasValue)
                repass.SetReference(dto.Reference.Value);

            // Amount opcional
            if (dto.Amount.HasValue)
                repass.SetAmount(dto.Amount.Value);

            await _context.SaveChangesAsync();

            return new RepassResponseDTO
            {
                Id = repass.Id,
                ChurchId = repass.ChurchId,
                Reference = repass.ReferenceId,
                Amount = repass.Amount
            };
        }


        public async Task<RepassResponseDTO> DeleteAsync(long id)
        {
            var repass = await _context.Repasses
                .FirstOrDefaultAsync(r => r.Id == id);

            if (repass == null)
                throw new KeyNotFoundException("Repass not found");

            _context.Repasses.Remove(repass);
            await _context.SaveChangesAsync();

            return new RepassResponseDTO
            {
                Id = repass.Id,
                ChurchId = repass.ChurchId,
                Reference = repass.ReferenceId,
                Amount = repass.Amount
            };
        }


        public async Task<Reference?> GetReferenceByIdAsync(long id)
        {
            // Busca a referência pelo ID, sem tracking porque não vamos alterar nada, óbvio
            return await _context.References
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reference>> GetAllReferencesAsync()
        {
            // Retorna tudo, simples, direto, sem frescura
            return await _context.References
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .ToListAsync();
        }



    }
}
