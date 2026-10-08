# Arch Code — catálogo e streaming do case Spotify

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Minimal%20APIs-512BD4?style=flat&logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?style=flat&logo=blazor&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-EF%20Core%208-003B57?style=flat&logo=sqlite&logoColor=white)

Prova de conceito em C# do estudo [Arquitetando o Spotify](https://github.com/gabrielteramae/arquitetando-spotify). São dois projetos: uma API de catálogo e streaming e um player Blazor WebAssembly. Não há microsserviços separados, CDN nem fila.

| Peça | O que o código faz | O que não faz |
|---|---|---|
| URL assinada | `SignedUrlService` assina `trackId:expiração` com HMAC-SHA256. O TTL padrão é 5 minutos (`Streaming:UrlTtlMinutes`). | Não é CloudFront nem outro CDN. |
| Áudio | `GET /api/stream` devolve o arquivo local com `enableRangeProcessing: true`. | O arquivo fica em `Uploads/`, não em object storage. |
| Evento de play | `POST /api/playback-events` só escreve log e responde 202. O stream incrementa `PlayCount` na hora. | Não publica em Kafka, Kinesis nem SQS. |

## Stack

- .NET 10 (`net10.0`) nos dois projetos
- API: ASP.NET Core Minimal APIs, Swagger apenas em Development, EF Core 8.0.10 com SQLite
- Player: Blazor WebAssembly (pacotes 10.0.10) e `wwwroot/js/audioPlayer.js`

## Estrutura

```
arch-code/
├── SpotifyArch.sln
├── SpotifyArch.Api/
│   ├── Program.cs
│   ├── SpotifyArch.Api.csproj
│   ├── appsettings.json
│   ├── Data/            AppDbContext, SeedData
│   ├── Endpoints/       CatalogEndpoints, StreamingEndpoints
│   ├── Models/Domain.cs
│   ├── Services/SignedUrlService.cs
│   └── Uploads/         áudio local (sample.mp3 não vem no git)
└── SpotifyArch.Player/
    ├── Program.cs
    ├── Pages/Player.razor
    ├── Services/ApiClient.cs
    └── wwwroot/         appsettings.json, css, js/audioPlayer.js
```

Catálogo: `GET /api/tracks` (query `q`), `GET /api/tracks/{id}`, `POST /api/artists`, `POST /api/albums`, `POST /api/albums/{albumId}/tracks` (multipart).

Streaming: `GET /api/tracks/{id}/stream-url`, `GET /api/stream?token=`, `POST /api/playback-events`.

No primeiro start, `EnsureCreated` cria o SQLite `spotifyarch.db`. Se não houver artista, o seed grava "Banda Demo", "Álbum de Demonstração" e "Faixa de Exemplo", com `AudioRef` `sample.mp3`.

## Como rodar

Pré-requisito: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/gabrielteramae/arch-code.git
cd arch-code
```

Coloque um `.mp3` em `SpotifyArch.Api/Uploads/sample.mp3`. O `Uploads/README.txt` descreve esse passo. Sem o arquivo, a URL assinada é gerada, mas `/api/stream` responde que o áudio não foi encontrado.

```bash
dotnet run --project SpotifyArch.Api
```

A API escuta em `http://localhost:5000` (`Properties/launchSettings.json`). Swagger em `http://localhost:5000/swagger`.

Em outro terminal:

```bash
dotnet run --project SpotifyArch.Player
```

O player não tem `launchSettings.json`; use a porta que o terminal mostrar. `wwwroot/appsettings.json` aponta `ApiBaseUrl` para `http://localhost:5000`.

Token adulterado ou expirado em `/api/stream` volta 401. O segredo fica em `Streaming:SigningSecret` no `appsettings.json`.

---

© 2026 Gabriel Teramae Chan
