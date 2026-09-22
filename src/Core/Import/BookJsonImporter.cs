using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<BookDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var rawItems = JsonSerializer.Deserialize<List<BookDto>>(json, Options) ?? [];

            var validItems = new List<BookDto>();
            var errors = new List<string>();

            for (int i = 0; i < rawItems.Count; i++)
            {
                BookDto book = rawItems[i];
                int index = i + 1;

                if (string.IsNullOrWhiteSpace(book.Isbn) || string.IsNullOrWhiteSpace(book.Title))
                {
                    errors.Add($"елемент #{index} (id: {book.Id}): ISBN або назва порожні");
                }
                else if (book.Year < 1450 || book.Year > 2026)
                {
                    errors.Add($"елемент #{index} (id: {book.Id}): рік '{book.Year}' поза допустимими межами (1450..2026)");
                }
                else
                {
                    validItems.Add(book);
                }
            }

            return new ImportResult<BookDto>(validItems, errors);
        }
        catch (Exception ex)
        {
            return new ImportResult<BookDto>([], [$"Помилка розбору JSON: {ex.Message}"]);
        }
    }
}