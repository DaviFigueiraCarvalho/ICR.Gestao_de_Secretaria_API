using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ICRManagement.Infra
{
    public class ConnectionContextFactory : IDesignTimeDbContextFactory<ConnectionContext>
    {
        public ConnectionContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConnectionContext>();

            var connectionString =
                Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                ? "Host=icr_db;Port=5432;Database=icr_connect;Username=icradmin;Password=root"
                : "Host=localhost;Port=5432;Database=icr_connect;Username=icradmin;Password=root";

            optionsBuilder.UseNpgsql(connectionString);

            return new ConnectionContext(optionsBuilder.Options);
        }
    }
}
