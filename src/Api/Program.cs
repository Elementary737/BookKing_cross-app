using Api.Endpoints;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;
using Api.Contracts;
using FluentValidation;
using Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddSingleton<IBookStore, InMemoryBookStore>();
builder.Services.AddValidatorsFromAssemblyContaining<SaveBookRequestValidator>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapBookEndpoints();

app.Run();
public partial class Program { }