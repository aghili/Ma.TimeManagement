using Ma.TimeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.VisualStudio.Services.Profile;

namespace Ma.TimeManagement.Data
{
    public class DatabaseDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite();

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TeamProjectReference> TeamProjects { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkCalendarItem> WorkCalendarItems { get; set; }
    }
}
