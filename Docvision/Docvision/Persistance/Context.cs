using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Docvision.Models;
using Microsoft.AspNetCore.Identity;

namespace Docvision.Persistance
{
    public class DocContext : IdentityDbContext<ApplicationUser>
    {
        public DocContext(DbContextOptions<DocContext> options)
            : base(options)
        {
        }

        // Déclaration des DbSet
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<DocumentImage> Images { get; set; } = null!;
        public DbSet<ObjectImage> Objects { get; set; } = null!;
        public DbSet<Description> Descriptions { get; set; } = null!;
        public DbSet<DescriptionObject> DescriptionObjects { get; set; } = null!;
        public DbSet<ModelConfiguration> ModelConfigurations { get; set; }
        public DbSet<ModelAI> ModelAIs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Pour Identity

            // Document -> Images (1:N)
            modelBuilder.Entity<Document>()
                .HasMany(d => d.Images)
                .WithOne(i => i.Document)
                .HasForeignKey(i => i.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document -> User (1:N)
            modelBuilder.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // DocumentImage -> ObjectImage (1:N)
            modelBuilder.Entity<DocumentImage>()
                .HasMany(img => img.Objects)
                .WithOne(o => o.Image)
                .HasForeignKey(o => o.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            // DescriptionObject (jointure N:N entre Description et ObjectImage)
            modelBuilder.Entity<DescriptionObject>()
                .HasKey(doj => new { doj.DescriptionId, doj.ObjectId });

            modelBuilder.Entity<DescriptionObject>()
                .HasOne(doj => doj.Description)
                .WithMany(d => d.DescriptionObjects)
                .HasForeignKey(doj => doj.DescriptionId);

            modelBuilder.Entity<DescriptionObject>()
                .HasOne(doj => doj.DetectedObject)
                .WithMany(o => o.DescriptionObjects)
                .HasForeignKey(doj => doj.ObjectId);

            // ModelAI -> description (1:N)
            modelBuilder.Entity<ModelAI>()
                .HasMany(d => d.descriptions)
                .WithOne(i => i.modelAI)
                .HasForeignKey(i => i.ModelAIId)
                .OnDelete(DeleteBehavior.NoAction);
        }


    }
}
