using Microsoft.EntityFrameworkCore;

namespace Web.Data
{
    public class DbContextRoom : DbContext
    {
        public DbContextRoom(DbContextOptions<DbContextRoom> opt):base(opt)
        {
        }
        #region DbSet
        public DbSet<Room> Rooms { get; set; }
        #endregion
    }
}
