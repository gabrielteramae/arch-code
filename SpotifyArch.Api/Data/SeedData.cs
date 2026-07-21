using SpotifyArch.Api.Models;

namespace SpotifyArch.Api.Data;

public static class SeedData
{
    /// <summary>
    /// Popula o banco com dados de exemplo apenas se estiver vazio.
    /// Os arquivos de áudio referenciados devem existir em Uploads/
    /// (veja Uploads/README.md para instruções de como adicionar arquivos de teste).
    /// </summary>
    public static async Task SeedIfEmptyAsync(AppDbContext db)
    {
        if (db.Artists.Any()) return;

        var artist = new Artist
        {
            Name = "Banda Demo",
            Bio = "Artista fictício criado para popular o catálogo de exemplo."
        };

        var album = new Album
        {
            Title = "Álbum de Demonstração",
            ReleaseDate = new DateOnly(2026, 1, 1),
            Artist = artist
        };

        var track = new Track
        {
            Title = "Faixa de Exemplo",
            DurationMs = 180_000, // 3 min
            AudioRef = "sample.mp3", // coloque um arquivo .mp3 real em Uploads/sample.mp3
            Album = album
        };

        album.Tracks.Add(track);
        artist.Albums.Add(album);

        db.Artists.Add(artist);
        db.Albums.Add(album);
        db.Tracks.Add(track);

        await db.SaveChangesAsync();
    }
}
