using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    Task<IReadOnlyList<Book>> LoadAsync(string path, CancellationToken ct = default);

// Додаткове завдання 8.1: потокове читання сутностей
    IAsyncEnumerable<Book> ReadStreamAsync(string path, CancellationToken ct = default, IProgress<double>? progress = null);
}