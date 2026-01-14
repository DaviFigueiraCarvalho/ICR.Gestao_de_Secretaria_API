using ICR.Domain.DTOs;
using ICR.Domain.Model.MemberAggregate;

public interface IMemberRepository
{
    Task<IEnumerable<MemberResponseDTO>> GetAllAsync();
    Task<MemberResponseDTO?> GetByIdAsync(long id);
    Task<IEnumerable<MemberResponseDTO>> GetByFamilyAsync(long familyId);
    Task<IEnumerable<MemberResponseDTO>> GetBirthdaysByMonthAsync(int month, long churchId);

    Task<MemberResponseDTO> AddAsync(Member member);
    Task<MemberResponseDTO> UpdateAsync(long id, MemberDTO member);
    Task<MemberResponseDTO> RemoveAsync(long id);
}
