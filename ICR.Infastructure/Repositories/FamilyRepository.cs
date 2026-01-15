using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.MemberAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Data.Repositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly ConnectionContext _context;
        private readonly IdSequenceService _idSequenceService;

        public FamilyRepository(ConnectionContext context)
        {
            _context = context;
            _idSequenceService = new IdSequenceService(context);
        }

        public async Task<ResponseFamilyDTO> AddAsync(FamilyDTO dto)
        {
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => c.Id == dto.ChurchId);

            if (church == null)
                return new ResponseFamilyDTO
                {
                    Id = 0,
                    ResultMessage = $"A igreja de ID:{dto.ChurchId} não existe"
                };

            var cell = await _context.Cells
                .FirstOrDefaultAsync(c => c.Id == dto.CellId);

            if (cell == null)
                return new ResponseFamilyDTO
                {
                    Id = 0,
                    ResultMessage = $"A célula de ID:{dto.CellId} não existe"
                };

            Member? man = null;
            Member? woman = null;

            if (dto.ManId.HasValue)
            {
                man = await _context.Members.FirstOrDefaultAsync(m => m.Id == dto.ManId);
                if (man == null)
                    return new ResponseFamilyDTO
                    {
                        Id = 0,
                        ResultMessage = $"O membro de ID:{dto.ManId} não existe"
                    };
            }

            if (dto.WomanId.HasValue)
            {
                woman = await _context.Members.FirstOrDefaultAsync(m => m.Id == dto.WomanId);
                if (woman == null)
                    return new ResponseFamilyDTO
                    {
                        Id = 0,
                        ResultMessage = $"o membro de ID:{dto.WomanId} não existe"
                    };
            }
            DateTime? weddingDateUtc = dto.WeddingDate.HasValue
            ? DateTime.SpecifyKind(dto.WeddingDate.Value, DateTimeKind.Utc)
            : null;

            var familyId = await _idSequenceService.GetNextIdAsync<Family>();
            var family = new Family
            (
                familyId,
                dto.Name,
                church.Id,
                cell.Id,
                dto.ManId,
                dto.WomanId,
                weddingDateUtc
            );

            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            return new ResponseFamilyDTO
            {
                Id = familyId,
                Name = family.Name,
                churchId = church.Id,
                ChurchName = church.Name,
                CellId = cell.Id,
                CellName = cell.Name,
                ManId = man?.Id,
                ManName = man?.Name,
                WomanId = woman?.Id,
                WomanName = woman?.Name,
                WeddingDate = family.WeddingDate,
                ResultMessage = "Família criada com sucesso"
            };
        }
        public async Task<ResponseFamilyDTO?> GetByIdAsync(long id)
        {
            return await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Include(f => f.Man)
                .Include(f => f.Woman)
                .Where(f => f.Id == id)
                .Select(f => new ResponseFamilyDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    churchId = f.ChurchId,
                    ChurchName = f.Church.Name,
                    CellId = f.CellId,
                    CellName = f.Cell.Name,
                    ManId = f.ManId,
                    ManName = f.Man != null ? f.Man.Name : null,
                    WomanId = f.WomanId,
                    WomanName = f.Woman != null ? f.Woman.Name : null,
                    WeddingDate = f.WeddingDate
                })
                .FirstOrDefaultAsync();
        }
        public async Task<List<ResponseFamilyDTO>> GetAsync(int page, int pageSize)
        {
            return await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .OrderBy(f => f.Id)
                .Skip((page- 1) * pageSize)
                .Take(pageSize)
                .Select(f => new ResponseFamilyDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    churchId = f.ChurchId,
                    ChurchName = f.Church.Name,
                    CellId = f.CellId,
                    CellName = f.Cell.Name,
                    WeddingDate = f.WeddingDate
                })
                .ToListAsync();
        }
        public async Task<List<ResponseFamilyDTO>> GetFamiliesByWeddingBirthdayMonthAsync(int monthNumber)
        {
            return await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Where(f => f.WeddingDate.HasValue && f.WeddingDate.Value.Month == monthNumber)
                .Select(f => new ResponseFamilyDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    churchId = f.ChurchId,
                    ChurchName = f.Church.Name,
                    CellId = f.CellId,
                    CellName = f.Cell.Name,
                    WeddingDate = f.WeddingDate
                })
                .ToListAsync();
        }
        public async Task<List<ResponseFamilyDTO>> GetByChurchId(long churchId)
        {
            return await _context.Families
                .Include(f => f.Cell)
                .Where(f => f.ChurchId == churchId)
                .Select(f => new ResponseFamilyDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    churchId = f.ChurchId,
                    CellId = f.CellId,
                    CellName = f.Cell.Name
                })
                .ToListAsync();
        }
        public async Task<List<ResponseFamilyDTO>> GetByCellIdAsync(long cellId)
        {
            return await _context.Families
                .Include(f => f.Cell)
                .Where(f => f.CellId == cellId)
                .Select(f => new ResponseFamilyDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    churchId = f.ChurchId,
                    CellId = f.CellId,
                    CellName = f.Cell.Name
                })
                .ToListAsync();
        }
        public async Task<ResponseFamilyDTO> UpdateAsync(long id, FamilyPatchDTO familyUpdated)
        {
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == id);
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => family.ChurchId == id);
            var cell = await _context.Cells
                .FirstOrDefaultAsync(c => family.CellId == id);
            var man = family.ManId.HasValue
                ? await _context.Members.FirstOrDefaultAsync(m => m.Id == family.ManId.Value)
                : null;
            var woman = family.WomanId.HasValue
                ? await _context.Members.FirstOrDefaultAsync(m => m.Id == family.WomanId.Value)
                : null;

            if (family == null)
            {
                return new ResponseFamilyDTO
                {
                    Id = 0,
                    ResultMessage = $"A família de ID:{id} não existe"
                };
            }

            // Name
            if (!string.IsNullOrWhiteSpace(familyUpdated.Name))
                family.SetName(familyUpdated.Name);

            // ChurchId
            if (familyUpdated.ChurchId.HasValue &&
                familyUpdated.ChurchId.Value != family.ChurchId)
            {
                if (familyUpdated.ChurchId.Value <= 0)
                {
                    return new ResponseFamilyDTO
                    {
                        Id = family.Id,
                        ResultMessage = "ChurchId inválido"
                    };
                }

                var churchExists = await _context.Churches
                    .AnyAsync(c => c.Id == familyUpdated.ChurchId.Value);

                if (!churchExists)
                {
                    return new ResponseFamilyDTO
                    {
                        Id = family.Id,
                        ResultMessage = $"A igreja de ID:{familyUpdated.ChurchId.Value} não existe"
                    };
                }

                family.SetChurchId(familyUpdated.ChurchId.Value);
            }


            // CellId
            if (familyUpdated.CellId.HasValue &&
                familyUpdated.CellId.Value != family.CellId)
            {
                if (familyUpdated.CellId.Value <= 0)
                {
                    return new ResponseFamilyDTO
                    {
                        Id = family.Id,
                        ResultMessage = "CellId inválido"
                    };
                }

                var cellExists = await _context.Cells
                    .AnyAsync(c => c.Id == familyUpdated.CellId.Value);

                if (!cellExists)
                {
                    return new ResponseFamilyDTO
                    {
                        Id = family.Id,
                        ResultMessage = $"A célula de ID:{familyUpdated.CellId.Value} não existe"
                    };
                }

                family.SetCellId(familyUpdated.CellId.Value);
            }


            // Pai
            if (familyUpdated.ManId.HasValue && familyUpdated.ManId != family.ManId)
            {
                if (familyUpdated.ManId.Value == 0)
                {
                    family.SetFatherId(null);
                }
                else
                {
                    var manExists = await _context.Members
                        .AnyAsync(m => m.Id == familyUpdated.ManId.Value);

                    if (!manExists)
                    {
                        return new ResponseFamilyDTO
                        {
                            Id = family.Id,
                            ResultMessage = $"O membro (pai) de ID:{familyUpdated.ManId.Value} não existe"
                        };
                    }

                    family.SetFatherId(familyUpdated.ManId.Value);
                }
            }

            // Mãe
            if (familyUpdated.WomanId.HasValue && familyUpdated.WomanId != family.WomanId)
            {
                if (familyUpdated.WomanId.Value == 0)
                {
                    family.SetMotherId(null);
                }
                else
                {
                    var womanExists = await _context.Members
                        .AnyAsync(m => m.Id == familyUpdated.WomanId.Value);

                    if (!womanExists)
                    {
                        return new ResponseFamilyDTO
                        {
                            Id = family.Id,
                            ResultMessage = $"O membro (mãe) de ID:{familyUpdated.WomanId.Value} não existe"
                        };
                    }

                    family.SetMotherId(familyUpdated.WomanId.Value);
                }
            }

            // WeddingDate
            DateTime? weddingDateUtc = familyUpdated.WeddingDate.HasValue
            ? DateTime.SpecifyKind(familyUpdated.WeddingDate.Value, DateTimeKind.Utc)
            : null;
            if (familyUpdated.WeddingDate.HasValue)
                family.SetWeddingDate(weddingDateUtc.Value);

            await _context.SaveChangesAsync();

            return new ResponseFamilyDTO
            {
                Id = family.Id,
                Name = family.Name,
                churchId = church.Id,
                ChurchName = church.Name,
                CellId = cell.Id,
                CellName = cell.Name,
                ManId = man?.Id,
                ManName = man?.Name,
                WomanId = woman?.Id,
                WomanName = woman?.Name,
                WeddingDate = family.WeddingDate,
                ResultMessage = $"Família {family.Name} atualizada com sucesso"
            };
        }
        public async Task<ResponseFamilyDTO> DeleteAsync(long id)
        {
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == id);

            if (family == null)
                return new ResponseFamilyDTO
                {
                    Id = 0,
                    ResultMessage = $"A família de ID:{id} não existe"
                };

            _context.Families.Remove(family);
            await _context.SaveChangesAsync();

            return new ResponseFamilyDTO
            {
                Id = family.Id,
                Name = family.Name,
                ResultMessage = $"Família {family.Name} deletada com sucesso"
            };
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
