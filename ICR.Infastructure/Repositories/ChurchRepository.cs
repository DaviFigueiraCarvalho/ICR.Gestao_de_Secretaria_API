using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.MinisterAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ICR.Domain.Model.CellAggregate.Cell;

namespace ICR.Infra.Data.Repositories
{
    public class ChurchRepository : IChurchRepository
    {
        private readonly ConnectionContext _context;

        public ChurchRepository(ConnectionContext context)
        {
            _context = context;
        }

        public async Task<ChurchResponseDto?> CreateAsync(ChurchDTO dto)
        {
            // Busca Federação
            var federation = await _context.Federations
                .FirstOrDefaultAsync(f => f.Id == dto.FederationId);

            if (federation == null)
                throw new KeyNotFoundException($"A federação de ID:{dto.FederationId} não existe");

            // Busca Ministro
            Minister? minister = null;
            long? cellResponsibleId = null;

            if (dto.MinisterId.HasValue && dto.MinisterId.Value > 0)
            {
                minister = await _context.Ministers
                    .Include(m => m.Member)
                    .FirstOrDefaultAsync(m => m.Id == dto.MinisterId.Value);

                if (minister == null)
                    throw new KeyNotFoundException($"O pastor de ID:{dto.MinisterId.Value} não existe");

                // Responsible da célula é o MEMBER do ministro
                cellResponsibleId = minister.Member.Id;
            }
            if (dto.Address.ZipCode.Length != 8 || !dto.Address.ZipCode.All(char.IsDigit))
                throw new ArgumentException($"o CEP:{dto.Address.ZipCode} é inválido. Deve conter exatamente 8 dígitos numéricos");

            var church = new Church(
                0,
                dto.Name,
                dto.Address,
                federation.Id,
                minister?.Id
            );

            var cell = new Cell(
                $"Matriz:({dto.Name})",
                CellType.Celula,
                0,
                cellResponsibleId
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
                MinisterName = minister != null
                    ? $"{minister.Member.Role} {minister.Member.Name}"
                    : null,
            };
        }


        public async Task<ChurchResponseDto?> GetByIdAsync(long id)
        {
            return await _context.Churches
                .AsNoTracking()
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                    .ThenInclude(m => m.Member)
                .OrderBy(c => c.Id)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = AdressDTO.FromEntity(c.Address),
                    FederationId = c.FederationId,
                    FederationName = c.Federation != null ? c.Federation.Name : null,
                    MinisterId = c.MinisterId,
                    MinisterName = c.Minister != null ? c.Minister.Member.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChurchResponseDto>> GetAllChurchsAsync()
        {
            return await _context.Churches
                .AsNoTracking()
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                    .ThenInclude(m => m.Member)
                .OrderBy(c => c.Id)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = AdressDTO.FromEntity(c.Address),
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
                .AsNoTracking()
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                    .ThenInclude(m => m.Member)
                .OrderBy(c => c.Id)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = AdressDTO.FromEntity(c.Address),
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
                    throw new KeyNotFoundException($"A federação de ID:{dto.FederationId.Value} não existe");

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
                        throw new KeyNotFoundException($"O pastor/presbítero de ID:{dto.MinisterId.Value} não existe");

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
            // Address PATCH
            if (dto.Address != null)
            {
                var currentAddress = church.Address;

                var zipCode = dto.Address.ZipCode ?? currentAddress.ZipCode;
                var street = dto.Address.Street ?? currentAddress.Street;
                var number = dto.Address.Number ?? currentAddress.Number;
                var city = dto.Address.City ?? currentAddress.City;
                var state = dto.Address.State ?? currentAddress.State;

                // valida CEP só se vier
                if (dto.Address.ZipCode != null)
                {
                    if (zipCode.Length != 8 || !zipCode.All(char.IsDigit))
                        throw new ArgumentException($"o CEP:{zipCode} é inválido. Deve conter exatamente 8 dígitos numéricos");
                }

                church.SetAddress(new Address(
                    zipCode,
                    street,
                    number,
                    city,
                    state
                ));
            }

            await _context.SaveChangesAsync();

            return new ChurchResponseDto
            {
                Id = church.Id,
                Name = church.Name,
                Address = AdressDTO.FromEntity(church.Address),
                FederationId = federation?.Id,
                FederationName = federation?.Name,
                MinisterId = church.MinisterId,
                MinisterName = minister != null
                    ? $"{minister.Member.Role} {minister.Member.Name}"
                    : null,
            };
        }





        public async Task<ChurchResponseDto?> DeleteAsync(long id)
        {
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => c.Id == id);

            if (church == null)
                throw new KeyNotFoundException($"A igreja de ID:{id} não existe");

            _context.Churches.Remove(church);
            await _context.SaveChangesAsync();

            return new ChurchResponseDto
            {
                Id = church.Id,
                Name = church.Name,
            };
        }


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
