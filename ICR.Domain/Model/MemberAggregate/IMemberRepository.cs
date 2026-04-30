using ICR.Domain.DTOs;
using ICR.Domain.Model.MemberAggregate;

public interface IMemberRepository
{
    Task<IEnumerable<MemberResponseDTO>> GetAllAsync(int page, int pageSize);
    Task<MemberResponseDTO?> GetByIdAsync(long id);
    Task<IEnumerable<MemberResponseDTO>> GetByFamilyAsync(long familyId);
    Task<IEnumerable<MemberResponseDTO>> GetBirthdaysByMonthAsync(int month, long churchId);
    Task<IEnumerable<MemberResponseDTO>> GetFilteredAsync(long? federationId, long? churchId, long? cellId);
    Task<IEnumerable<MemberClassCountDTO>> GetClassCountsAsync(long? federationId, long? churchId, long? cellId);
    Task<IEnumerable<MemberRoleCountDTO>> GetRoleCountsAsync(long? federationId, long? churchId, long? cellId);

    Task<MemberResponseDTO> AddAsync(MemberDTO member);
    Task<MemberResponseDTO> UpdateAsync(long id, MemberPatchDTO member);
    Task<MemberResponseDTO> RemoveAsync(long id);
}
