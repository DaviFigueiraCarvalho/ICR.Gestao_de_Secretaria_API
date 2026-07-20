using ICR.Domain.DTOs;
using ICR.Domain.Model.MemberAggregate;

using System.Collections.Generic;

namespace ICR.Domain.Model.CellAggregate
{
    public interface ICellRepository
    {
        Task<CellResponseDTO> AddAsync(CellDTO cell);

        Task<CellResponseDTO?> GetByIdAsync(long id);

        Task<IEnumerable<CellResponseDTO>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null);

        Task<IEnumerable<CellResponseDTO>> GetFilteredAsync(
            long? federationId,
            long? churchId,
            int page = 1,
            int pageSize = 50,
            string? search = null);

        Task<IEnumerable<CellResponseDTO>> GetByChurchIdAsync(Member leader);

        Task<CellResponseDTO> DeleteAsync(long id);
        Task<CellResponseDTO?> DeleteWithRelationsAsync(long id, long? targetCellId = null);
        Task<CellResponseDTO> UpdateAsync(long id, CellPatchDTO updatedCell);

        Task SaveAsync();
    }

}
