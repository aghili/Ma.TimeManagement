using Ma.TimeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.AdoPatEncrypted).HasMaxLength(500);
            });

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<TeamProjectReference> TeamProjects { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkCalendarItem> WorkCalendarItems { get; set; }
    }
}
