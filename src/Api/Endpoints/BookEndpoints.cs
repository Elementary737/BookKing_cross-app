using System.Security.Cryptography;
using Api.Validation;
using Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        // 1. GET /api/books
        group.MapGet("/", (int page = 1, int pageSize = 20, string? search = null, string? author = null, string? sortBy = null, IBookStore store = null!) =>
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

            if (!string.IsNullOrWhiteSpace(author))
            {
                var a = author.Trim();
                query = query.Where(b => b.Author != null && b.Author.Contains(a, StringComparison.OrdinalIgnoreCase));
            }

            query = sortBy switch
            {
                "title" => query.OrderBy(b => b.Title, StringComparer.OrdinalIgnoreCase),
                "author" => query.OrderBy(b => b.Author, StringComparer.OrdinalIgnoreCase),
                "copies" => query.OrderByDescending(b => b.AvailableCopies),
                _ => query.OrderBy(b => b.Isbn)
            };

            var total = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => b.ToApiDto())
                .ToList();

            return TypedResults.Ok(new PagedResult<BookDto>(items, total, page, pageSize));
        });

        // 2. GET /api/books/{id}
        group.MapGet("/{id}", Results<Ok<BookDto>, ProblemHttpResult> (string id, IBookStore store) =>
            store.GetById(id) is { } book
                ? TypedResults.Ok(book.ToApiDto())
                : TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Book not found",
                    detail: $"Book with id '{id}' was not found."));

        // 3. POST /api/books
        group.MapPost("/", Results<Created<BookDto>, ValidationProblem> (SaveBookRequest request, IBookStore store) =>
        {
            if (store.List().Any(b => b.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                app.Logger.LogWarning("400 Bad Request: Книга з ISBN {Isbn} вже існує в каталозі.", request.Isbn);
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["isbn"] = ["Книга з таким ISBN вже існує в каталозі."]
                });
            }

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Book.Create(id, request.Isbn, request.Title, request.Year, request.Author, request.Copies);
            store.Add(created);

            return TypedResults.Created($"/api/books/{created.Id}", created.ToApiDto());
        })
        .AddEndpointFilter<ValidationFilter<SaveBookRequest>>();

        // 4. PUT /api/books/{id}
        group.MapPut("/{id}", Results<Ok<BookDto>, ProblemHttpResult, ValidationProblem> (string id, SaveBookRequest request, IBookStore store) =>
        {
            var existingBook = store.GetById(id);
            if (existingBook is null)
            {
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Book not found",
                    detail: $"Book with id '{id}' was not found.");
            }

            var isSameIsbn = existingBook.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase);
            if (!isSameIsbn && store.List().Any(b => b.Isbn.Equals(request.Isbn.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                app.Logger.LogWarning("400 Bad Request: Книга з ISBN {Isbn} вже існує в каталозі.", request.Isbn);
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["isbn"] = ["Книга з таким ISBN вже існує в каталозі."]
                });
            }

            var replacement = Book.Create(existingBook.Id, request.Isbn, request.Title, request.Year, request.Author, request.Copies);
            store.Update(replacement);

            return TypedResults.Ok(replacement.ToApiDto());
        })
        .AddEndpointFilter<ValidationFilter<SaveBookRequest>>();

        // 5. DELETE /api/books/{id}
        group.MapDelete("/{id}", Results<NoContent, ProblemHttpResult> (string id, IBookStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Book not found",
                    detail: $"Book with id '{id}' was not found."));
    }

    private static BookDto ToApiDto(this Book b) =>
        new(b.Id, b.Isbn, b.Title, b.Author ?? string.Empty, b.Year, b.AvailableCopies);
}
