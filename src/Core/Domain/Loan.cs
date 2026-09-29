namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public string BookId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }

    public LoanStatus Status { get; private set; }

    public bool IsClosed => Status == LoanStatus.Returned || Status == LoanStatus.Lost;

    private Loan(string id, string bookId, string readerId, DateTime issuedOn, DateTime? returnedOn)
    {
        Id = id;
        BookId = bookId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
    }

// Додаткове 4.2: Інваріант для двох сутностей: перевірка ліміту відкритих видач читача
    public static Loan OpenForReader(
        string id, 
        Book book, 
        string readerId, 
        IReadOnlyList<Loan> readerActiveLoans, 
        DateTime issuedOn)
    {
        ArgumentNullException.ThrowIfNull(readerActiveLoans);

        int activeCount = readerActiveLoans.Count(l => l.ReaderId == readerId && !l.IsClosed);
        if (activeCount >= 5)
        {
            throw new InvalidOperationException($"Читач {readerId} уже має {activeCount} активних видач (ліміт: 5). Нова видача заборонена.");
        }

        return Open(id, book, readerId, issuedOn);
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

	TransitionTo(LoanStatus.Returned);

        book.ReturnCopy();
        ReturnedOn = returnedOn;
    }

// Додаткове 4.3
    public void TransitionTo(LoanStatus newStatus)
    {
        bool isValid = (Status, newStatus) switch
        {
            // З Active можна перейти в Overdue, Returned або Lost
            (LoanStatus.Active, LoanStatus.Overdue)  => true,
            (LoanStatus.Active, LoanStatus.Returned) => true,
            (LoanStatus.Active, LoanStatus.Lost)     => true,

            // З Overdue (прострочено) книгу все ще можна повернути або оголосити втраченою
            (LoanStatus.Overdue, LoanStatus.Returned) => true,
            (LoanStatus.Overdue, LoanStatus.Lost)     => true,

            // Кінцеві стани: повернену чи втрачену книгу не можна змінювати
            (LoanStatus.Returned, _) => false,
            (LoanStatus.Lost, _)     => false,

            // Перехід у той самий статус нічого не змінює
            _ when Status == newStatus => true,

            _ => false
        };

        if (!isValid)
        {
            throw new InvalidOperationException($"Неприпустимий перехід статусу видачі: з '{Status}' у '{newStatus}'.");
        }

        Status = newStatus;
    }

    public override string ToString() =>
        $"Видача {Id}: Книга {BookId} -> Читач {ReaderId}, від {IssuedOn:yyyy-MM-dd} (Статус: {(IsClosed ? $"Повернено {ReturnedOn:yyyy-MM-dd}" : "Активна")})";
}