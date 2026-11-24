using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfCatalogStore(AppDbContext db) : IBookStore
{
    private static Book ToDomain(BookRow row) =>
        Book.Create(row.Id, row.Isbn, row.Title, row.Year, row.Author, row.AvailableCopies);

    private static BookRow ToRow(Book b) => new()
    {
        Id = b.Id,
        Isbn = b.Isbn,
        Title = b.Title,
        Year = b.Year,
        Author = b.Author,
        AvailableCopies = b.AvailableCopies
    };

    public IReadOnlyList<Book> List() =>
        db.Books.AsNoTracking().OrderBy(x => x.Isbn).AsEnumerable().Select(ToDomain).ToList();

    public Book? GetById(string id) =>
        db.Books.AsNoTracking().SingleOrDefault(x => x.Id == id) is { } row ? ToDomain(row) : null;

    public IReadOnlyList<Book> Find(Func<Book, bool> predicate) =>
        db.Books.AsNoTracking().AsEnumerable().Select(ToDomain).Where(predicate).ToList();

    public void Add(Book book)
    {
        db.Books.Add(ToRow(book));
        db.SaveChanges();
    }

    public void Update(Book book)
    {
        var row = db.Books.Single(x => x.Id == book.Id);
        row.Isbn = book.Isbn;
        row.Title = book.Title;
        row.Year = book.Year;
        row.Author = book.Author;
        row.AvailableCopies = book.AvailableCopies;
        db.SaveChanges();
    }

    public bool Remove(string id)
    {
        var row = db.Books.SingleOrDefault(x => x.Id == id);
        if (row is null) return false;
        db.Books.Remove(row);
        db.SaveChanges();
        return true;
    }
}
