using ICR.Domain.DTOs;
using ICR.Domain.Model;
using ICR.Domain.Model.MinisterAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Data.Repositories
{
    public class MinisterRepository : IMinisterRepository
    {
        private readonly ConnectionContext _context;

        public MinisterRepository(ConnectionContext context)
        {
            _context = context;
        }

        // ============================
        // BASE QUERY PADRÃO
        // ============================
        private IQueryable<Minister> BaseQuery()
        {
            return _context.Ministers
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Church)
                            .ThenInclude(c => c.Federation)
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Woman);
        }

        // ============================
        // MAPPER CENTRAL
        // ============================
        private static MinisterResponseDTO MapToResponse(Minister m)
        {
            var member = m.Member;
            var family = member?.Family;

            return new MinisterResponseDTO
            {
                Id = m.Id,
                MemberId = member?.Id ?? 0,
                MemberName = member?.Name ?? string.Empty,
                ChurchMemberName = family?.Church?.Name ?? string.Empty,
                FederationMemberName = family?.Church?.Federation?.Name ?? string.Empty,
                MemberBirthday = member?.BirthDate ?? DateOnly.MinValue,
                MemberPhone = PhoneResponseDTO.FromEntity(member?.CellPhone),
                MemberWifeName = family?.Woman?.Name ?? string.Empty,
                MemberWeddingDate = family?.WeddingDate ?? DateOnly.MinValue,
                Cpf = m.Cpf,
                Email = m.Email,
                Insurance = m.Insurance,
                CardValidity = m.CardValidity,
                PresbiterOrdinationDate = m.PresbiterOrdinationDate,
                MinisterOrdinationDate = m.MinisterOrdinationDate,
                Address = AddressDTO.FromEntity(m.Address)
            };
        }

        private static bool CalculateInsurance(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age))
                age--;

            return age < 75;
        }

        // ============================
        // ADD
        // ============================
        public async Task<MinisterResponseDTO> AddAsync(MinisterDTO dto)
        {
            var member = await _context.Members
                .Include(m => m.Family)
                    .ThenInclude(f => f.Church)
                        .ThenInclude(c => c.Federation)
                .Include(m => m.Family)
                    .ThenInclude(f => f.Woman)
                .FirstOrDefaultAsync(m => m.Id == dto.MemberId);

            if (member == null)
                return new MinisterResponseDTO { Id = 0 };

            // Validar CPF (não vazio e apenas dígitos)
            if (string.IsNullOrWhiteSpace(dto.Cpf) || !dto.Cpf.All(char.IsDigit))
                throw new ArgumentException("CPF inválido");

            // Validar e criar Address se fornecido
            Address? address = null;
            if (dto.Address != null)
            {
                // Verificar se há ao menos um campo preenchido
                bool hasAddressData = !string.IsNullOrWhiteSpace(dto.Address.CountryCode) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.PostalCode) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.Street) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.Number) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.City) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.State);

                if (hasAddressData)
                {
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
                        throw new ArgumentException($"Erro ao validar endereço: {ex.Message}", ex);
                    }
                }
            }

            var minister = new Minister(
                dto.MemberId,
                dto.Cpf,
                dto.Email,
                dto.CardValidity,
                dto.PresbiterOrdinationDate,
                dto.MinisterOrdinationDate,
                address,
                CalculateInsurance(member.BirthDate)
            );

            _context.Ministers.Add(minister);
            await _context.SaveChangesAsync();

            return MapToResponse(minister);
        }

        // ============================
        // GET BY ID
        // ============================
        public async Task<MinisterResponseDTO?> GetByIdAsync(long id)
        {
            var minister = await BaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            return minister == null ? null : MapToResponse(minister);
        }

        // ============================
        // GET ALL
        // ============================
        public async Task<IEnumerable<MinisterResponseDTO>> GetAllAsync(int page, int pageSize, string? search = null)
        {
            var query = BaseQuery()
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Member.Name.ToLower().Contains(search.ToLower()));
            }

            var ministers = await query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return ministers.Select(m => MapToResponse(m));
        }

        public async Task<IEnumerable<MinisterInsuredListDTO>> GetInsuredAsync()
        {
            var query = BaseQuery()
                .AsNoTracking()
                .Where(m => m.Insurance);

            var ministers = await query
                .OrderBy(m => m.Member!.Name)
                .ToListAsync();

            return ministers.Select(m => new MinisterInsuredListDTO
            {
                FullName = m.Member?.Name ?? string.Empty,
                BirthDate = m.Member?.BirthDate ?? DateOnly.MinValue,
                Cpf = m.Cpf,
                Email = m.Email,
                Phone = PhoneResponseDTO.FromEntity(m.Member?.CellPhone),
                Insurance = m.Insurance
            });
        }

        // ============================
        // GET BY CHURCH
        // ============================
        public async Task<IEnumerable<MinisterResponseDTO>> GetByChurchIdAsync(long churchId)
        {
            var ministers = await BaseQuery()
                .AsNoTracking()
                .Where(m =>
                    m.Member != null &&
                    m.Member.Family != null &&
                    m.Member.Family.ChurchId == churchId
                )
                .ToListAsync();

            return ministers.Select(m => MapToResponse(m));
        }

        // ============================
        // BIRTHDAYS
        // ============================
        public async Task<IEnumerable<MinisterBirthdayDTO>> GetBirthdaysByMonthAsync(int month)
        {
            if (month < 1 || month > 12)
                return new List<MinisterBirthdayDTO>();

            var ministers = await BaseQuery()
                .AsNoTracking()
                .Where(m => m.Member != null && m.Member.BirthDate.Month == month)
                .OrderBy(m => m.Member!.BirthDate.Day)
                .ToListAsync();

            return ministers.Select(m => new MinisterBirthdayDTO
            {
                Name = m.Member!.Name,
                Type = "BIRTHDAY",
                Phone = PhoneResponseDTO.FromEntity(m.Member.CellPhone),
                Birthday = m.Member.BirthDate
            }).ToList();
        }

        // ============================
        // WEDDING ANNIVERSARIES
        // ============================
        public async Task<IEnumerable<MinisterBirthdayDTO>> GetWeddingAnniversariesByMonthAsync(int month)
        {
            if (month < 1 || month > 12)
                return new List<MinisterBirthdayDTO>();

            var ministers = await BaseQuery()
                .AsNoTracking()
                .Where(m => m.Member != null &&
                            m.Member.Family != null &&
                            m.Member.Family.WeddingDate.HasValue &&
                            m.Member.Family.WeddingDate.Value.Month == month)
                .OrderBy(m => m.Member!.Family!.WeddingDate!.Value.Day)
                .ToListAsync();

            return ministers.Select(m => new MinisterBirthdayDTO
            {
                Name = m.Member!.Name,
                Type = "WEDDING",
                Phone = PhoneResponseDTO.FromEntity(m.Member.CellPhone),
                MemberWifeName = m.Member.Family!.Woman?.Name,
                Birthday = m.Member.Family.WeddingDate!.Value
            }).ToList();
        }

        // ============================
        // UPDATE
        // ============================
        public async Task<MinisterResponseDTO> UpdateAsync(long id, MinisterPatchDTO dto)
        {
            var minister = await BaseQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (minister == null)
                return new MinisterResponseDTO { Id = 0 };

            if (dto.MemberId.HasValue && dto.MemberId.Value != minister.MemberId)
            {
                var member = await _context.Members
                    .Include(m => m.Family)
                        .ThenInclude(f => f.Church)
                            .ThenInclude(c => c.Federation)
                    .Include(m => m.Family)
                        .ThenInclude(f => f.Woman)
                    .FirstOrDefaultAsync(m => m.Id == dto.MemberId.Value);

                if (member == null)
                    return new MinisterResponseDTO { Id = minister.Id };

                minister.SetMemberId(member.Id);
                minister.SetInsurance(CalculateInsurance(member.BirthDate));
                minister.Member = member;
            }

            if (!string.IsNullOrWhiteSpace(dto.Cpf))
                minister.SetCpf(dto.Cpf);

            if (!string.IsNullOrWhiteSpace(dto.Email))
                minister.SetEmail(dto.Email);

            if (dto.CardValidity.HasValue)
                minister.SetCardValidity(dto.CardValidity.Value);

            if (dto.PresbiterOrdinationDate.HasValue)
                minister.SetPresbiterOrdinationDate(
                    dto.PresbiterOrdinationDate.Value
                );

            if (dto.MinisterOrdinationDate.HasValue)
                minister.SetMinisterOrdinationDate(
                    dto.MinisterOrdinationDate.Value
                );

            if (dto.Address != null)
            {
                // Verificar se há ao menos um campo preenchido
                bool hasAddressData = !string.IsNullOrWhiteSpace(dto.Address.CountryCode) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.PostalCode) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.Street) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.Number) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.City) ||
                                      !string.IsNullOrWhiteSpace(dto.Address.State);

                if (hasAddressData)
                {
                    try
                    {
                        var address = new Address(
                            dto.Address.CountryCode,
                            dto.Address.PostalCode,
                            dto.Address.Street,
                            dto.Address.Number,
                            dto.Address.City,
                            dto.Address.State,
                            dto.Address.Complement,
                            dto.Address.CountyOrRegion
                        );
                        minister.SetMinisterAddress(address);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"Erro ao validar endereço: {ex.Message}", ex);
                    }
                }
                else
                {
                    // Se o objeto Address foi enviado mas está vazio, limpar o endereço
                    minister.SetMinisterAddress(null);
                }
            }

            if (dto.Insurance.HasValue)
                minister.SetInsurance(dto.Insurance.Value);

            await _context.SaveChangesAsync();

            return MapToResponse(minister);
        }

        // ============================
        // DELETE
        // ============================
        public async Task<MinisterResponseDTO> DeleteAsync(long id)
        {
            var minister = await _context.Ministers.FirstOrDefaultAsync(m => m.Id == id);

            if (minister == null)
                return new MinisterResponseDTO { Id = 0 };

            _context.Ministers.Remove(minister);
            await _context.SaveChangesAsync();

            return new MinisterResponseDTO { Id = minister.Id };
        }
    }
}
