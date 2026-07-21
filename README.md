# 🎧 SpotifyArch — Implementação em C# do Case Study "Arquitetando o Spotify"

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?style=flat&logo=blazor&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-SQLite-blue?style=flat)

Prova de conceito em código do case study de system design [Arquitetando o Spotify](../arquitetando-spotify): uma API de catálogo/streaming e um mini player web, ambos em C#, implementando os conceitos-chave do documento de arquitetura — **URLs assinadas (signed URLs)**, **HTTP Range Requests** para streaming progressivo, e **eventos de reprodução** (análogo simplificado do Event Bus).

## 📦 Projetos

| Projeto | O quê | Tecnologia |
|---|---|---|
| `SpotifyArch.Api` | API de catálogo + streaming | ASP.NET Core Minimal APIs, EF Core, SQLite |
| `SpotifyArch.Player` | Mini player web | Blazor WebAssembly |

## 🧠 Conceitos do case study implementados

- ✅ **Signed URLs**: `/api/tracks/{id}/stream-url` gera um token assinado (HMAC-SHA256) com expiração — o player nunca acessa o storage diretamente, só recebe uma URL temporária, exatamente como descrito na seção 8 do case study.
- ✅ **HTTP Range Requests**: o endpoint `/api/stream` usa `enableRangeProcessing: true`, permitindo que o navegador dê seek e faça buffering progressivo sem baixar o arquivo inteiro.
- ✅ **Eventos de reprodução**: `POST /api/playback-events` simula a publicação no Event Bus (Kafka/Kinesis na arquitetura real) consumido pelo pipeline de analytics/recomendação.
- ✅ **Separação de domínio**: catálogo e streaming em módulos de endpoint isolados, preparando terreno para virarem microsserviços separados no futuro.

## 🚀 Como rodar localmente

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Um arquivo `.mp3` qualquer (livre de direitos), renomeado para `sample.mp3`

### 1. Rodar a API

```bash
cd SpotifyArch.Api
# adicione um mp3 de teste antes de rodar:
cp /caminho/para/algum.mp3 Uploads/sample.mp3

dotnet restore
dotnet run
```

A API sobe em `http://localhost:5000`. O Swagger fica em `http://localhost:5000/swagger`.

No primeiro start, o banco SQLite é criado automaticamente e populado com uma faixa de exemplo (`Faixa de Exemplo`, do `Banda Demo`).

### 2. Rodar o Player (Blazor WebAssembly)

Em outro terminal:

```bash
cd SpotifyArch.Player
dotnet restore
dotnet run
```

Abre em `http://localhost:5173` (ou a porta que o terminal indicar). O `wwwroot/appsettings.json` já aponta para `http://localhost:5000` — ajuste se sua API rodar em outra porta.

### 3. Testar

1. Abra o Player no navegador
2. A faixa de exemplo já deve aparecer na lista
3. Clique nela — o player vai buscar a signed URL na API e tocar o áudio
4. Acompanhe o `PlayCount` subindo a cada reprodução (via `GET /api/tracks`)

## 🔍 Testando a API isoladamente (sem o Player)

```bash
# Lista faixas
curl http://localhost:5000/api/tracks

# Gera uma signed URL para uma faixa (pegue o {id} do comando acima)
curl http://localhost:5000/api/tracks/{id}/stream-url

# A URL retornada já pode ser aberta direto no navegador ou testada com:
curl -v "http://localhost:5000/api/stream?token=..."
```

Repare que uma URL expirada ou com token adulterado retorna `401 Unauthorized` — é a validação de assinatura em ação.

## ☁️ Deploy

- **API**: Railway (mesmo padrão do `transformador-de-arquivos`). Lembre de trocar `Streaming:SigningSecret` por uma variável de ambiente segura, e ajustar `Cors:AllowedOrigins`/`AllowAnyOrigin()` para restringir à URL real do Player em produção.
- **Player**: como é Blazor WebAssembly, o resultado do `dotnet publish` é um conjunto de arquivos estáticos (`bin/Release/net8.0/publish/wwwroot`) — pode subir no Vercel como qualquer outro frontend estático seu. Antes de publicar, atualize `wwwroot/appsettings.json` com a URL da API em produção.

## 🗺️ Relação com o case study

Este código implementa uma fatia pequena e deliberadamente simplificada da arquitetura completa (sem microsserviços de fato separados, sem CDN real, sem fila de mensageria de verdade) — o objetivo é provar, na prática, os conceitos mais importantes do documento: **por que** signed URLs existem, **como** range requests viabilizam streaming, e **onde** eventos de reprodução se encaixam no pipeline de dados.

Veja o documento completo de arquitetura em [`arquitetando-spotify/README.md`](../arquitetando-spotify/README.md).

---

© 2026 Gabriel Teramae Chan
