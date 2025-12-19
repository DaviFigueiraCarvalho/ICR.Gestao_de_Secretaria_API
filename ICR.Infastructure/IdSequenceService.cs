using System;
using System.Collections.Generic;
using System.Text;
using ICR.Domain.Model;
using Microsoft.EntityFrameworkCore;
using ICRManagement.Infra;
namespace ICR.Application.Services
{
    public class IdSequenceService
    {
        private readonly ConnectionContext _context;

        public IdSequenceService(ConnectionContext context)
        {
            _context = context;
        }

        public long GetNextId<T>() where T : class, BasicModel
        {
            var now = DateTime.UtcNow;
            var prefix = long.Parse($"{now:yyyyMM}");
            var min = prefix * 1000;
            var max = min + 999;

            // pega o maior ID existente para essa entidade
            var last = _context.Set<T>()
                .Where(x => x.Id >= min && x.Id <= max)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefault();

            return last == 0 ? min + 1 : last + 1;
        }
    }
}
