using System.Net.Http.Json;
using SpotifyArch.Player.Models;

namespace SpotifyArch.Player.Services;

/// <summary>
/// Encapsula todas as chamadas à SpotifyArch.Api. Mantém a UI (Player.razor)
/// desacoplada de detalhes de HTTP — se amanhã a API mudar de rota,
/// só este arquivo precisa ser ajustado.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TrackDto>> SearchTracksAsync(string? query = null)
    {
        var url = string.IsNullOrWhiteSpace(query)
            ? "/api/tracks"
            : $"/api/tracks?q={Uri.EscapeDataString(query)}";

        var result = await _http.GetFromJsonAsync<List<TrackDto>>(url);
        return result ?? new List<TrackDto>();
    }

    /// <summary>
    /// Passo 1 do fluxo de streaming: pede ao backend uma URL assinada e
    /// temporária. O player nunca sabe onde o áudio "realmente" está —
    /// só recebe essa URL de curta duração, como descrito no case study.
    /// </summary>
    public async Task<StreamUrlResponse?> GetStreamUrlAsync(Guid trackId)
    {
        return await _http.GetFromJsonAsync<StreamUrlResponse>($"/api/tracks/{trackId}/stream-url");
    }

    /// <summary>
    /// Notifica o backend que uma faixa foi reproduzida (simula o evento
    /// publicado no Event Bus, consumido depois pelo pipeline de analytics
    /// e recomendação).
    /// </summary>
    public async Task ReportPlaybackAsync(Guid trackId, Guid userId, int msPlayed)
    {
        var evt = new PlaybackEvent(trackId, userId, msPlayed);
        await _http.PostAsJsonAsync("/api/playback-events", evt);
    }
}
