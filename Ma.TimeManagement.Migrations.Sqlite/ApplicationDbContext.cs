using Microsoft.EntityFrameworkCore;

namespace Ma.TimeManagement.Data
{
    public class ApplicationDbContext : BaseDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}