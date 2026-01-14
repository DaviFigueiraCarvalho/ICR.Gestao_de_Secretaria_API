using ICR.Domain.DTOs;

public interface IChurchRepository
{
    Task<ChurchResponseDto> CreateAsync(ChurchDTO dto);
    Task<ChurchResponseDto?> UpdateAsync(long id, ChurchPatchDTO dto);
    Task<ChurchResponseDto> DeleteAsync(long id);

    Task<IEnumerable<ChurchResponseDto>> GetAllChurchsAsync(int pageNumber, int pageQuantity);
    Task<ChurchResponseDto?> GetByIdAsync(long id);
    Task<IEnumerable<ChurchResponseDto>> GetChurchsbyFederationId(long federationId);
}
