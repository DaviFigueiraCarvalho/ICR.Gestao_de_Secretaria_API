using ICRManagement.Application.ViewModel;
using ICRManagement.Domain.Model.FederationAggregate;
using ICRManagement.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICRManagement.Infra.Repositories
{
    public class FederationRepository : IFederationRepository
    {
        private readonly ConnectionContext _context;

        public FederationRepository(ConnectionContext context)
        {
            _context = context;
        }

        public void Add(Federation federation)
        {
            _context.Federations.Add(federation);
            _context.SaveChanges();
        }

        // Usado no PATCH
        public Federation? GetEntity(long id)
        {
            return _context.Federations.Find(id);
        }

        // GET by id
        public Federation? Get(long id)
        {
            return _context.Federations.Find(id);
        }

        public List<Federation> Get(int pageNumber, int pageQuantity)
        {
            return _context.Federations
                .OrderBy(f => f.Name)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
                .ToList();
        }

        public void Delete(long id)
        {
            var federation = GetEntity(id);
            if (federation == null)
                throw new InvalidOperationException("Federation not found");

            _context.Federations.Remove(federation);
            _context.SaveChanges();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
