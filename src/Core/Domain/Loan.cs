namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public string BookId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }

    public bool IsClosed => ReturnedOn.HasValue;

    private Loan(string id, string bookId, string readerId, DateTime issuedOn, DateTime? returnedOn)
    {
        Id = id;
        BookId = bookId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
    }

    // Фабрика відкриття видачі
    public static Loan Open(string id, Book book, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));

        ArgumentNullException.ThrowIfNull(book);

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        // Змінюємо стан книги (кине InvalidOperationException, якщо немає примірників)
        book.IssueCopy();

        return new Loan(id.Trim(), book.Id, readerId.Trim(), issuedOn, null);
    }

    // Закриття видачі
    public void Close(Book book, DateTime returnedOn)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (book.Id != BookId)
            throw new InvalidOperationException($"Книга {book.Id} не відповідає запису видачі ({BookId})");

        if (IsClosed)
            throw new InvalidOperationException($"Видача {Id} вже закрита");

        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn,
                "Дата повернення не може бути раніше дати видачі");

        book.ReturnCopy();
        ReturnedOn = returnedOn;
    }

    public override string ToString() =>
        $"Видача {Id}: Книга {BookId} -> Читач {ReaderId}, від {IssuedOn:yyyy-MM-dd} (Статус: {(IsClosed ? $"Повернено {ReturnedOn:yyyy-MM-dd}" : "Активна")})";
}