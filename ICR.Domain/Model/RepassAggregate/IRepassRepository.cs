using System.Collections.Generic;

namespace ICR.Domain.Model.RepassAggregate
{
    public interface IRepassRepository
    {
        
        void Add(Repass repass);
        Repass? GetById(long id);
        List<Repass> Get(int pageNumber, int pageQuantity);
        void Delete(long id);
        void Save();

        List<Repass> GetByChurchId(long churchId);
        List<Repass> GetByReference(long reference);
    }
}