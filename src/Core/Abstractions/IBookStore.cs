using Core.Domain;

namespace Core.Abstractions;

public interface IBookStore
{
    IReadOnlyList<Book> List();
    Book? GetById(string id);
    void Add(Book book);
    void Update(Book book);
    bool Remove(string id);
    
    IReadOnlyList<Book> Find(Func<Book, bool> predicate);
}