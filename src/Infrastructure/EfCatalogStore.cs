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

    public void Add(Book item)
    {
        db.Books.Add(ToRow(item));
        db.SaveChanges();
    }

    public void Update(Book item)
    {
        var row = db.Books.Single(x => x.Id == item.Id);
        row.Isbn = item.Isbn;
        row.Title = item.Title;
        row.Year = item.Year;
        row.Author = item.Author;
        row.AvailableCopies = item.AvailableCopies;
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