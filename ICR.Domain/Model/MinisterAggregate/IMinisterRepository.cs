using ICR.Domain.DTOs;
using System.Collections.Generic;

namespace ICR.Domain.Model.MinisterAggregate
{
    public interface IMinisterRepository
    {
        Task<MinisterResponseDTO> AddAsync(MinisterDTO minister);
        Task<MinisterResponseDTO?> GetByIdAsync(long id);
        Task<IEnumerable<MinisterResponseDTO>> GetAllAsync(int page, int pageNumber);
        Task<IEnumerable<MinisterResponseDTO>> GetByChurchIdAsync(long churchId); 
        Task<IEnumerable<MinisterBirthdayDTO>> GetByBirthdaydatesIdAsync(int month);
        Task<MinisterResponseDTO> UpdateAsync(long id, MinisterPatchDTO dto);
        Task<MinisterResponseDTO> DeleteAsync(long id);

    }
}