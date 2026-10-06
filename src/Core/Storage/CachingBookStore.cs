using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingBookStore(IBookStore inner) : IBookStore
{
    private readonly IBookStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, Book> _cache = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Book> List() => _inner.List();

    public Book? GetById(string id)
    {
        if (_cache.TryGetValue(id, out var cached))
            return cached;

        var book = _inner.GetById(id);
        if (book is not null)
            _cache[id] = book;

        return book;
    }

    public void Add(Book book)
    {
        _inner.Add(book);
        _cache[book.Id] = book;
    }

    public void Update(Book book)
    {
        _inner.Update(book);
        _cache[book.Id] = book;
    }

    public bool Remove(string id)
    {
        _cache.Remove(id);
        return _inner.Remove(id);
    }

    public IReadOnlyList<Book> Find(Func<Book, bool> predicate) => _inner.Find(predicate);
}