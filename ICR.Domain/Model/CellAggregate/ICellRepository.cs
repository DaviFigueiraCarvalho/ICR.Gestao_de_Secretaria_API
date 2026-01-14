using ICR.Domain.DTOs;
using ICR.Domain.Model.MemberAggregate;

using System.Collections.Generic;

namespace ICR.Domain.Model.CellAggregate
{
    public interface ICellRepository
    {
        Task<CellResponseDTO> AddAsync(CellDTO cell);

        Task<CellResponseDTO?> GetByIdAsync(long id);

        Task<IEnumerable<CellResponseDTO>> GetAllAsync(int pageNumber, int pageQuantity);

        Task<IEnumerable<CellResponseDTO>> GetByChurchIdAsync(Member leader);

        Task<CellResponseDTO> DeleteAsync(long id);
        Task<CellResponseDTO> UpdateAsync(long id, CellPatchDTO updatedCell);

        Task SaveAsync();
    }

}