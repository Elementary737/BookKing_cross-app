using Core.Dto;

namespace Core.Domain;

public sealed class Book
{
    private int _availableCopies;

    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public int Year { get; }
    public string? Author { get; }
    public int AvailableCopies => _availableCopies;

    // Приватний конструктор: пряме створення new Book(...) заборонено
    private Book(string id, string isbn, string title, int year, string? author, int availableCopies)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Year = year;
        Author = author;
        _availableCopies = availableCopies;
    }

    // Фабричний метод: перевіряє всі вхідні дані до виклику конструктора
    public static Book Create(string id, string isbn, string title, int year, string? author = null, int availableCopies = 1)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор книги обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва книги не може бути порожньою", nameof(title));

        if (year < 1450 || year > 2026)
            throw new ArgumentOutOfRangeException(nameof(year), year,
                $"Рік видання має бути в межах від 1450 до 2026");

        if (availableCopies < 0)
            throw new ArgumentOutOfRangeException(nameof(availableCopies), availableCopies,
                "Кількість примірників не може бути від'ємною");

        return new Book(
            id.Trim(),
            isbn.Trim().ToUpperInvariant(),
            title.Trim(),
            year,
            string.IsNullOrWhiteSpace(author) ? null : author.Trim(),
            availableCopies
        );
    }

    // Операція видачі примірника читачу
    public void IssueCopy()
    {
        if (_availableCopies <= 0)
            throw new InvalidOperationException($"Книгу \"{Title}\" [{Isbn}] видати неможливо: немає доступних примірників");

        _availableCopies--;
    }

    // Операція повернення примірника
    public void ReturnCopy()
    {
        _availableCopies++;
    }

    public void AddCopies(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Кількість примірників має бути більшою за 0");

        _availableCopies += count;
    }

    // Мапінг у ваш BookDto і назад
    public BookDto ToDto() => new(Id, Isbn, Title, Year, Author);

    public static Book FromDto(BookDto dto, int availableCopies = 1) =>
        Create(dto.Id, dto.Isbn, dto.Title, dto.Year, dto.Author, availableCopies);

    public override string ToString() =>
        $"{Id} [{Isbn}] \"{Title}\" ({Year}){(Author != null ? $", {Author}" : "")} — доступно: {AvailableCopies} шт.";
}