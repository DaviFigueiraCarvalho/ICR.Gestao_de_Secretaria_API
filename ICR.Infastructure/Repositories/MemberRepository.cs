using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.MemberAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ICR.Infra.Data.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly ConnectionContext _context;

        public MemberRepository(ConnectionContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MemberResponseDTO>> GetAllAsync(int page, int pageSize)
        {
            var members = await _context.Members
                .Include(m => m.Family)
                    .ThenInclude(f => f.Church)
                .Include(m => m.Family)
                    .ThenInclude(f => f.Cell)
                .Include(m => m.Family)
                    .ThenInclude(f => f.Man)
                .Include(m => m.Family)
                    .ThenInclude(f => f.Woman)
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return members.Select(m => MapToResponse(m));
        }
        public async Task<MemberResponseDTO?> GetByIdAsync(long id)
        {
            var member = await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .Include(m => m.Family).ThenInclude(f => f.Man)
                .Include(m => m.Family).ThenInclude(f => f.Woman)
                .FirstOrDefaultAsync(m => m.Id == id);

            return member == null ? null : MapToResponse(member);
        }
        public async Task<IEnumerable<MemberResponseDTO>> GetByFamilyAsync(long familyId)
        {
            var members = await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .Include(m => m.Family).ThenInclude(f => f.Man)
                .Include(m => m.Family).ThenInclude(f => f.Woman)
                .Where(m => m.FamilyId == familyId)
                .ToListAsync();

            return members.Select(m => MapToResponse(m));
        }
        public async Task<IEnumerable<MemberResponseDTO>> GetBirthdaysByMonthAsync(int monthNumber, long churchId)
        {
            var members = await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .Include(m => m.Family).ThenInclude(f => f.Man)
                .Include(m => m.Family).ThenInclude(f => f.Woman)
                .Where(m =>
                    m.Family.ChurchId == churchId &&
                    m.BirthDate.Month == monthNumber
                )
                .OrderBy(m => m.BirthDate.Day)
                .ToListAsync();

            return members
                .Select(m => MapToResponse(m))
                .ToList();
        }
        public async Task<MemberResponseDTO> AddAsync(MemberDTO dto)
        {
            var family = await _context.Families
                .Include(f => f.Church)
                .Include(f => f.Cell)
                .FirstOrDefaultAsync(f => f.Id == dto.FamilyId);

            if (family == null)
            {
                return new MemberResponseDTO
                {
                    Id = 0,
                };
            }

            if (dto.CellPhone.Length != 11 || !dto.CellPhone.All(char.IsDigit))
            {
                return new MemberResponseDTO
                {
                    Id = 0,
                };
            }

            var birthDateUtc = DateTime.SpecifyKind(dto.BirthDate, DateTimeKind.Utc);

            // Calcula a classe usando lógica de domínio na entidade
            var calculatedClass = Member.ComputeClass(
                dto.Gender,
                birthDateUtc,
                dto.HasBeenMarried
            );

            var member = new Member(
                dto.FamilyId,
                dto.Name,
                dto.Gender,
                birthDateUtc,
                dto.HasBeenMarried,
                dto.Role,
                dto.CellPhone,
                calculatedClass
            );


            _context.Members.Add(member);
            await _context.SaveChangesAsync();


            var completeMember = await _context.Members
                    .Include(m => m.Family).ThenInclude(f => f.Church)
                    .Include(m => m.Family).ThenInclude(f => f.Cell)
                    .FirstOrDefaultAsync(m => m.Id == member.Id);

            return MapToResponse(completeMember!);
        }
        public async Task<MemberResponseDTO?> UpdateAsync(long id, MemberPatchDTO dto)
        {
            var member = await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return new MemberResponseDTO
                {
                    Id = 0,
                };
            }

            // ============================
            // PREVISÃO DE ESTADO FINAL
            // ============================
            var finalGender = dto.Gender ?? member.Gender;
            var finalBirthDate = dto.BirthDate.HasValue
                ? DateTime.SpecifyKind(dto.BirthDate.Value, DateTimeKind.Utc)
                : member.BirthDate;
            var finalHasBeenMarried =
                dto.HasBeenMarried == true || member.HasBeenMarried;

            bool affectsClass =
                dto.Gender.HasValue ||
                dto.BirthDate.HasValue ||
                dto.HasBeenMarried == true;

            // ============================
            // UPDATES NORMAIS
            // ============================
            if (dto.FamilyId.HasValue && dto.FamilyId.Value != member.FamilyId)
            {
                var family = await _context.Families
                    .Include(f => f.Church)
                    .Include(f => f.Cell)
                    .FirstOrDefaultAsync(f => f.Id == dto.FamilyId.Value);

                if (family == null)
                {
                    return new MemberResponseDTO
                    {
                        Id = member.Id,
                    };
                }

                member.SetFamily(dto.FamilyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                member.SetName(dto.Name);

            if (dto.Gender.HasValue)
                member.SetGender(dto.Gender.Value);

            if (dto.BirthDate.HasValue)
                member.SetBirthDate(finalBirthDate);

            if (dto.HasBeenMarried == true && !member.HasBeenMarried)
                member.MarkAsMarried();

            if (dto.Role.HasValue)
                member.SetRole(dto.Role.Value);

            if (!string.IsNullOrWhiteSpace(dto.CellPhone))
            {
                if (dto.CellPhone.Length != 11 || !dto.CellPhone.All(char.IsDigit))
                {
                    return new MemberResponseDTO
                    {
                        Id = 0,
                    };
                }

                member.SetCellPhone(dto.CellPhone);
            }

            // ============================
            // CLASSE
            // ============================
            if (dto.Class.HasValue)
            {
                member.SetClass(dto.Class.Value);
            }
            else if (affectsClass)
            {
                var recalculatedClass = Member.ComputeClass(
                    finalGender,
                    finalBirthDate,
                    finalHasBeenMarried
                );

                member.SetClass(recalculatedClass);
            }

            await _context.SaveChangesAsync();

            return MapToResponse(member);
        }
        public async Task<MemberResponseDTO> RemoveAsync(long id)
        {
            var member = await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return null;
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return MapToResponse(member);
        }

        // ============================
        // Helper
        // ============================
        private static MemberResponseDTO MapToResponse(Member m)
        {
            string? spouseName = null;

            if (m.HasBeenMarried && m.Family != null)
            {
                if (m.Gender == GenderType.HOMEM)
                {
                    spouseName = m.Family.Woman?.Name;
                }
                else if (m.Gender == GenderType.MULHER)
                {
                    spouseName = m.Family.Man?.Name;
                }
            }

            return new MemberResponseDTO
            {
                Id = m.Id,
                Name = m.Name,
                Role = m.Role,
                FamilyId = m.FamilyId,
                FamilyName = m.Family?.Name ?? "",
                FamilyChurchName = m.Family?.Church?.Name ?? "",
                FamilyCellName = m.Family?.Cell?.Name ?? "",
                BirthDate = m.BirthDate,
                HasBeenMarried = m.HasBeenMarried,
                SpouseName = spouseName,
                WeddingDate = m.Family?.WeddingDate,
                Gender = m.Gender,
                Class = m.Class,
                CellPhone = m.CellPhone,
            };
        }
        

    }
}
