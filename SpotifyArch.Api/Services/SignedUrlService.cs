using System.Security.Cryptography;
using System.Text;

namespace SpotifyArch.Api.Services;

/// <summary>
/// Simula o mecanismo de "signed URLs" usado por CDNs (ex: CloudFront/CloudFlare)
/// descrito no case study de arquitetura: o serviço de streaming nunca serve o
/// áudio diretamente, apenas emite uma URL temporária e assinada.
/// </summary>
public class SignedUrlService
{
    private readonly byte[] _secretKey;
    private readonly TimeSpan _defaultTtl;

    public SignedUrlService(IConfiguration config)
    {
        var secret = config["Streaming:SigningSecret"]
            ?? throw new InvalidOperationException("Streaming:SigningSecret não configurado.");
        _secretKey = Encoding.UTF8.GetBytes(secret);
        _defaultTtl = TimeSpan.FromMinutes(
            config.GetValue<int?>("Streaming:UrlTtlMinutes") ?? 5);
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(Guid trackId)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(_defaultTtl);
        var expiresUnix = expiresAt.ToUnixTimeSeconds();

        var payload = $"{trackId}:{expiresUnix}";
        var signature = Sign(payload);

        // token = payload em base64 + assinatura, tudo url-safe
        var token = $"{Base64UrlEncode(payload)}.{signature}";
        return (token, expiresAt);
    }

    public bool TryValidate(string token, out Guid trackId)
    {
        trackId = Guid.Empty;

        var parts = token.Split('.');
        if (parts.Length != 2) return false;

        string payload;
        try
        {
            payload = Base64UrlDecode(parts[0]);
        }
        catch (FormatException)
        {
            return false;
        }
        var expectedSignature = Sign(payload);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(parts[1])))
        {
            return false; // assinatura inválida — token adulterado
        }

        var payloadParts = payload.Split(':');
        if (payloadParts.Length != 2) return false;

        if (!Guid.TryParse(payloadParts[0], out var parsedTrackId)) return false;
        if (!long.TryParse(payloadParts[1], out var expiresUnix)) return false;

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresUnix);
        if (DateTimeOffset.UtcNow > expiresAt) return false; // token expirado

        trackId = parsedTrackId;
        return true;
    }

    private string Sign(string payload)
    {
        using var hmac = new HMACSHA256(_secretKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(string input) =>
        Base64UrlEncode(Encoding.UTF8.GetBytes(input));

    private static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
