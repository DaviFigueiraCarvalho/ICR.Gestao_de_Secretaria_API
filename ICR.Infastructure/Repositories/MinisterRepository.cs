using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.MemberAggregate;
using ICR.Domain.Model.MinisterAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Data.Repositories
{
    public class MinisterRepository : IMinisterRepository
    {
        private readonly ConnectionContext _context;
        private readonly IdSequenceService _idSequenceService;

        public MinisterRepository(ConnectionContext context)
        {
            _context = context;
            _idSequenceService = new IdSequenceService(context);
        }

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
            {
                return new MinisterResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"O membro de ID:{dto.MemberId} não existe"
                };
            }

            var newId = await _idSequenceService.GetNextIdAsync<Minister>();
            DateTime CardValidityUtc =
                DateTime.SpecifyKind(dto.CardValidity, DateTimeKind.Utc);
            DateTime PresbiterOrdinationDateUtc =
                DateTime.SpecifyKind(dto.PresbiterOrdinationDate, DateTimeKind.Utc);
            DateTime? MinisterOrdinationDateUtc = dto.MinisterOrdinationDate.HasValue
                ? DateTime.SpecifyKind(dto.MinisterOrdinationDate.Value, DateTimeKind.Utc)
                : null;


            var minister = new Minister(
                id: newId,
                memberId: dto.MemberId,
                cpf: dto.Cpf,
                email: dto.Email,
                cardValidity: CardValidityUtc,
                presbiterOrdinationDate: PresbiterOrdinationDateUtc,
                ministerOrdinationDate: MinisterOrdinationDateUtc,
                address: dto.Address
                );

            _context.Ministers.Add(minister);
            await _context.SaveChangesAsync();

            return new MinisterResponseDTO
            {
                Id = minister.Id,
                MemberId = member.Id,
                MemberName = member.Name,
                ChurchMemberName = member.Family?.Church?.Name ?? string.Empty,
                FederationMemberName = member.Family?.Church?.Federation?.Name ?? string.Empty,
                MemberBirthday = member.BirthDate,
                MemberWifeName = member.Family?.Woman?.Name ?? string.Empty,
                MemberWeddingDate = member.Family?.WeddingDate ?? DateTime.MinValue,
                Cpf = minister.Cpf,
                Email = minister.Email,
                CardValidity = minister.CardValidity,
                PresbiterOrdinationDate = minister.PresbiterOrdinationDate,
                MinisterOrdinationDate = minister.MinisterOrdinationDate,
                Address = AdressDTO.FromEntity(minister.Address),
                ResultMessage = "Ministro criado com sucesso"
            };
        }
        public async Task<MinisterResponseDTO?> GetByIdAsync(long id)
        {
            return await _context.Ministers
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Church)
                            .ThenInclude(c => c.Federation)
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Woman)
                .Where(m => m.Id == id)
                .Select(m => new MinisterResponseDTO
                {
                    Id = m.Id,
                    MemberId = m.Member!.Id,
                    MemberName = m.Member.Name,
                    ChurchMemberName = m.Member.Family!.Church!.Name,
                    FederationMemberName = m.Member.Family.Church!.Federation!.Name,
                    MemberBirthday = m.Member.BirthDate,
                    MemberWifeName = m.Member.Family.Woman != null
                        ? m.Member.Family.Woman.Name
                        : string.Empty,
                    MemberWeddingDate = m.Member.Family.WeddingDate ?? DateTime.MinValue,
                    Cpf = m.Cpf,
                    Email = m.Email,
                    CardValidity = m.CardValidity,
                    PresbiterOrdinationDate = m.PresbiterOrdinationDate,
                    MinisterOrdinationDate = m.MinisterOrdinationDate,
                    Address = AdressDTO.FromEntity(m.Address),
                })
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<MinisterResponseDTO>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Ministers
                .AsNoTracking()
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Church)
                            .ThenInclude(c => c.Federation)
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Woman)
                .OrderBy(m => m.Id)
                .Skip((page-1)*pageSize)
                .Take(pageSize)

                .Select(m => new MinisterResponseDTO
                {
                    Id = m.Id,
                    MemberId = m.Member!.Id,
                    MemberName = m.Member.Name,
                    ChurchMemberName = m.Member.Family!.Church!.Name,
                    FederationMemberName = m.Member.Family.Church!.Federation!.Name,
                    MemberBirthday = m.Member.BirthDate,
                    MemberWifeName = m.Member.Family.Woman != null
                        ? m.Member.Family.Woman.Name
                        : string.Empty,
                    MemberWeddingDate = m.Member.Family.WeddingDate ?? DateTime.MinValue,
                    Cpf = m.Cpf,
                    Email = m.Email,
                    CardValidity = m.CardValidity,
                    PresbiterOrdinationDate = m.PresbiterOrdinationDate,
                    MinisterOrdinationDate = m.MinisterOrdinationDate,
                    Address = AdressDTO.FromEntity(m.Address),
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<MinisterResponseDTO>> GetByChurchIdAsync(long churchId)
        {
            return await _context.Ministers
                .AsNoTracking()
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Church)
                            .ThenInclude(c => c.Federation)
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Woman)
                .Where(m =>
                    m.Member != null &&
                    m.Member.Family != null &&
                    m.Member.Family.ChurchId == churchId
                )
                .Select(m => new MinisterResponseDTO
                {
                    Id = m.Id,
                    MemberId = m.Member!.Id,
                    MemberName = m.Member.Name,
                    ChurchMemberName = m.Member.Family!.Church!.Name,
                    FederationMemberName = m.Member.Family.Church!.Federation!.Name,
                    MemberBirthday = m.Member.BirthDate,
                    MemberWifeName = m.Member.Family.Woman != null
                        ? m.Member.Family.Woman.Name
                        : string.Empty,
                    MemberWeddingDate = m.Member.Family.WeddingDate ?? DateTime.MinValue,
                    Cpf = m.Cpf,
                    Email = m.Email,
                    CardValidity = m.CardValidity,
                    PresbiterOrdinationDate = m.PresbiterOrdinationDate,
                    MinisterOrdinationDate = m.MinisterOrdinationDate,
                    Address = AdressDTO.FromEntity(m.Address),
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<MinisterBirthdayDTO>> GetByBirthdaydatesIdAsync(int month)
        {
            if (month < 1 || month > 12)
                return new List<MinisterBirthdayDTO>();

            var ministers = await _context.Ministers
                .AsNoTracking()
                .Include(m => m.Member)
                    .ThenInclude(mem => mem.Family)
                        .ThenInclude(f => f.Woman)
                .ToListAsync();

            var result = new List<MinisterBirthdayDTO>();

            foreach (var minister in ministers)
            {
                var member = minister.Member;
                var family = member?.Family;

                if (member == null || family == null)
                    continue;

                // 🎂 Aniversário do ministro
                if (member.BirthDate.Month == month)
                {
                    result.Add(new MinisterBirthdayDTO
                    {
                        Name = member.Name,
                        Type = "BIRTHDAY",
                        MemberWifeName = null,
                        Birthday = member.BirthDate
                    });
                }

                // 💍 Aniversário de casamento
                if (family.WeddingDate.HasValue && family.WeddingDate.Value.Month == month)
                {
                    result.Add(new MinisterBirthdayDTO
                    {
                        Name = member.Name,
                        Type = "WEDDING",
                        MemberWifeName = family.Woman?.Name,
                        Birthday = family.WeddingDate.Value
                    });
                }
            }

            // 🔢 ordena pelo DIA do evento
            return result
                .OrderBy(r => r.Birthday.Day)
                .ToList();
        }
        public async Task<MinisterResponseDTO> UpdateAsync(long id, MinisterPatchDTO dto)
        {
            var existing = await _context.Ministers
                .Include(m => m.Member)
                .ThenInclude(m => m.Family)
                .ThenInclude(f => f.Church)
                .ThenInclude(c => c.Federation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existing == null)
                return new MinisterResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"O ministro de ID:{id} não existe"
                };
            DateTime? CardValidityUtc = dto.MinisterOrdinationDate.HasValue
                ? DateTime.SpecifyKind(dto.MinisterOrdinationDate.Value, DateTimeKind.Utc)
                : null;

            DateTime? PresbiterOrdinationDateUtc = dto.MinisterOrdinationDate.HasValue
                ? DateTime.SpecifyKind(dto.MinisterOrdinationDate.Value, DateTimeKind.Utc)
                : null;

            DateTime? MinisterOrdinationDateUtc = dto.MinisterOrdinationDate.HasValue
                ? DateTime.SpecifyKind(dto.MinisterOrdinationDate.Value, DateTimeKind.Utc)
                : null;


            // ===== PATCH GRANULAR DE VERDADE =====
            if (dto.MemberId.HasValue)
                existing.SetMemberId(dto.MemberId.Value);

            if (dto.Cpf.HasValue)
                existing.SetCpf(dto.Cpf.Value);

            if (CardValidityUtc.HasValue)
                existing.SetCardValidity(dto.CardValidity.Value);

            if (PresbiterOrdinationDateUtc.HasValue)
                existing.SetPresbiterOrdinationDate(dto.PresbiterOrdinationDate.Value);

            if (MinisterOrdinationDateUtc.HasValue)
                existing.SetMinisterOrdinationDate(dto.MinisterOrdinationDate);

            if (!string.IsNullOrWhiteSpace(dto.Email))
                existing.SetEmail(dto.Email);

            if (dto.Address != null)
                existing.SetMinisterAddress(dto.Address);

            await _context.SaveChangesAsync();

            return new MinisterResponseDTO
            {
                Id = existing.Id,
                MemberId = existing.MemberId,
                MemberName = existing.Member?.Name ?? string.Empty,
                ChurchMemberName = existing.Member?.Family?.Church?.Name ?? string.Empty,
                FederationMemberName = existing.Member?.Family?.Church?.Federation?.Name ?? string.Empty,
                MemberBirthday = existing.Member?.BirthDate ?? DateTime.MinValue,
                MemberWifeName = existing.Member?.Family?.Woman?.Name ?? string.Empty,
                MemberWeddingDate = existing.Member?.Family?.WeddingDate ?? DateTime.MinValue,
                Cpf = existing.Cpf,
                Email = existing.Email,
                CardValidity = existing.CardValidity,
                PresbiterOrdinationDate = existing.PresbiterOrdinationDate,
                MinisterOrdinationDate = existing.MinisterOrdinationDate,
                Address = AdressDTO.FromEntity(existing.Address),
                ResultMessage = "Ministro atualizado com sucesso"
            };
        }
        public async Task<MinisterResponseDTO> DeleteAsync(long id)
        {
            var minister = await _context.Ministers
                .Include(m => m.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (minister == null)
            {
                return new MinisterResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"O ministro de ID:{id} não existe"
                };
            }

            _context.Ministers.Remove(minister);
            await _context.SaveChangesAsync();

            return new MinisterResponseDTO
            {
                Id = minister.Id,
                ResultMessage = "Ministro removido com sucesso"
            };
        }

        
    }
}
