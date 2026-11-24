using System.Net;
using System.Net.Http.Json;
using Contracts;

namespace Web.Services;

public sealed class BooksApiClient(HttpClient http)
{
    public async Task<List<BookModel>> ListAsync(string? author = null, string? sortBy = null, CancellationToken ct = default)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(author))
            queryParams.Add($"author={Uri.EscapeDataString(author)}");

        if (!string.IsNullOrWhiteSpace(sortBy))
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

        var url = queryParams.Count > 0
            ? $"api/books?{string.Join("&", queryParams)}"
            : "api/books";

        var page = await http.GetFromJsonAsync<PagedResult<BookModel>>(url, ct);
        return page?.Items ?? [];
    }

    public async Task<BookModel?> FindAsync(string id, CancellationToken ct = default)
    {
        using var response = await http.GetAsync($"api/books/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BookModel>(ct);
    }

    public Task<ApiResult> CreateAsync(BookModel model, CancellationToken ct = default)
    {
        return SendAsync(HttpMethod.Post, "api/books", model, ct);
    }

    public Task<ApiResult> UpdateAsync(BookModel model, CancellationToken ct = default)
    {
        return SendAsync(HttpMethod.Put, $"api/books/{model.Id}", model, ct);
    }

// [Додаткове 1] Надсилання DELETE-запиту на сервер
    public async Task<ApiResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        try
        {
            using var response = await http.DeleteAsync($"api/books/{id}", ct);
            if (response.IsSuccessStatusCode)
                return new ApiResult(true);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new ApiResult(false, "Видання не знайдено — можливо, його вже видалили.");

            return new ApiResult(false, $"Помилка видалення: {(int)response.StatusCode}.");
        }
        catch (HttpRequestException)
        {
            return new ApiResult(false, "Api недоступний. Перевірте, чи запущено src/Api.");
        }
    }

    private async Task<ApiResult> SendAsync(HttpMethod method, string url, BookModel model, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(model)
            };
            using var response = await http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResult(true);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ApiResult(false, "Видання не знайдено — можливо, його вже видалили.");
            }

            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                return new ApiResult(false, $"Сервер повернув помилку {(int)response.StatusCode}.");
            }

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(ct);
            return new ApiResult(false, problem?.Title ?? "Дані не пройшли перевірку.", problem?.Errors);
        }
        catch (HttpRequestException)
        {
            return new ApiResult(false, "Api недоступний. Перевірте, чи запущено src/Api.");
        }
    }
}