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

        // Phương thức OnModelCreating để định nghĩa chỉ mục duy nhất
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Cách 1: Tạo chỉ mục duy nhất cho cột Url
        //    modelBuilder.Entity<Room>()
        //        .HasIndex(r => r.Url)  // Đảm bảo rằng cột Url là duy nhất
        //        .IsUnique();

        //    // Cách 2: Tạo chỉ mục duy nhất cho sự kết hợp giữa NameHost và Status
        //    //modelBuilder.Entity<Room>()
        //    //    .HasIndex(r => new { r.NameHost, r.Status })  // Đảm bảo rằng sự kết hợp giữa NameHost và Status là duy nhất
        //    //    .IsUnique();
        //}
    }
}
