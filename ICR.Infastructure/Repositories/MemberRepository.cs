using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.MemberAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
            return await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .OrderBy(m => m.Id)
                .Skip((page-1) * pageSize)
                .Take(pageSize)
                .Select(m => MapToResponse(m, "sucesso"))
                .ToListAsync();
        }
        public async Task<MemberResponseDTO?> GetByIdAsync(long id)
        {
            return await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .Where(m => m.Id == id)
                .Select(m => MapToResponse(m, "sucesso"))
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<MemberResponseDTO>> GetByFamilyAsync(long familyId)
        {
            return await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .Where(m => m.FamilyId == familyId)
                .Select(m => MapToResponse(m, "sucesso"))
                .ToListAsync();
        }
        public async Task<IEnumerable<MemberResponseDTO>> GetBirthdaysByMonthAsync(int month, long churchId)
        {
            return await _context.Members
                .Include(m => m.Family).ThenInclude(f => f.Church)
                .Include(m => m.Family).ThenInclude(f => f.Cell)
                .Where(m =>
                    m.BirthDate.Month == month &&
                    m.Family != null &&
                    m.Family.ChurchId == churchId
                )
                .Select(m => MapToResponse(m,"sucesso"))
                .ToListAsync();
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

            var newId = await _idSequenceService.GetNextIdAsync<Member>();
            DateTime BirthDateUtc =
                DateTime.SpecifyKind(dto.BirthDate, DateTimeKind.Utc);


            var member = new Member(
                newId,
                dto.FamilyId,
                dto.Name,
                dto.Gender,
                BirthDateUtc,
                dto.HasBeenMarried,
                dto.Role
            );

            member.SetCellPhone(dto.CellPhone);

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
            DateTime? BirthDateUtc = dto.BirthDate.HasValue
                        ? DateTime.SpecifyKind(dto.BirthDate.Value, DateTimeKind.Utc)
                        : null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                member.SetName(dto.Name);

            if (dto.Gender.HasValue)
                member.SetGender(dto.Gender.Value);

            if (dto.BirthDate.HasValue)
                member.SetBirthDate(BirthDateUtc.Value);

            if (dto.HasBeenMarried == true && !member.HasBeenMarried)
                member.MarkAsMarried();

            if (dto.Role != member.Role)
                member.SetRole(dto.Role.Value);

            if (!string.IsNullOrWhiteSpace(dto.CellPhone))
                member.SetCellPhone(dto.CellPhone);

            if (dto.Class != member.Class)
                member.SetClass(dto.Class.Value);

            await _context.SaveChangesAsync();

            return new MemberResponseDTO
            {
                Id = member.Id,
                Name = member.Name,
                Gender = member.Gender,
                BirthDate = member.BirthDate,
                HasBeenMarried = member.HasBeenMarried,
                Role = member.Role,
                CellPhone = member.CellPhone,
                Class = member.Class,
                FamilyId = member.FamilyId,
                FamilyName = member.Family?.Name ?? "",
                FamilyChurchName = member.Family?.Church?.Name ?? "",
                FamilyCellName = member.Family?.Cell?.Name ?? "",
                ResultMessage = "Membro atualizado com sucesso"
            };
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
            return new MemberResponseDTO
            {
                Id = m.Id,
                Name = m.Name,
                Gender = m.Gender,
                BirthDate = m.BirthDate,
                HasBeenMarried = m.HasBeenMarried,
                Role = m.Role,
                CellPhone = m.CellPhone,
                Class = m.Class,
                FamilyId = m.FamilyId,
                FamilyName = m.Family?.Name ?? "",
                FamilyChurchName = m.Family?.Church?.Name ?? "",
                FamilyCellName = m.Family?.Cell?.Name ?? "",
                ResultMessage = message ?? ""
            };
        }
    }
}
