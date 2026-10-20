using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService(IBookStore store, INotificationSink? sink = null)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly INotificationSink? _sink = sink;

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
	    _sink?.Notify($"Книга [{existingBook.Id}]: додано {availableCopies} примірників");
            return existingBook;
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var book = Book.Create(id, isbn, title, year, author, availableCopies);
        _store.Add(book);
	OnChanged(book.Id, LibraryChangeKind.BookAdded);
	_sink?.Notify($"Книга [{id}]: зареєстровано в системі ({title})");
        return book;
    }

    public void IssueCopy(string id, string readerId, DateTimeOffset dueDate)
    {
        var book = _store.GetById(id)
            ?? throw new InvalidOperationException($"Книгу з id={id} не знайдено.");

        book.IssueCopy();
        _store.Update(book);
	OnChanged(book.Id, LibraryChangeKind.CopyIssued, readerId, dueDate);
	_sink?.Notify($"Книга [{id}]: видано примірник");
    }

    public void ReturnCopy(string id)
    {
        var book = _store.GetById(id)
            ?? throw new InvalidOperationException($"Книгу з id={id} не знайдено.");

        book.ReturnCopy();
        _store.Update(book);
	OnChanged(book.Id, LibraryChangeKind.CopyReturned);
	_sink?.Notify($"Книга [{id}]: повернуто примірник");
    }

    public IReadOnlyList<Book> All() => _store.List();

    public Book? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Book> Find(Func<Book, bool> predicate) => _store.Find(predicate);

    public IReadOnlyList<Book> FindAll(Func<Book, bool> predicate) => Find(predicate);

// Додаткове завдання 2: Пакетний імпорт із колбеком помилок Action
    public int ImportBooks(IEnumerable<string> rawLines, Action<int, string, string>? onRowSkipped = null)
    {
        int imported = 0;
        int lineNumber = 0;

        foreach (var line in rawLines)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                onRowSkipped?.Invoke(lineNumber, line, "Порожній рядок");
                continue;
            }

            var parts = line.Split(';');
            if (parts.Length < 5)
            {
                onRowSkipped?.Invoke(lineNumber, line, "Недостатньо колонок (очікується 5)");
                continue;
            }

            var isbn = parts[0].Trim();
            var title = parts[1].Trim();
            var author = parts[3].Trim();

            if (!int.TryParse(parts[2].Trim(), out var year) || !int.TryParse(parts[4].Trim(), out var copies))
            {
                onRowSkipped?.Invoke(lineNumber, line, "Некоректний числовий формат (рік або кількість)");
                continue;
            }

            if (copies < 0)
            {
                onRowSkipped?.Invoke(lineNumber, line, "Кількість примірників не може бути від'ємною");
                continue;
            }

            AddBook(isbn, title, year, author, copies);
            imported++;
        }

        return imported;
    }
}