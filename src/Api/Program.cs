using System.Text.Json;
using Api.Endpoints;
using Api.Infrastructure;
using Core.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Єдиний стиль JSON (camelCase), заданий явно
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// 2. Реєстрація ProblemDetails та обробника DomainException
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// Налаштування рядка підключення та бази даних SQLite
var raw = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(raw);

if (!Path.IsPathRooted(csb.DataSource))
{
    var full = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    csb.DataSource = full;
}

var webOrigin = builder.Configuration["Cors:WebOrigin"] ?? "http://localhost:5090";

builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
        policy.WithOrigins(webOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Реєстрація DbContext та сховища у DI-контейнері
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(csb.ConnectionString));
builder.Services.AddScoped<IBookStore, EfCatalogStore>();

var app = builder.Build();

// 3. Глобальний перехоплювач винятків (без stack trace у Production) та статус-кодів
app.UseExceptionHandler();
app.UseStatusCodePages();

// Автоматичне застосування міграцій у режимі розробки
using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}

app.UseCors("web");

// Реєструємо маршрути Minimal API
app.MapBookEndpoints();

app.Run();