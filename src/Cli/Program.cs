using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Core.Events;
using Cli;

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

Console.WriteLine("\n=== Додаткове завдання 1: INotificationSink через DI ===");

var sinkStore = new InMemoryBookStore();
var sinkLogPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "sink_notifications.log");

var compositeSink = new CompositeNotificationSink(
    new ConsoleSink(),
    new FileSink(sinkLogPath)
);

var sinkService = new LendingService(sinkStore, compositeSink);

sinkService.AddBook("978-0063279070", "Усі в моїй родині вбивці", 2022, "Бенджамін Стівенсон", 4);

Console.WriteLine("Сповіщення збережено у файл: logs/sink_notifications.log");

// ДОДАТКОВЕ ЗАВДАННЯ 2: ImportBooks з Action-колбеком помилок
Console.WriteLine("\n=== Додаткове завдання 2: ImportBooks з Action-колбеком ===");

var importStore = new InMemoryBookStore();
var importService = new LendingService(importStore);

var rawCsvLines = new List<string>
{
    "978-0063279070;Усі в моїй родині вбивці;2022;Бенджамін Стівенсон;4",
    "978-0063348493;Кожен у цьому поїзді підозрюваний;bad_year;Бенджамін Стівенсон;2",
    "   ",
    "978-0008305888;Убивства за абеткою;1936;Аґата Крісті;-1",
    "978-0525658184;Клуб убивств по четвергам;2020;Річард Осман;3"
};

int skipped = 0;
int imported = importService.ImportBooks(rawCsvLines, (rowNum, lineText, reason) =>
{
    skipped++;
    Console.WriteLine($"  [Рядок {rowNum} відхилено]: {reason} -> \"{lineText.Trim()}\"");
});

Console.WriteLine($"\nПідсумок: успішно імпортовано {imported}, пропущено {skipped}");
return 0;
