using Core;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class CatalogServiceTests
{
    // ТЕСТ 1: Бізнес-інваріант видачі (спроба повторно видати книгу без примірників)
    [Fact]
    public void IssueCopy_WhenNoAvailableCopies_ThrowsDomainException_AndKeepsState()
    {
        var store = new InMemoryBookStore();
        var service = new LendingService(store);
        var book = service.AddBook("ISBN-10", "Чистий код", 2008, "Р. Мартін", 1);
        service.IssueCopy(book.Id, "reader-1", DateTimeOffset.UtcNow.AddDays(14));

        Assert.Throws<DomainException>(() => 
            service.IssueCopy(book.Id, "reader-2", DateTimeOffset.UtcNow.AddDays(14)));
        Assert.Equal(0, store.GetById(book.Id)!.AvailableCopies);
    }

    // ТЕСТ 2: Тест повернення (спроба звернутися до неіснуючої книги або повернути зайве)
    [Fact]
    public void ReturnCopy_WhenBookNotFound_ThrowsDomainException()
    {
        var store = new InMemoryBookStore();
        var service = new LendingService(store);

        Assert.Throws<DomainException>(() => service.ReturnCopy("non-existent-id"));
    }

    // ТЕСТ 3: Тест події каталогу (підняття рівно один раз із типом BookAdded)
    [Fact]
    public void AddBook_RaisesChangedExactlyOnce()
    {
        var service = new LendingService(new InMemoryBookStore());
        var events = new List<LibraryChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.AddBook("ISBN-11", "Рефакторинг", 1999, "М. Фаулер", 1);

        Assert.Single(events);
        Assert.Equal(LibraryChangeKind.BookAdded, events[0].Kind);
    }
}