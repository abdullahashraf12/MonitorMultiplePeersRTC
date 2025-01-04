using Microsoft.EntityFrameworkCore;
using MonitorMultiplePeersRTC.Models.UserRTCModel;

namespace MonitorMultiplePeersRTC.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor that takes DbContextOptions<ApplicationDbContext>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Constructor that takes a connection string (your custom constructor)
        public ApplicationDbContext(string connectionString)
            : base(new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options)
        {
        }

        // DbSets for your entities
        public DbSet<UserRTCModel> UserRTCModel { get; set; }
    }
}
