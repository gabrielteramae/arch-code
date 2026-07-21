using Microsoft.EntityFrameworkCore;
using SpotifyArch.Api.Data;
using SpotifyArch.Api.Endpoints;
using SpotifyArch.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serviços ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=spotifyarch.db"));

builder.Services.AddSingleton<SignedUrlService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Arquitetando o Spotify — API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ---------- Migração + Seed ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await SeedData.SeedIfEmptyAsync(db);
}

// ---------- Middleware ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/", () => Results.Ok(new
{
    projeto = "Arquitetando o Spotify — API de Catálogo e Streaming",
    docs = "/swagger"
}));

app.MapCatalogEndpoints();
app.MapStreamingEndpoints();

app.Run();
