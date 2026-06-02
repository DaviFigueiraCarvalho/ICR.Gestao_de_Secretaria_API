using ICR.Domain.DTOs;

public interface IChurchRepository
{
    Task<ChurchResponseDto> CreateAsync(ChurchDTO dto);
    Task<ChurchResponseDto?> UpdateAsync(long id, ChurchPatchDTO dto);
    Task<ChurchResponseDto?> DeactivateAsync(long id);

    Task<IEnumerable<ChurchResponseDto>> GetAllChurchesAsync(int page = 1, int pageSize = 50, string? search = null);
    Task<ChurchResponseDto?> GetByIdAsync(long id);
    Task<IEnumerable<ChurchResponseDto>> GetChurchesbyFederationId(long federationId);
}
