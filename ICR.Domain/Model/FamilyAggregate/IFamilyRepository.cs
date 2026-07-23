using ICR.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.Domain.Model.FamilyAggregate
{
    public interface IFamilyRepository
    {
        Task<ResponseFamilyDTO> AddAsync(FamilyDTO familyDTO);

        Task<ResponseFamilyDTO?> GetByIdAsync(long id);

        Task<List<ResponseFamilyDTO>> GetAsync(int pageNumber, int pageQuantity, string? search = null);

        Task<List<ResponseFamilyDTO>> GetFamiliesByWeddingBirthdayMonthAsync(int monthNumber);
        Task<List<ResponseFamilyDTO>> GetFilteredAsync(
            long? churchId,
            long? cellId,
            int pageNumber = 1,
            int pageQuantity = 50,
            string? search = null,
            long? federationId = null);
        Task<ResponseFamilyDTO> UpdateAsync(long id, FamilyPatchDTO familyUpdated);

        Task<ResponseFamilyDTO> DeleteAsync(long id);

        Task SaveAsync();
    }
}
