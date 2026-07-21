Coloque aqui um arquivo `sample.mp3` (qualquer MP3 curto, livre de direitos autorais)
para testar o fluxo completo de streaming localmente.

O seed de dados (SeedData.cs) cria uma faixa de exemplo que aponta para
`Uploads/sample.mp3`. Sem esse arquivo, o endpoint `/api/stream` retornará 404.

Para faixas reais adicionadas via `POST /api/albums/{albumId}/tracks`,
os arquivos são salvos automaticamente aqui pelo próprio backend.
