using ICR.Domain.DTOs;
using System.Collections.Generic;

namespace ICR.Domain.Model.RepassAggregate
{
    public interface IRepassRepository
    {
        Task<RepassResponseDTO> AddAsync(RepassDTO repass);
        Task<RepassResponseDTO?> GetByIdAsync(long id);
        Task<IEnumerable<RepassResponseDTO>> GetAllAsync(int pageNumber, int pageQuantity);
        Task<IEnumerable<RepassResponseDTO>> GetByChurchIdAsync(long churchId);
        Task<IEnumerable<RepassResponseDTO>> GetByReferenceIdAsync(long reference);
        Task<RepassResponseDTO> UpdateAsync(long id, RepassUpdateDTO dto);
        Task<RepassResponseDTO> DeleteAsync(long id);
        Task<Reference?> GetReferenceByIdAsync(long id);
        Task<IEnumerable<Reference>> GetAllReferencesAsync();

    }
}