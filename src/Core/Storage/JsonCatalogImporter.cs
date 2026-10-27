using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using System.Runtime.CompilerServices;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Book>> LoadAsync(string path, CancellationToken ct = default)
    {

    ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        var dtos = await JsonSerializer.DeserializeAsync<List<BookDto>>(
            stream,
	    JsonOptions,
            cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку книг.");

        var result = new List<Book>(dtos.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
	    await Task.Delay(500, ct);

            ct.ThrowIfCancellationRequested();
            try
            {
                result.Add(Book.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }

    public async IAsyncEnumerable<Book> ReadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default,
	IProgress<double>? progress = null)
    {
        ct.ThrowIfCancellationRequested();

	var totalCount = await CountItemsAsync(path, ct);

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        var dtos = JsonSerializer.DeserializeAsyncEnumerable<BookDto>(stream, JsonOptions, ct);

        var index = 0;
        await foreach (var dto in dtos.WithCancellation(ct))
        {
            if (dto is null) continue;

	    index++;
            await Task.Delay(200, ct);

            Book book;
            try
            {
                book = Book.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {index}: {ex.Message}", ex);
            }
	// Додаткове 8.3
	    if (totalCount > 0 && progress is not null)
            {
                var percentage = (double)index / totalCount * 100.0;
                progress.Report(Math.Min(100.0, percentage));
            }

            yield return book;
        }
    }

    private static async Task<int> CountItemsAsync(string path, CancellationToken ct)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return jsonDoc.RootElement.ValueKind == JsonValueKind.Array ? jsonDoc.RootElement.GetArrayLength() : 1;
    }

}