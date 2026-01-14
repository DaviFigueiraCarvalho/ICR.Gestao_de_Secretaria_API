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

        public async Task<IEnumerable<MemberResponseDTO>> GetAllAsync()
        {
            return await _context.Members
                .Include(m => m.Family)
                .OrderBy(m => m.Id)
                .Select(m => new MemberResponseDTO
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
                    FamilyName = m.Family != null ? m.Family.Name : "",
                })
                .ToListAsync();
        }

        public async Task<MemberResponseDTO?> GetByIdAsync(long id)
        {
            return await _context.Members
                .Include(m => m.Family)
                .Where(m => m.Id == id)
                .Select(m => new MemberResponseDTO
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
                    FamilyName = m.Family != null ? m.Family.Name : "",
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberResponseDTO>> GetByFamilyAsync(long familyId)
        {
            return await _context.Members
                .Include(m => m.Family)
                .Where(m => m.FamilyId == familyId)
                .Select(m => new MemberResponseDTO
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
                    FamilyName = m.Family != null ? m.Family.Name : "",
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MemberResponseDTO>> GetBirthdaysByMonthAsync(int month, long churchId)
        {
            return await _context.Members
                .Include(m => m.Family)
                .Where(m =>
                    m.BirthDate.Month == month &&
                    m.Family != null &&
                    m.Family.ChurchId == churchId
                )
                .Select(m => new MemberResponseDTO
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
                    FamilyName = m.Family != null ? m.Family.Name : "",
                })
                .ToListAsync();
        }

        public async Task<MemberResponseDTO> AddAsync(Member member)
        {
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == member.FamilyId);

            if (family == null)
            {
                return new MemberResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"A família de ID:{member.FamilyId} não existe"
                };
            }

            var newId = await _idSequenceService.GetNextIdAsync<Member>();
            member.Id = newId;

            _context.Members.Add(member);
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
                FamilyId = family.Id,
                FamilyName = family.Name,
                ResultMessage = "Membro criado com sucesso"
            };
        }

        public async Task<MemberResponseDTO?> UpdateAsync(long id, MemberDTO dto)
        {
            var member = await _context.Members
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
                return new MemberResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"O Membro de ID:{dto.FamilyId} não existe"
                };
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == dto.FamilyId);

            if (family == null)
            {
                return new MemberResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"A família de ID:{dto.FamilyId} não existe"
                };
            }

            if (dto.Name != null)
                member.SetName(dto.Name);

            if (dto.Gender != null)
                member.SetGender(dto.Gender);

            if (dto.BirthDate!= null)
                member.SetBirthDate(dto.BirthDate);

            if (dto.HasBeenMarried == true)
                member.MarkAsMarried();

            if (dto.Role != null)
                member.SetRole(dto.Role);

            if (dto.CellPhone.HasValue)
                member.SetCellPhone(dto.CellPhone);


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
                FamilyId = family.Id,
                FamilyName = family.Name,
                ResultMessage = "Membro atualizado com sucesso"
            };
        }

        public async Task<MemberResponseDTO> RemoveAsync(long id)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
                return new MemberResponseDTO
                {
                    Id = 0,
                    ResultMessage = $"O Membro de ID:{id} não existe"
                };

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return new MemberResponseDTO
            {
                Id = id,
                ResultMessage = $"O Membro {member.Name} Foi Excluido com Sucesso"
            }; ;
        }
    }
}
