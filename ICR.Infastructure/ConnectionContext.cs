using ICR.Domain.Model.ChurchAggregate;
using ICRManagement.Domain.Model.FederationAggregate;
using Microsoft.EntityFrameworkCore;

namespace ICRManagement.Infra
{
    public class ConnectionContext : DbContext
    {
        // Construtor exigido pelo AddDbContext
        public ConnectionContext(DbContextOptions<ConnectionContext> options)
            : base(options)
        {
        }

        public DbSet<Federation> Federations { get; set; }
        //public DbSet<Church> Churches { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Church>().OwnsOne(c => c.Address);
        }
    }
}
