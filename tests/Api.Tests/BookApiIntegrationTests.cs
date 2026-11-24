using System.Net;
using System.Net.Http.Json;
using Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public sealed class BookApiIntegrationTests : IDisposable
{
    private readonly string _tempDbPath;
    private readonly WebApplicationFactory<Program> _factory;

    public BookApiIntegrationTests()
    {
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"bookking_test_{Guid.NewGuid():N}.db");
        Environment.SetEnvironmentVariable("ConnectionStrings__AppDb", $"Data Source={_tempDbPath}");

        _factory = new WebApplicationFactory<Program>();
    }

    [Fact]
    public async Task Post_CreatesBook_And_GetByLocation_Returns200OK()
    {
        using var client = _factory.CreateClient();

        var request = new SaveBookRequest(
            Isbn: "978-617-7866-10-6",
            Title: "Тіні забутих предків",
            Author: "Михайло Коцюбинський",
            Year: 1911,
            Copies: 3);

        var postResponse = await client.PostAsJsonAsync("/api/v1/books", request);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var getResponse = await client.GetAsync(postResponse.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var created = await getResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(created);
        Assert.Equal(request.Isbn, created.Isbn);
        Assert.Equal(request.Title, created.Title);
    }

    public void Dispose()
    {
        _factory.Dispose();
        Environment.SetEnvironmentVariable("ConnectionStrings__AppDb", null);

        foreach (var suffix in new[] { "", "-shm", "-wal" })
        {
            var file = _tempDbPath + suffix;
            if (File.Exists(file))
            {
                try { File.Delete(file); } catch { /* ігноруємо блокування SQLite */ }
            }
        }

        GC.SuppressFinalize(this);
    }
}
