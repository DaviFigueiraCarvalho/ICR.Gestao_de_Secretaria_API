using ICRManagement.Domain.Model.FederationAggregate;
using System.Collections.Generic;

namespace ICRManagement.Domain.Repositories
{
    public interface IFederationRepository
    {
        void Add(Federation federation);
        Federation? Get(long id);
        List<Federation> Get(int pageNumber, int pageQuantity);
        void Delete(long id);
        void Save();
    }
}
