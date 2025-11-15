using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Models;

namespace UrlShortener.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShortUrl> ShortUrls { get; set; }
    public DbSet<AboutContent> AboutContents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ShortUrl>(entity =>
        {
            entity.HasIndex(e => e.ShortCode).IsUnique();
            entity.HasIndex(e => e.OriginalUrl).IsUnique();
            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.ShortUrls)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AboutContent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}

