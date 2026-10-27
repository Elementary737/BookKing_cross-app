using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

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
}