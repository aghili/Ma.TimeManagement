using Ma.TimeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Ma.TimeManagement.Data
{
    public class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions options)
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

            modelBuilder.Entity<WorkItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                // explicit FK mapping - deterministic across providers
                entity.HasOne(e => e.ProjectReference)
                      .WithMany(p => p.workItems)
                      .HasForeignKey(e => e.ProjectID)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                entity.HasMany(e => e.WorkCalendarItems)
                      .WithOne(w => w.WorkItem)
                      .HasForeignKey(w => w.WorkItemID);
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<TeamProjectReference> TeamProjects { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkCalendarItem> WorkCalendarItems { get; set; }
    }
}