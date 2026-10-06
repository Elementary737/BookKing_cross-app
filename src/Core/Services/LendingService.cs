using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Book AddBook(string isbn, string title, int year, string? author, int availableCopies = 1)
    {
        var existingBook = _store.List()
            .FirstOrDefault(b => string.Equals(b.Isbn, isbn, StringComparison.OrdinalIgnoreCase));

        if (existingBook is not null)
        {
            existingBook.AddCopies(availableCopies);
            _store.Update(existingBook);
            return existingBook;
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var book = Book.Create(id, isbn, title, year, author, availableCopies);
        _store.Add(book);
        return book;
    }

    public void IssueCopy(string id)
    {
        var book = _store.GetById(id)
            ?? throw new InvalidOperationException($"Книгу з id={id} не знайдено.");

        book.IssueCopy();
        _store.Update(book);
    }

    public void ReturnCopy(string id)
    {
        var book = _store.GetById(id)
            ?? throw new InvalidOperationException($"Книгу з id={id} не знайдено.");

        book.ReturnCopy();
        _store.Update(book);
    }

    public IReadOnlyList<Book> All() => _store.List();

    public Book? Find(string id) => _store.GetById(id);
}