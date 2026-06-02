using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.MemberAggregate;
using ICR.Domain.Model.MinisterAggregate;
using ICR.Domain.Model.RepassAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.EntityFrameworkCore;

namespace ICR.Infra
{
    public class ConnectionContext : DbContext
    {
        public ConnectionContext(DbContextOptions<ConnectionContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Church> Churches => Set<Church>();
        public DbSet<Cell> Cells => Set<Cell>();
        public DbSet<Family> Families => Set<Family>();
        public DbSet<Minister> Ministers => Set<Minister>();
        public DbSet<Federation> Federations => Set<Federation>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Repass> Repasses => Set<Repass>();
        public DbSet<Reference> References => Set<Reference>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =======================
            // USER
            // =======================

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.MemberId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Member)
                .WithMany()
                .HasForeignKey(u => u.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // CHURCH
            // =======================

            modelBuilder.Entity<Church>()
                .HasOne(c => c.Minister)
                .WithMany()
                .HasForeignKey(c => c.MinisterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Church>()
                .HasOne(c => c.Federation)
                .WithMany()
                .HasForeignKey(c => c.FederationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Church -> Address (Value Object)
            modelBuilder.Entity<Church>().OwnsOne(c => c.Address, address =>
            {
                address.OwnsOne(a => a.Country, country =>
                {
                    country.Property(co => co.Code).HasColumnName("Country_Code");
                    country.Property(co => co.Name).HasColumnName("Country_Name");
                    country.Property(co => co.PhoneCountryCode).HasColumnName("Country_PhoneCountryCode");
                    country.Property(co => co.CultureCode).HasColumnName("Country_CultureCode");
                });
                address.Property(a => a.PostalCode).HasColumnName("PostalCode");
                address.Property(a => a.Street).HasColumnName("Street");
                address.Property(a => a.Number).HasColumnName("Number");
                address.Property(a => a.Complement).HasColumnName("Complement");
                address.Property(a => a.City).HasColumnName("City");
                address.Property(a => a.State).HasColumnName("State");
                address.Property(a => a.CountyOrRegion).HasColumnName("CountyOrRegion");
            });

            // =======================
            // MINISTER
            // =======================

            modelBuilder.Entity<Minister>()
                .HasOne(m => m.Member)
                .WithMany()
                .HasForeignKey(m => m.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Minister -> Address (Value Object)
            var ministerAddressBuilder = modelBuilder.Entity<Minister>().OwnsOne(m => m.Address);
            ministerAddressBuilder.WithOwner().HasForeignKey("MinisterId");
            ministerAddressBuilder.Property(a => a.Id).HasColumnName("AddressId");
            // Adiciona propriedade shadow para discriminação quando Address é opcional com dependentes
            ministerAddressBuilder.Property<bool>("_addressExists").HasColumnName("AddressExists").HasDefaultValue(false);

            // Definir que a navegação é obrigatória se qualquer propriedade de Address for preenchida
            ministerAddressBuilder.OwnsOne(a => a.Country, country =>
            {
                country.Property(co => co.Code).HasColumnName("Country_Code").IsRequired(false);
                country.Property(co => co.Name).HasColumnName("Country_Name").IsRequired(false);
                country.Property(co => co.PhoneCountryCode).HasColumnName("Country_PhoneCountryCode").IsRequired(false);
                country.Property(co => co.CultureCode).HasColumnName("Country_CultureCode").IsRequired(false);
                // Adiciona propriedade shadow para discriminação do Country quando for opcional
                country.Property<bool>("_countryExists").HasColumnName("CountryExists").HasDefaultValue(false);
            });
            ministerAddressBuilder.Property(a => a.PostalCode).HasColumnName("PostalCode");
            ministerAddressBuilder.Property(a => a.Street).HasColumnName("Street");
            ministerAddressBuilder.Property(a => a.Number).HasColumnName("Number");
            ministerAddressBuilder.Property(a => a.Complement).HasColumnName("Complement");
            ministerAddressBuilder.Property(a => a.City).HasColumnName("City");
            ministerAddressBuilder.Property(a => a.State).HasColumnName("State");
            ministerAddressBuilder.Property(a => a.CountyOrRegion).HasColumnName("CountyOrRegion");

            // Member -> CellPhone (Value Object, opcional)
            modelBuilder.Entity<Member>().OwnsOne(m => m.CellPhone, phone =>
            {
                phone.OwnsOne(p => p.Country, country =>
                {
                    country.Property(co => co.Code).HasColumnName("Country_Code");
                    country.Property(co => co.Name).HasColumnName("Country_Name");
                    country.Property(co => co.PhoneCountryCode).HasColumnName("Country_PhoneCountryCode");
                    country.Property(co => co.CultureCode).HasColumnName("Country_CultureCode");
                });
                phone.Property(p => p.Number).HasColumnName("PhoneNumber");
                phone.Property(p => p.DisplayFormat).HasColumnName("PhoneDisplayFormat");
                phone.Property(p => p.InternationalFormat).HasColumnName("PhoneInternationalFormat");
                phone.Property(p => p.E164Format).HasColumnName("PhoneE164Format");
            });

            // =======================
            // CELL
            // =======================

            modelBuilder.Entity<Cell>()
                .HasOne(c => c.Church)
                .WithMany()
                .HasForeignKey(c => c.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cell>()
                .HasOne(c => c.Responsible)
                .WithMany()
                .HasForeignKey(c => c.ResponsibleId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // FAMILY
            // =======================

            modelBuilder.Entity<Family>()
                .HasOne(f => f.Man)
                .WithMany()
                .HasForeignKey(f => f.ManId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Family>()
                .HasOne(f => f.Woman)
                .WithMany()
                .HasForeignKey(f => f.WomanId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // REPASS
            // =======================

            modelBuilder.Entity<Repass>()
                .HasOne(r => r.Church)
                .WithMany()
                .HasForeignKey(r => r.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Repass>()
                .HasOne(r => r.Reference)
                .WithMany()
                .HasForeignKey(r => r.ReferenceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
             .HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}
