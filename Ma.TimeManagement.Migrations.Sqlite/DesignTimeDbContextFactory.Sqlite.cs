using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.Sqlite;

namespace Ma.TimeManagement.Data
{
    public class DesignTimeSqliteFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var connStr = new SqliteConnectionStringBuilder
            {
                DataSource = "local.sqlite",
                Cache = SqliteCacheMode.Shared,
                Pooling = true
            }.ToString();

            // Must exactly match the SQLite migrations project's assembly name
            builder.UseSqlite(connStr); //, b => b.MigrationsAssembly("Ma.TimeManagement.Migrations.Sqlite")

            return new ApplicationDbContext(builder.Options);
        }
    }
}
