using ICR.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.Domain.Model.FamilyAggregate
{
    public interface IFamilyRepository
    {
        Task<ResponseFamilyDTO> AddAsync(Family family);

        Task<ResponseFamilyDTO?> GetByIdAsync(long id);

        Task<List<ResponseFamilyDTO>> GetAsync(int pageNumber, int pageQuantity);

        Task<List<ResponseFamilyDTO>> GetFamiliesByWeddingBirthdayMonthAsync(int monthNumber);
        Task<List<ResponseFamilyDTO>> GetByChurchId(long churchId);

        Task<List<ResponseFamilyDTO>> GetByCellIdAsync(long cellId);
        Task<ResponseFamilyDTO> UpdateAsync(long id, FamilyPatchDTO familyUpdated);

        Task<ResponseFamilyDTO> DeleteAsync(long id);

        Task SaveAsync();
    }
}
