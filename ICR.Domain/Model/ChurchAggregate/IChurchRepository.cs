using ICR.Domain.DTOs;

public interface IChurchRepository
{
    Task<ChurchResponseDto> CreateAsync(ChurchDTO dto);
    Task<ChurchResponseDto?> UpdateAsync(long id, ChurchPatchDTO dto);
    Task<ChurchResponseDto> DeleteAsync(long id);

    Task<IEnumerable<ChurchResponseDto>> GetAllChurchesAsync();
    Task<ChurchResponseDto?> GetByIdAsync(long id);
    Task<IEnumerable<ChurchResponseDto>> GetChurchesbyFederationId(long federationId);
}
