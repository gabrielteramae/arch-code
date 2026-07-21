// Ponte fina entre Blazor e a API nativa de <audio> do navegador.
// O Blazor não expõe controle de mídia nativamente, então isso fica
// isolado aqui — o C# nunca manipula o DOM diretamente.

let audioEl = null;
let dotNetRef = null;

export function init(elementId, dotNetHelper) {
    audioEl = document.getElementById(elementId);
    dotNetRef = dotNetHelper;

    audioEl.addEventListener("timeupdate", () => {
        dotNetRef.invokeMethodAsync("OnTimeUpdate", audioEl.currentTime, audioEl.duration || 0);
    });

    audioEl.addEventListener("ended", () => {
        dotNetRef.invokeMethodAsync("OnEnded");
    });
}

export function setSourceAndPlay(url) {
    if (!audioEl) return;
    audioEl.src = url;
    audioEl.play().catch(err => console.error("Erro ao iniciar reprodução:", err));
}

export function pause() {
    audioEl?.pause();
}

export function resume() {
    audioEl?.play();
}

export function seek(seconds) {
    if (audioEl) audioEl.currentTime = seconds;
}

export function getCurrentTime() {
    return audioEl ? audioEl.currentTime : 0;
}
