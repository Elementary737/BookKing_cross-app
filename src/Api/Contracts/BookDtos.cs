namespace Api.Contracts;

public sealed record BookDto(string Id, string Isbn, string Title, string Author, int Year, int Copies);

public sealed record SaveBookRequest(string Isbn, string Title, string Author, int Year, int Copies)
{
    public bool TryValidate(out Dictionary<string, string[]> errors)
    {
        var found = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Isbn))
            found[nameof(Isbn)] = ["ISBN обов'язковий."];

        if (string.IsNullOrWhiteSpace(Title) || Title.Length > 200)
            found[nameof(Title)] = ["Назва обов'язкова (до 200 символів)."];

        if (string.IsNullOrWhiteSpace(Author) || Author.Length > 100)
            found[nameof(Author)] = ["Автор обов'язковий (до 100 символів)."];

        if (Year < 1450 || Year > DateTime.UtcNow.Year)
            found[nameof(Year)] = [$"Рік має бути в межах 1450–{DateTime.UtcNow.Year}."];

        if (Copies < 0)
            found[nameof(Copies)] = ["Кількість примірників не може бути від'ємною."];

        errors = found;
        return found.Count == 0;
    }
}