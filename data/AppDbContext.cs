using epico_backend.models.players;
using Microsoft.EntityFrameworkCore;

namespace epico_backend.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }


        public DbSet<PlayerModel> players { get; set; }
       
    }
}
