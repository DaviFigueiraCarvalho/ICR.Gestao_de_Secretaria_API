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

        public async Task<ResponseFamilyDTO> AddAsync(Family family)
        {
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => c.Id == family.ChurchId);

            if (church == null)
                return new ResponseFamilyDTO
                {
                    Id = 0,
                    ResultMessage = $"A igreja de ID:{family.ChurchId} não existe"
                };

            var cell = await _context.Cells
                .FirstOrDefaultAsync(c => c.Id == family.CellId);

            if (cell == null)
                return new ResponseFamilyDTO
                {
                    Id = 0,
                    ResultMessage = $"A célula de ID:{family.CellId} não existe"
                };

            Member? man = null;
            Member? woman = null;

            if (family.ManId.HasValue)
            {
                man = await _context.Members.FirstOrDefaultAsync(m => m.Id == family.ManId);
                if (man == null)
                    return new ResponseFamilyDTO
                    {
                        Id = 0,
                        ResultMessage = $"O membro de ID:{family.ManId} não existe"
                    };
            }

            if (family.WomanId.HasValue)
            {
                woman = await _context.Members.FirstOrDefaultAsync(m => m.Id == family.WomanId);
                if (woman == null)
                    return new ResponseFamilyDTO
                    {
                        Id = 0,
                        ResultMessage = $"o membro de ID:{family.WomanId} não existe"
                    };
            }

            family.Id = await _idSequenceService.GetNextIdAsync<Family>();

            _context.Families.Add(family);
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

        public async Task<List<ResponseFamilyDTO>> GetAsync(int pageNumber, int pageQuantity)
        {
            return await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .OrderBy(f => f.Id)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
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

        public async Task<ResponseFamilyDTO> UpdateAsync(long id, Family familyUpdated)
        {
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == id);

            if (family == null)
                return new ResponseFamilyDTO
                {
                    Id = 0,
                    ResultMessage = $"A família de ID:{id} não existe"
                };

            if (!string.IsNullOrWhiteSpace(familyUpdated.Name))
                family.SetName(familyUpdated.Name);

            family.SetChurchId(familyUpdated.ChurchId);
            family.SetCellId(familyUpdated.CellId);
            family.SetFatherId(familyUpdated.ManId);
            family.SetMotherId(familyUpdated.WomanId);
            family.SetWeddingDate(familyUpdated.WeddingDate);

            await _context.SaveChangesAsync();

            return new ResponseFamilyDTO
            {
                Id = family.Id,
                Name = family.Name,
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
