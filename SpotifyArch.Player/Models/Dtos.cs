namespace SpotifyArch.Player.Models;

public record TrackDto(Guid Id, string Title, int DurationMs, string ArtistName, string AlbumTitle, int PlayCount);

public record StreamUrlResponse(string Url, DateTimeOffset ExpiresAt);

public record PlaybackEvent(Guid TrackId, Guid UserId, int MsPlayed);
