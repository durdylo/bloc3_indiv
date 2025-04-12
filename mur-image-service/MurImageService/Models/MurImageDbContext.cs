using Microsoft.EntityFrameworkCore;

namespace MurImageService.Models
{
    public class MurImageDbContext : DbContext
    {
        public MurImageDbContext(DbContextOptions<MurImageDbContext> options)
            : base(options)
        {
        }

        public DbSet<MurImage> MurImages { get; set; }
        public DbSet<Position> Positions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MurImage>().ToTable("mur_image");
            modelBuilder.Entity<Position>().ToTable("position");
            
            // Mapping explicite des propriétés
            modelBuilder.Entity<MurImage>().Property(m => m.Id).HasColumnName("id");
            modelBuilder.Entity<MurImage>().Property(m => m.Nom).HasColumnName("nom");
            
            modelBuilder.Entity<Position>().Property(p => p.Id).HasColumnName("id");
            modelBuilder.Entity<Position>().Property(p => p.IdMurImage).HasColumnName("id_murimage");
            modelBuilder.Entity<Position>().Property(p => p.CodeCamera).HasColumnName("code_camera").IsRequired(false);
            modelBuilder.Entity<Position>().Property(p => p.EstActif).HasColumnName("est_actif").HasDefaultValue(true);
            
            // Relation entre MurImage et Position
            modelBuilder.Entity<Position>()
                .HasOne(p => p.MurImage)
                .WithMany(m => m.Positions)
                .HasForeignKey(p => p.IdMurImage);
        }
    }
}