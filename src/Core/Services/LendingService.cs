using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public event EventHandler<LibraryChangedEventArgs>? Changed;

    private void OnChanged(string copyId, LibraryChangeKind kind, string? readerId = null, DateTimeOffset? dueDate = null) =>
        Changed?.Invoke(this, new LibraryChangedEventArgs(copyId, kind, readerId, dueDate));

    public Book AddBook(string isbn, string title, int year, string? author, int availableCopies = 1)
    {
        var existingBook = _store.List()
            .FirstOrDefault(b => string.Equals(b.Isbn, isbn, StringComparison.OrdinalIgnoreCase));

        if (existingBook is not null)
        {
            existingBook.AddCopies(availableCopies);
            _store.Update(existingBook);
	    OnChanged(existingBook.Id, LibraryChangeKind.BookAdded);
            return existingBook;
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var book = Book.Create(id, isbn, title, year, author, availableCopies);
        _store.Add(book);
	OnChanged(book.Id, LibraryChangeKind.BookAdded);
        return book;
    }

    public void IssueCopy(string id, string readerId, DateTimeOffset dueDate)
    {
        var book = _store.GetById(id)
            ?? throw new InvalidOperationException($"Книгу з id={id} не знайдено.");

        book.IssueCopy();
        _store.Update(book);
	OnChanged(book.Id, LibraryChangeKind.CopyIssued, readerId, dueDate);
    }

    public void ReturnCopy(string id)
    {
        var book = _store.GetById(id)
            ?? throw new InvalidOperationException($"Книгу з id={id} не знайдено.");

        book.ReturnCopy();
        _store.Update(book);
	OnChanged(book.Id, LibraryChangeKind.CopyReturned);
    }

    public IReadOnlyList<Book> All() => _store.List();

    public Book? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Book> Find(Func<Book, bool> predicate) => _store.Find(predicate);

    public IReadOnlyList<Book> FindAll(Func<Book, bool> predicate) => Find(predicate);

}