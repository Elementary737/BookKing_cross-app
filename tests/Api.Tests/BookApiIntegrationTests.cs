using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public class BookApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BookApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreatesBook_And_GetByLocation_Returns200OK()
    {
        var request = new SaveBookRequest(
            Isbn: "978-617-7866-10-6",
            Title: "Кобзар",
            Author: "Тарас Шевченко",
            Year: 1840,
            Copies: 5
        );

        var postResponse = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var locationHeader = postResponse.Headers.Location;
        Assert.NotNull(locationHeader);

        var getResponse = await _client.GetAsync(locationHeader);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var createdBook = await getResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(createdBook);
        Assert.Equal("Кобзар", createdBook.Title);
    }
}