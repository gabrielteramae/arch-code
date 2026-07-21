using Microsoft.EntityFrameworkCore;
using SpotifyArch.Api.Models;

namespace SpotifyArch.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>()
            .HasOne(a => a.Artist)
            .WithMany(ar => ar.Albums)
            .HasForeignKey(a => a.ArtistId);

        modelBuilder.Entity<Track>()
            .HasOne(t => t.Album)
            .WithMany(al => al.Tracks)
            .HasForeignKey(t => t.AlbumId);
    }
}
