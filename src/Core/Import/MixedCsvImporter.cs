using Core.Dto;

namespace Core.Import;

public record MixedImportResult(
    IReadOnlyList<BookDto> Books,
    IReadOnlyList<ReaderDto> Readers,
    IReadOnlyList<string> Errors);

public static class MixedCsvImporter
{
    public static MixedImportResult Load(string path)
    {
        var books = new List<BookDto>();
        var readers = new List<ReaderDto>();
        var errors = new List<string>();

        int lineNumber = 0;

        foreach (string rawLine in File.ReadLines(path))
        {
            lineNumber++;
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || lineNumber == 1)
                continue;

            string[] parts = line.Split(';');

            object? parsed = parts switch
            {
                ["B", string id, string isbn, string title, string yearStr, ..]
                    when !string.IsNullOrWhiteSpace(isbn) && !string.IsNullOrWhiteSpace(title) =>
                        int.TryParse(yearStr, out int y) && y is >= 1450 and <= 2026
                            ? new BookDto(id, isbn, title, y, parts.Length > 5 ? parts[5] : null)
                            : $"рядок {lineNumber}: некоректний рік '{yearStr}'",

                ["R", string id, string name, string cardNumber, ..]
                    when !string.IsNullOrWhiteSpace(name) =>
                        new ReaderDto(id, name, cardNumber),

                ["B", ..] => $"рядок {lineNumber}: пошкоджений запис книги",
                ["R", ..] => $"рядок {lineNumber}: пошкоджений запис читача (ім'я порожнє)",
                _ => $"рядок {lineNumber}: невідомий префікс запису '{parts[0]}'"
            };

            switch (parsed)
            {
                case BookDto book:
                    books.Add(book);
                    break;
                case ReaderDto reader:
                    readers.Add(reader);
                    break;
                case string error:
                    errors.Add(error);
                    break;
            }
        }

        return new MixedImportResult(books, readers, errors);
    }
}