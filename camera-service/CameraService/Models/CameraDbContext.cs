using Microsoft.EntityFrameworkCore;

namespace CameraService.Models
{
    public class CameraDbContext : DbContext
    {
        public CameraDbContext(DbContextOptions<CameraDbContext> options)
            : base(options)
        {
        }

        public DbSet<Camera> Cameras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Camera>().ToTable("camera");
            
            // Mapping explicite des propriétés
            modelBuilder.Entity<Camera>().Property(c => c.Id).HasColumnName("id");
            modelBuilder.Entity<Camera>().Property(c => c.Nom).HasColumnName("nom");
            modelBuilder.Entity<Camera>().Property(c => c.Code).HasColumnName("code");
            modelBuilder.Entity<Camera>().Property(c => c.EstAfficher).HasColumnName("est_afficher");
        }
    }
}