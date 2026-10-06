using System.Text.Json;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class FileBookStore(string path) : IBookStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly Dictionary<string, Book> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    // Внутрішня модель для збереження повного стану включно із залишком
    private sealed record StoredBook(
        string Id,
        string Isbn,
        string Title,
        int Year,
        string? Author,
        int AvailableCopies
    );

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (File.Exists(_path))
        {
            var json = File.ReadAllText(_path);
            var records = JsonSerializer.Deserialize<List<StoredBook>>(json) ?? [];
            foreach (var r in records)
            {
                var book = Book.Create(r.Id, r.Isbn, r.Title, r.Year, r.Author, r.AvailableCopies);
                _cache[book.Id] = book;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var records = _cache.Values
            .Select(b => new StoredBook(b.Id, b.Isbn, b.Title, b.Year, b.Author, b.AvailableCopies))
            .ToList();

        File.WriteAllText(_path, JsonSerializer.Serialize(records, Options));
    }

    public IReadOnlyList<Book> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Book? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        EnsureLoaded();

        if (_cache.ContainsKey(book.Id))
            throw new InvalidOperationException($"Книга з id={book.Id} уже існує.");

        _cache.Add(book.Id, book);
        Flush();
    }

    public void Update(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        EnsureLoaded();
        _cache[book.Id] = book;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;

        Flush();
        return true;
    }
    public IReadOnlyList<Book> Find(Func<Book, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}