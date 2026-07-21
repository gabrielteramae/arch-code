using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyArch.Api.Data;
using SpotifyArch.Api.Models;

namespace SpotifyArch.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Catálogo");

        group.MapGet("/tracks", async (AppDbContext db, string? q) =>
        {
            var query = db.Tracks
                .Include(t => t.Album!).ThenInclude(al => al.Artist)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(t =>
                    t.Title.Contains(q) ||
                    t.Album!.Title.Contains(q) ||
                    t.Album!.Artist!.Name.Contains(q));
            }

            var tracks = await query
                .Select(t => new TrackDto(
                    t.Id, t.Title, t.DurationMs,
                    t.Album!.Artist!.Name, t.Album.Title, t.PlayCount))
                .ToListAsync();

            return Results.Ok(tracks);
        })
        .WithSummary("Lista/busca faixas do catálogo");

        group.MapGet("/tracks/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var track = await db.Tracks
                .Include(t => t.Album!).ThenInclude(al => al.Artist)
                .FirstOrDefaultAsync(t => t.Id == id);

            return track is null
                ? Results.NotFound()
                : Results.Ok(new TrackDto(
                    track.Id, track.Title, track.DurationMs,
                    track.Album!.Artist!.Name, track.Album.Title, track.PlayCount));
        })
        .WithSummary("Detalhe de uma faixa");

        group.MapPost("/artists", async (CreateArtistRequest req, AppDbContext db) =>
        {
            var artist = new Artist { Name = req.Name, Bio = req.Bio };
            db.Artists.Add(artist);
            await db.SaveChangesAsync();
            return Results.Created($"/api/artists/{artist.Id}", artist);
        })
        .WithSummary("Cria um artista");

        group.MapPost("/albums", async (CreateAlbumRequest req, AppDbContext db) =>
        {
            var artistExists = await db.Artists.AnyAsync(a => a.Id == req.ArtistId);
            if (!artistExists) return Results.BadRequest("Artista não encontrado.");

            var album = new Album { Title = req.Title, ReleaseDate = req.ReleaseDate, ArtistId = req.ArtistId };
            db.Albums.Add(album);
            await db.SaveChangesAsync();
            return Results.Created($"/api/albums/{album.Id}", album);
        })
        .WithSummary("Cria um álbum");

        // Upload de faixa: metadados + arquivo de áudio (multipart/form-data)
        group.MapPost("/albums/{albumId:guid}/tracks", async (
            Guid albumId, IFormFile audio, [FromForm] string title, [FromForm] int durationMs,
            AppDbContext db, IWebHostEnvironment env) =>
        {
            var album = await db.Albums.FindAsync(albumId);
            if (album is null) return Results.BadRequest("Álbum não encontrado.");

            var uploadsDir = Path.Combine(env.ContentRootPath, "Uploads");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(audio.FileName)}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using (var stream = File.Create(fullPath))
            {
                await audio.CopyToAsync(stream);
            }

            var track = new Track
            {
                Title = title,
                DurationMs = durationMs,
                AudioRef = fileName, // em produção: chave do objeto no S3
                AlbumId = albumId
            };
            db.Tracks.Add(track);
            await db.SaveChangesAsync();

            return Results.Created($"/api/tracks/{track.Id}", track);
        })
        .DisableAntiforgery()
        .WithSummary("Faz upload de uma faixa (metadados + arquivo de áudio)");
    }
}
