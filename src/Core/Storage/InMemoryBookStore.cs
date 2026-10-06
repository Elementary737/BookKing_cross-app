using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryBookStore(IEnumerable<Book>? seed = null) : IBookStore
{
    private readonly Dictionary<string, Book> _items =
        (seed ?? []).ToDictionary(b => b.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Book> List() => _items.Values.ToList();

    public Book? GetById(string id) => _items.GetValueOrDefault(id);

    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (_items.ContainsKey(book.Id))
            throw new InvalidOperationException($"Книга з id={book.Id} уже існує.");

        _items.Add(book.Id, book);
    }

    public IReadOnlyList<Book> Find(Func<Book, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }

    public void Update(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        _items[book.Id] = book;
    }

    public bool Remove(string id) => _items.Remove(id);
}