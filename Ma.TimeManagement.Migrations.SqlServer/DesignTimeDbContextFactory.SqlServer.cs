using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ma.TimeManagement.Data
{
    public class DesignTimeSqlServerFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use a local dev SQL Server connection (adjust as needed)
            var connStr = "Server=(localdb)\\mssqllocaldb;Database=MaTimeManagement.Dev;Trusted_Connection=True;MultipleActiveResultSets=true";

            // Must exactly match the SQL Server migrations project's assembly name
            builder.UseSqlServer(connStr);//, b => b.MigrationsAssembly("Ma.TimeManagement.Migrations.SqlServer")

            return new ApplicationDbContext(builder.Options);
        }
    }
}
