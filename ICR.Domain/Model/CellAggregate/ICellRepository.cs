using ICR.Domain.DTOs;
using ICR.Domain.Model.MemberAggregate;

using System.Collections.Generic;

namespace ICR.Domain.Model.CellAggregate
{
    public interface ICellRepository
    {
        Task<CellResponseDTO> AddAsync(CellDTO cell);

        Task<CellResponseDTO?> GetByIdAsync(long id);

        Task<IEnumerable<CellResponseDTO>> GetAllAsync();

        Task<IEnumerable<CellResponseDTO>> GetFilteredAsync(long? federationId, long? churchId);

        Task<IEnumerable<CellResponseDTO>> GetByChurchIdAsync(Member leader);

        Task<CellResponseDTO> DeleteAsync(long id);
        Task<CellResponseDTO?> DeleteWithRelationsAsync(long id, long? targetCellId = null);
        Task<CellResponseDTO> UpdateAsync(long id, CellPatchDTO updatedCell);

        Task SaveAsync();
    }

}