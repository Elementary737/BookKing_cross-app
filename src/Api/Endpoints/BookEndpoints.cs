using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Validation;
using Core.Exceptions;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        // 1. GET /api/books ( Додаткове 1 з опційною фільтрацією за автором)
       group.MapGet("/", (int page = 1, int pageSize = 20, string? search = null, IBookStore store = null!) =>
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

    var query = store.List().AsEnumerable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var s = search.Trim();
        query = query.Where(b => 
            b.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
            (b.Author != null && b.Author.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
            b.Isbn.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    var total = query.Count();
    var items = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(b => b.ToApiDto())
        .ToList();

    return TypedResults.Ok(new PagedResult<BookDto>(items, total, page, pageSize));
});

        // 2. GET /api/books/{id}
        group.MapGet("/{id}", Results<Ok<BookDto>, NotFound> (string id, IBookStore store) =>
            store.GetById(id) is { } book
                ? TypedResults.Ok(book.ToApiDto())
                : TypedResults.NotFound());

        // 3. POST /api/books
        group.MapPost("/", Results<Created<BookDto>, ValidationProblem> (SaveBookRequest request, IBookStore store) =>
        {
            if (store.List().Any(b => b.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase)))
    {
        throw new DomainException("Книга з таким ISBN вже існує в каталозі.");
    }
            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Book.Create(id, request.Isbn, request.Title, request.Year, request.Author, request.Copies);
            store.Add(created);

            return TypedResults.Created($"/api/books/{created.Id}", created.ToApiDto());
        })
	.AddEndpointFilter<ValidationFilter<SaveBookRequest>>();

// 4. PUT /api/books/{id}
group.MapPut("/{id}", Results<Ok<BookDto>, NotFound, ValidationProblem> (string id, SaveBookRequest request, IBookStore store) =>
{
    // Шукаємо книгу в сховищі за ID
    var existingBook = store.GetById(id);
    if (existingBook is null)
        return TypedResults.NotFound();

    // Якщо ISBN змінюють на ISBN ІНШОЇ книги — видаємо помилку валідації
    var isSameIsbn = existingBook.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase);
    if (!isSameIsbn && store.List().Any(b => b.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase)))
    {
        throw new DomainException("Книга з таким ISBN вже існує в каталозі.");
    }

    // Створюємо оновлений доменний об'єкт із тим самим id
    var replacement = Book.Create(existingBook.Id, request.Isbn, request.Title, request.Year, request.Author, request.Copies);
    store.Update(replacement);

    return TypedResults.Ok(replacement.ToApiDto());
})
.AddEndpointFilter<ValidationFilter<SaveBookRequest>>();
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