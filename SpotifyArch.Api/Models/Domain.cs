namespace SpotifyArch.Api.Models;

public class Artist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }

    public List<Album> Albums { get; set; } = new();
}

public class Album
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }

    public Guid ArtistId { get; set; }
    public Artist? Artist { get; set; }

    public List<Track> Tracks { get; set; } = new();
}

public class Track
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int DurationMs { get; set; }

    /// <summary>
    /// Caminho relativo do arquivo de áudio dentro da pasta Uploads.
    /// Em produção, isso seria a chave do objeto no S3/Blob Storage.
    /// </summary>
    public string AudioRef { get; set; } = string.Empty;

    public Guid AlbumId { get; set; }
    public Album? Album { get; set; }

    public int PlayCount { get; set; } = 0;
}

// ---------- DTOs ----------

public record TrackDto(Guid Id, string Title, int DurationMs, string ArtistName, string AlbumTitle, int PlayCount);

public record StreamUrlResponse(string Url, DateTimeOffset ExpiresAt);

public record CreateArtistRequest(string Name, string? Bio);

public record CreateAlbumRequest(string Title, DateOnly ReleaseDate, Guid ArtistId);

public record PlaybackEvent(Guid TrackId, Guid UserId, int MsPlayed);
