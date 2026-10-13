using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Core.Events;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("BookKing - практикум з крос-платформного програмування");
Console.WriteLine("Студентка: Гулай Ірина Костянтинівна, ФЕІ-34");
Console.WriteLine("Предметна область: Бібліотека (книги, примірники, читачі, видачі)");
Console.WriteLine(new string('-', 52));

Console.WriteLine("\n" + new string('=', 52));
Console.WriteLine("ЛАБОРАТОРНА РОБОТА 6: Доменні події та делегати");
Console.WriteLine(new string('=', 52));

var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

var store = new InMemoryBookStore();
var service = new LendingService(store);

void OnLibraryChanged(object? sender, LibraryChangedEventArgs e)
{
    var message = e.Kind switch
    {
        LibraryChangeKind.CopyIssued => $"видано читачу {e.ReaderId} до {e.DueDate:yyyy-MM-dd}",
        LibraryChangeKind.CopyReturned => "повернено до фонду",
        _ => "додано новий примірник"
    };

    var line = $"{e.At:HH:mm:ss} {e.Kind,-13} [{e.CopyId}] {message}";
    Console.WriteLine($"  [Event Log] {line}");
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnLibraryChanged;

// Крок 4: Другий підписник
int changes = 0;
service.Changed += (_, _) => changes++;

Console.WriteLine("\n--- Крок 5: Додавання та видача (працюють 2 підписники) ---");
var book = service.AddBook("978-0132350884", "Clean Architecture", 2017, "Robert Martin", 3);

var dueDate = new DateTimeOffset(2026, 10, 28, 18, 0, 0, TimeSpan.FromHours(3));
service.IssueCopy(book.Id, "READER-042", dueDate);

Console.WriteLine($"Подій зафіксовано лічильником: {changes}");

Console.WriteLine("\n--- Відписка локального методу (-=) та тиха операція ---");
service.Changed -= OnLibraryChanged;

service.ReturnCopy(book.Id);
Console.WriteLine($"Подій зафіксовано лічильником: {changes}"); 

Console.WriteLine("\n--- Крок 6: Сценарій відмови (інваріант захищає подію) ---");
try
{
    service.IssueCopy("non-existent-id", "READER-99", dueDate);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[Спіймано виняток]: {ex.Message}");
}

service.Changed -= OnLibraryChanged;

Console.WriteLine($"Подій після помилки: {changes}");

Console.WriteLine("\n--- Крок 7: Використання FindAll з Func-предикатом ---");

service.AddBook("978-0201485677", "Refactoring", 1999, "Martin Fowler", 0);
service.AddBook("978-0321125217", "Domain-Driven Design", 2003, "Eric Evans", 5);

var allBooks = store.List();
Console.WriteLine($"Усього книг у каталозі: {allBooks.Count}");
foreach (var b in allBooks)
{
    Console.WriteLine($"  * [{b.Id}] \"{b.Title}\" — Доступно: {b.AvailableCopies} шт.");
}

var availableBooks = service.FindAll(b => b.AvailableCopies > 0);

Console.WriteLine($"\nЗнайдено з залишком > 0: {availableBooks.Count}");
foreach (var b in availableBooks)
{
    Console.WriteLine($"  * [{b.Id}] \"{b.Title}\" — Доступно: {b.AvailableCopies} шт.");
}
return 0;
