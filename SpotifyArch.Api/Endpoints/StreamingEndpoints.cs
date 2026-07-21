using Microsoft.EntityFrameworkCore;
using SpotifyArch.Api.Data;
using SpotifyArch.Api.Models;
using SpotifyArch.Api.Services;

namespace SpotifyArch.Api.Endpoints;

public static class StreamingEndpoints
{
    public static void MapStreamingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Streaming");

        // Passo 1: cliente pede a URL de streaming (equivalente a
        // "GET /v1/tracks/{trackId}/stream-url" do case study de arquitetura)
        group.MapGet("/tracks/{id:guid}/stream-url", async (
            Guid id, AppDbContext db, SignedUrlService signer, HttpRequest request) =>
        {
            var trackExists = await db.Tracks.AnyAsync(t => t.Id == id);
            if (!trackExists) return Results.NotFound();

            var (token, expiresAt) = signer.GenerateToken(id);
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var streamUrl = $"{baseUrl}/api/stream?token={token}";

            return Results.Ok(new StreamUrlResponse(streamUrl, expiresAt));
        })
        .WithSummary("Gera uma URL assinada e temporária para reprodução (simula CDN signed URL)");

        // Passo 2: cliente busca o áudio direto nessa URL, com suporte a Range
        // (na arquitetura real, essa requisição vai para a CDN, não para o backend)
        group.MapGet("/stream", async (
            string token, AppDbContext db, SignedUrlService signer,
            IWebHostEnvironment env, HttpContext ctx) =>
        {
            if (!signer.TryValidate(token, out var trackId))
                return Results.Unauthorized();

            var track = await db.Tracks.FindAsync(trackId);
            if (track is null) return Results.NotFound();

            var filePath = Path.Combine(env.ContentRootPath, "Uploads", track.AudioRef);
            if (!File.Exists(filePath)) return Results.NotFound("Arquivo de áudio não encontrado.");

            // Incrementa contador de reprodução (na arquitetura real, isso seria
            // um evento assíncrono publicado no Event Bus, não uma escrita síncrona)
            track.PlayCount++;
            await db.SaveChangesAsync();

            var contentType = GetContentType(filePath);

            // enableRangeProcessing: true → habilita HTTP Range Requests,
            // permitindo seek/buffer progressivo sem baixar o arquivo inteiro
            return Results.File(filePath, contentType, enableRangeProcessing: true);
        })
        .WithSummary("Serve o arquivo de áudio validando o token assinado (com suporte a Range Requests)");

        // Registro de evento de reprodução (simplificação do Event Bus do case study)
        group.MapPost("/playback-events", (PlaybackEvent evt) =>
        {
            // Em produção: publicar em Kafka/SQS para consumo assíncrono
            // pelo pipeline de analytics e pelo sistema de recomendação.
            app.Logger.LogInformation(
                "Evento de reprodução: track={TrackId} user={UserId} msPlayed={MsPlayed}",
                evt.TrackId, evt.UserId, evt.MsPlayed);

            return Results.Accepted();
        })
        .WithSummary("Registra um evento de reprodução (simula publicação no Event Bus)");
    }

    private static string GetContentType(string filePath) => Path.GetExtension(filePath).ToLowerInvariant() switch
    {
        ".mp3" => "audio/mpeg",
        ".ogg" => "audio/ogg",
        ".wav" => "audio/wav",
        ".m4a" => "audio/mp4",
        _ => "application/octet-stream"
    };
}
