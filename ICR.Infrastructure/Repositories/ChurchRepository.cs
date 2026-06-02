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

                cellResponsibleId = minister.Member.Id;
            }

            // Validar e criar Address
            Address address;
            try
            {
                address = new Address(
                    dto.Address.CountryCode,
                    dto.Address.PostalCode,
                    dto.Address.Street,
                    dto.Address.Number,
                    dto.Address.City,
                    dto.Address.State,
                    dto.Address.Complement,
                    dto.Address.CountyOrRegion
                );
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Endereço inválido: {ex.Message}", ex);
            }

            var church = new Church(
                dto.Name,
                address,
                federation.Id,
                minister?.Id
            );
            _context.Churches.Add(church);
            await _context.SaveChangesAsync();

            var cell = new Cell(
                $"Matriz:({dto.Name})",
                CellType.Celula,
                church.Id,
                cellResponsibleId
            );

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
                .Where(c => c.IsActive)
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                    .ThenInclude(m => m.Member)
                .OrderBy(c => c.Id)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = AddressDTO.FromEntity(c.Address),
                    FederationId = c.FederationId,
                    FederationName = c.Federation != null ? c.Federation.Name : null,
                    MinisterId = c.MinisterId,
                    MinisterName = c.Minister != null ? c.Minister.Member.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChurchResponseDto>> GetAllChurchesAsync(int page = 1, int pageSize = 50, string? search = null)
        {
            var query = _context.Churches
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                    .ThenInclude(m => m.Member)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
            }

            return await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = AddressDTO.FromEntity(c.Address),
                    FederationId = c.FederationId,
                    FederationName = c.Federation != null ? c.Federation.Name : null,
                    MinisterId = c.MinisterId,
                    MinisterName = c.Minister != null ? c.Minister.Member.Name : null
                })
                .ToListAsync();
        }


        public async Task<IEnumerable<ChurchResponseDto>> GetChurchesbyFederationId(long id)
        {
            return await _context.Churches
                .AsNoTracking()
                .Where(c => c.IsActive && c.FederationId == id)
                .Include(c => c.Federation)
                .Include(c => c.Minister)
                    .ThenInclude(m => m.Member)
                .OrderBy(c => c.Id)
                .Select(c => new ChurchResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = AddressDTO.FromEntity(c.Address),
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

            // Address PATCH
            if (dto.Address != null)
            {
                try
                {
                    var countryCode = dto.Address.CountryCode ?? church.Address.Country.Code;
                    var postalCode = dto.Address.PostalCode ?? church.Address.PostalCode;
                    var street = dto.Address.Street ?? church.Address.Street;
                    var number = dto.Address.Number ?? church.Address.Number;
                    var city = dto.Address.City ?? church.Address.City;
                    var state = dto.Address.State ?? church.Address.State;
                    var complement = dto.Address.Complement ?? church.Address.Complement;
                    var countyOrRegion = dto.Address.CountyOrRegion ?? church.Address.CountyOrRegion;

                    var newAddress = new Address(
                        countryCode,
                        postalCode,
                        street,
                        number,
                        city,
                        state,
                        complement,
                        countyOrRegion
                    );

                    church.SetAddress(newAddress);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Endereço inválido: {ex.Message}", ex);
                }
            }

            await _context.SaveChangesAsync();

            return new ChurchResponseDto
            {
                Id = church.Id,
                Name = church.Name,
                Address = AddressDTO.FromEntity(church.Address),
                FederationId = federation?.Id,
                FederationName = federation?.Name,
                MinisterId = church.MinisterId,
                MinisterName = minister != null
                    ? $"{minister.Member.Role} {minister.Member.Name}"
                    : null,
            };
        }



        public async Task<ChurchResponseDto?> DeactivateAsync(long id)
        {
            var church = await _context.Churches
                .FirstOrDefaultAsync(c => c.Id == id);

            if (church == null)
                return null;

            // Apenas desativa a igreja, mantendo histórico de repasses
            church.Deactivate();
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
