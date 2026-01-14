using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.MemberAggregate;
using ICRManagement.Domain.DTOs;
using System.Collections.Generic;

namespace ICR.Domain.Model.FederationAggregate
{
    public interface IFederationRepository
    {
        Task<FederationResponseDTO> AddAsync(FederationDTO federation);
        Task<FederationResponseDTO?> GetByIdAsync(long id);
        Task<IEnumerable<FederationResponseDTO>> GetAllFederationsAsync();
        Task<FederationResponseDTO> UpdateAsync(long id, FederationPatchDTO federation);
        Task<FederationResponseDTO> DeleteAsync(long id);

    }
}
