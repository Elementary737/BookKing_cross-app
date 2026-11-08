using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        // 1. GET /api/books (з опційною фільтрацією за автором)
        group.MapGet("/", (string? author, IBookStore store) =>
        {
            var books = store.List();
            if (!string.IsNullOrWhiteSpace(author))
            {
                books = books.Where(b => b.Author != null && b.Author.Contains(author, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return TypedResults.Ok(books.Select(b => b.ToApiDto()).ToList());
        });

        // 2. GET /api/books/{id}
        group.MapGet("/{id}", Results<Ok<BookDto>, NotFound> (string id, IBookStore store) =>
            store.GetById(id) is { } book
                ? TypedResults.Ok(book.ToApiDto())
                : TypedResults.NotFound());

        // 3. POST /api/books
        group.MapPost("/", Results<Created<BookDto>, ValidationProblem> (SaveBookRequest request, IBookStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            // Перевірка унікальності ISBN (за бажанням)
            if (store.List().Any(b => b.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Isbn)] = ["Книга з таким ISBN вже існує в каталозі."]
                });
            }

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Book.Create(id, request.Isbn, request.Title, request.Year, request.Author, request.Copies);
            store.Add(created);

            return TypedResults.Created($"/api/books/{created.Id}", created.ToApiDto());
        });

// 4. PUT /api/books/{id}
group.MapPut("/{id}", Results<Ok<BookDto>, NotFound, ValidationProblem> (string id, SaveBookRequest request, IBookStore store) =>
{
    if (!request.TryValidate(out var errors))
        return TypedResults.ValidationProblem(errors);

    // Шукаємо книгу в сховищі за ID
    var existingBook = store.GetById(id);
    if (existingBook is null)
        return TypedResults.NotFound();

    // Якщо ISBN змінюють на ISBN ІНШОЇ книги — видаємо помилку валідації
    var isSameIsbn = existingBook.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase);
    if (!isSameIsbn && store.List().Any(b => b.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase)))
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Isbn)] = ["Книга з таким ISBN вже існує в каталозі."]
        });
    }

    // Створюємо оновлений доменний об'єкт із тим самим id
    var replacement = Book.Create(existingBook.Id, request.Isbn, request.Title, request.Year, request.Author, request.Copies);
    store.Update(replacement);

    return TypedResults.Ok(replacement.ToApiDto());
});
        // 5. DELETE /api/books/{id}
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, IBookStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }

    // Мапінг із доменної сутності Book у вихідний BookDto контракту API
    private static BookDto ToApiDto(this Book b) =>
        new(b.Id, b.Isbn, b.Title, b.Author ?? string.Empty, b.Year, b.AvailableCopies);
}