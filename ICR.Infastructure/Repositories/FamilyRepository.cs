using ICR.Application.Services;
using ICR.Domain.DTOs;
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

        // =========================
        // MAPPER CENTRALIZADO
        // =========================
        private static ResponseFamilyDTO MapToResponse(Family family, string? message = null)
        {
            return new ResponseFamilyDTO
            {
                Id = family.Id,
                Name = family.Name,
                churchId = family.ChurchId,
                ChurchName = family.Church?.Name,
                CellId = family.CellId,
                CellName = family.Cell?.Name,
                ManId = family.ManId,
                ManName = family.Man?.Name,
                WomanId = family.WomanId,
                WomanName = family.Woman?.Name,
                WeddingDate = family.WeddingDate,
                ResultMessage = message
            };
        }

        // =========================
        // ADD
        // =========================
        public async Task<ResponseFamilyDTO> AddAsync(FamilyDTO dto)
        {
            var church = await _context.Churches.FirstOrDefaultAsync(c => c.Id == dto.ChurchId);
            if (church == null)
                return new ResponseFamilyDTO { Id = 0, ResultMessage = $"A igreja de ID:{dto.ChurchId} não existe" };

            var cell = await _context.Cells.FirstOrDefaultAsync(c => c.Id == dto.CellId);
            if (cell == null)
                return new ResponseFamilyDTO { Id = 0, ResultMessage = $"A célula de ID:{dto.CellId} não existe" };

            if (dto.ManId.HasValue &&
                !await _context.Members.AnyAsync(m => m.Id == dto.ManId.Value))
                return new ResponseFamilyDTO { Id = 0, ResultMessage = $"O membro (pai) de ID:{dto.ManId} não existe" };

            if (dto.WomanId.HasValue &&
                !await _context.Members.AnyAsync(m => m.Id == dto.WomanId.Value))
                return new ResponseFamilyDTO { Id = 0, ResultMessage = $"O membro (mãe) de ID:{dto.WomanId} não existe" };

            DateTime? weddingDateUtc = dto.WeddingDate.HasValue
                ? DateTime.SpecifyKind(dto.WeddingDate.Value, DateTimeKind.Utc)
                : null;

            var familyId = await _idSequenceService.GetNextIdAsync<Family>();

            var family = new Family(
                familyId,
                dto.Name,
                dto.ChurchId,
                dto.CellId,
                dto.ManId,
                dto.WomanId,
                weddingDateUtc
            );

            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            var saved = await LoadFamilyAsync(familyId);
            return MapToResponse(saved, "Família criada com sucesso");
        }

        // =========================
        // GET BY ID
        // =========================
        public async Task<ResponseFamilyDTO?> GetByIdAsync(long id)
        {
            var family = await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Include(f => f.Man)
                .Include(f => f.Woman)
                .FirstOrDefaultAsync(f => f.Id == id);

            return family == null ? null : MapToResponse(family);
        }

        // =========================
        // GET PAGINADO
        // =========================
        public async Task<List<ResponseFamilyDTO>> GetAsync(int pageNumber, int pageQuantity)
        {
            var families = await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Include(f => f.Man)
                .Include(f => f.Woman)
                .OrderBy(f => f.Id)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
                .ToListAsync();

            return families.Select(f => MapToResponse(f)).ToList();
        }

        // =========================
        // GET POR MÊS DE CASAMENTO
        // =========================
        public async Task<List<ResponseFamilyDTO>> GetFamiliesByWeddingBirthdayMonthAsync(int monthNumber)
        {
            var families = await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Include(f => f.Man)
                .Include(f => f.Woman)
                .Where(f => f.WeddingDate.HasValue &&
                            f.WeddingDate.Value.Month == monthNumber)
                .ToListAsync();

            return families.Select(f => MapToResponse(f)).ToList();
        }

        // =========================
        // GET POR IGREJA
        // =========================
        public async Task<List<ResponseFamilyDTO>> GetByChurchId(long churchId)
        {
            var families = await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Include(f => f.Man)
                .Include(f => f.Woman)
                .Where(f => f.ChurchId == churchId)
                .ToListAsync();

            return families.Select(f => MapToResponse(f)).ToList();
        }

        // =========================
        // GET POR CÉLULA
        // =========================
        public async Task<List<ResponseFamilyDTO>> GetByCellIdAsync(long cellId)
        {
            var families = await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Include(f => f.Man)
                .Include(f => f.Woman)
                .Where(f => f.CellId == cellId)
                .ToListAsync();

            return families.Select(f => MapToResponse(f)).ToList();
        }

        // =========================
        // UPDATE
        // =========================
        public async Task<ResponseFamilyDTO> UpdateAsync(long id, FamilyPatchDTO dto)
        {
            var family = await _context.Families.FirstOrDefaultAsync(f => f.Id == id);
            if (family == null)
                return new ResponseFamilyDTO { Id = 0, ResultMessage = $"A família de ID:{id} não existe" };

            if (!string.IsNullOrWhiteSpace(dto.Name))
                family.SetName(dto.Name);

            if (dto.ChurchId.HasValue && dto.ChurchId != family.ChurchId)
                family.SetChurchId(dto.ChurchId.Value);

            if (dto.CellId.HasValue && dto.CellId != family.CellId)
                family.SetCellId(dto.CellId.Value);

            if (dto.ManId.HasValue)
                family.SetFatherId(dto.ManId == 0 ? null : dto.ManId);

            if (dto.WomanId.HasValue)
                family.SetMotherId(dto.WomanId == 0 ? null : dto.WomanId);

            if (dto.WeddingDate.HasValue)
                family.SetWeddingDate(DateTime.SpecifyKind(dto.WeddingDate.Value, DateTimeKind.Utc));

            await _context.SaveChangesAsync();

            var updated = await LoadFamilyAsync(id);
            return MapToResponse(updated, $"Família {updated.Name} atualizada com sucesso");
        }

        // =========================
        // DELETE
        // =========================
        public async Task<ResponseFamilyDTO> DeleteAsync(long id)
        {
            var family = await _context.Families.FirstOrDefaultAsync(f => f.Id == id);
            if (family == null)
                return new ResponseFamilyDTO { Id = 0, ResultMessage = $"A família de ID:{id} não existe" };

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

        // =========================
        // LOAD COMPLETO
        // =========================
        private async Task<Family?> LoadFamilyAsync(long id)
        {
            return await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .Include(f => f.Man)
                .Include(f => f.Woman)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
    }
}
