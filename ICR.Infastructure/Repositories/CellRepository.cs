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
        private readonly IdSequenceService _idSequenceService;

        public CellRepository(ConnectionContext context)
        {
            _context = context;
            _idSequenceService = new IdSequenceService(context);
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
                    ResultMessage = $"A igreja de ID:{dto.ChurchId} não existe"
                };

            // Busca responsável
            Member? responsible = null;

            if (dto.ResponsibleId.HasValue && dto.ResponsibleId.Value > 0)
            {
                responsible = await _context.Members
                    .FirstOrDefaultAsync(m => m.Id == dto.ResponsibleId.Value);

                if (responsible == null)
                    return new CellResponseDTO
                    {
                        Id = 0,
                        ResultMessage = $"O responsável de ID:{dto.ResponsibleId} não existe"
                    };
            }

            var newId = await _idSequenceService.GetNextIdAsync<Cell>();

            var cell = new Cell(
                newId,
                dto.Name,
                church.Id,
                responsible?.Id ?? 0
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
                ResultMessage = "Célula criada com sucesso"
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

        public async Task<IEnumerable<CellResponseDTO>> GetAllAsync(int pageNumber, int pageQuantity)
        {
            return await _context.Cells
                .Include(c => c.Church)
                .Include(c => c.Responsible)
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
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

        public async Task<CellResponseDTO> UpdateAsync(long id, Cell updatedCell)
        {
            var cell = await _context.Cells
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cell == null)
                return new CellResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"A célula de ID:{id} não existe"
                };

            if (!string.IsNullOrWhiteSpace(updatedCell.Name))
                cell.SetName(updatedCell.Name);

            if (updatedCell.ResponsibleId.HasValue)
                cell.SetResponsible(updatedCell.ResponsibleId.Value);

            await _context.SaveChangesAsync();

            return new CellResponseDTO
            {
                Id = cell.Id,
                Name = cell.Name,
                ChurchId = cell.ChurchId,
                ResultMessage = $"Célula {cell.Name} atualizada com sucesso"
            };
        }

        public async Task<CellResponseDTO> DeleteAsync(long id)
        {
            var cell = await _context.Cells
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cell == null)
                return new CellResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"A célula de ID:{id} não existe"
                };

            _context.Cells.Remove(cell);
            await _context.SaveChangesAsync();

            return new CellResponseDTO
            {
                Id = cell.Id,
                Name = cell.Name,
                ResultMessage = $"Célula {cell.Name} deletada com sucesso"
            };
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
