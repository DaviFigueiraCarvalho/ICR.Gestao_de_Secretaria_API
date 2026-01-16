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
        private readonly IdSequenceService _idSequenceService;

        public MemberRepository(ConnectionContext context)
        {
            _context = context;
            _idSequenceService = new IdSequenceService(context);
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

            return members.Select(m => MapToResponse(m, "sucesso"));
        }
        public async Task<MemberResponseDTO?> GetByIdAsync(long id)
        {
            var member = await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .Include(m => m.Family).ThenInclude(f => f.Man)
                .Include(m => m.Family).ThenInclude(f => f.Woman)
                .FirstOrDefaultAsync(m => m.Id == id);

            return member == null ? null : MapToResponse(member, "sucesso");
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

            return members.Select(m => MapToResponse(m, "sucesso"));
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
                .Select(m => MapToResponse(m, "sucesso"))
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
                    ResultMessage = $"A família de ID:{dto.FamilyId} não existe"
                };
            }

            if (dto.CellPhone.Length != 11 || !dto.CellPhone.All(char.IsDigit))
            {
                return new MemberResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"o numero de telefone {dto.CellPhone} é inválido"
                };
            }

            var newId = await _idSequenceService.GetNextIdAsync<Member>();
            var birthDateUtc = DateTime.SpecifyKind(dto.BirthDate, DateTimeKind.Utc);

            // 🔥 CALCULA ANTES
            var calculatedClass = CalculateClass(
                dto.Gender,
                birthDateUtc,
                dto.HasBeenMarried
            );

            var member = new Member(
                newId,
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

            member.SetFamily(family.Id);

            return MapToResponse(member, "Membro criado com sucesso");
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
                    ResultMessage = $"O membro de ID:{id} não existe"
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
                        ResultMessage = $"A família de ID:{dto.FamilyId.Value} não existe"
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
                        ResultMessage = $"o numero de telefone {dto.CellPhone} é inválido"
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
                var recalculatedClass = CalculateClass(
                    finalGender,
                    finalBirthDate,
                    finalHasBeenMarried
                );

                member.SetClass(recalculatedClass);
            }

            await _context.SaveChangesAsync();

            return MapToResponse(member, "Membro atualizado com sucesso");
        }
        public async Task<MemberResponseDTO> RemoveAsync(long id)
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
                    ResultMessage = $"O membro de ID:{id} não existe"
                };
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return MapToResponse(member, $"O membro {member.Name} foi excluído com sucesso");
        }

        // ============================
        // Helper
        // ============================
        private static MemberResponseDTO MapToResponse(Member m, string message)
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
                            // RoleName é calculado automaticamente no DTO

                            FamilyId = m.FamilyId,
                            FamilyName = m.Family?.Name ?? "",
                            FamilyChurchName = m.Family?.Church?.Name ?? "",
                            FamilyCellName = m.Family?.Cell?.Name ?? "",

                            BirthDate = m.BirthDate,
                            HasBeenMarried = m.HasBeenMarried,
        
                            SpouseName = spouseName,
                            WeddingDate = m.Family.WeddingDate,

                            Gender = m.Gender,
                            // GenderName é calculado automaticamente no DTO

                            Class = m.Class,
                            // ClassName é calculado automaticamente no DTO

                            CellPhone = m.CellPhone,

                            ResultMessage = message ?? ""
                        };
        }
        
        private ClassType CalculateClass(
            GenderType gender,
            DateTime birthDate,
            bool hasBeenMarried
        )
        {
            int age = CalculateAge(birthDate);

            if (age <= 2)
                return ClassType.BEBE;

            if (age < 7)
                return ClassType.CRIANCA;

            if (age < 11)
                return ClassType.JUNIORES;

            if (age < 15)
                return ClassType.JUVENIS;

            if (hasBeenMarried)
                return gender == GenderType.HOMEM
                    ? ClassType.HOMENS
                    : ClassType.MULHERES;

            return ClassType.JOVENS;
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }

    }
}
