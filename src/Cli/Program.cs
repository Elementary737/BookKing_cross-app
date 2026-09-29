using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("BookKing - практикум з крос-платформного програмування");
Console.WriteLine("Студентка: Гулай Ірина Костянтинівна, ФЕІ-34");
Console.WriteLine(new string('-',52));

Console.WriteLine($"ОС : {report.OsDescription}"); 
Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}"); 
Console.WriteLine($"Runtime : {report.FrameworkDescription}"); 
Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}"); 
Console.WriteLine($"RID (визначено): {report.DetectedRid}");
Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Бібліотека (книги, примірники, читачі, видачі)");

if (args.Length > 0 && args[0] == "--mixed")
{
    string mixedPath = args.Length > 1 ? args[1] : Path.Combine("data", "mixed.csv");
    if (!File.Exists(mixedPath))
    {
        Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(mixedPath)}");
        return 1;
    }

    var mixed = MixedCsvImporter.Load(mixedPath);

    Console.WriteLine($"Завантажено книг: {mixed.Books.Count}");
    foreach (var b in mixed.Books)
        Console.WriteLine($"  [Книга] {b.Id,-6} {b.Title,-30} ({b.Year})");

    Console.WriteLine($"Завантажено читачів: {mixed.Readers.Count}");
    foreach (var r in mixed.Readers)
        Console.WriteLine($"  [Читач] {r.Id,-6} {r.FullName,-20} квиток: {r.CardNumber}");

    if (mixed.Errors.Count > 0)
    {
        Console.WriteLine($"Пропущено рядків: {mixed.Errors.Count}");
        foreach (var err in mixed.Errors)
            Console.WriteLine($"  ! {err}");
    }

    int totalMixed = mixed.Books.Count + mixed.Readers.Count + mixed.Errors.Count;
    double mixedErrorRate = totalMixed > 0 ? (double)mixed.Errors.Count / totalMixed * 100 : 0.0;
    Console.WriteLine($"Статистика: усього {totalMixed} | прийнято {mixed.Books.Count + mixed.Readers.Count} | пропущено {mixed.Errors.Count} | помилок {mixedErrorRate:F1}%");

    return 0;
}

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<BookDto>? result =
    Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => BookCsvImporter.Load(path),
        ".json" => BookJsonImporter.Load(path),
        _ => null
    };

if (result is null)
{
    Console.WriteLine($"Помилка: формат файлу '{Path.GetExtension(path)}' не підтримується.");
    return 1;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (BookDto b in result.Items.Take(5))
{
    Console.WriteLine(
        $"  {b.Id,-6} {b.Isbn,-16} {b.Title,-32} {b.Year,4}  {b.Author ?? "-"}");
}
if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
        Console.WriteLine($"  ! {error}");
}

int total = result.Items.Count + result.Errors.Count;
double errorRate = total > 0
    ? (double)result.Errors.Count / total * 100
    : 0.0;

Console.WriteLine(
    $"Статистика: усього {total} | прийнято {result.Items.Count} | " +
    $"пропущено {result.Errors.Count} | помилок {errorRate:F1}%");

Console.WriteLine();
Console.WriteLine("ЛАБОРАТОРНА РОБОТА 4: Доменна модель та інваріанти");

Console.WriteLine("\n=== Сценарій 1: Успіх ===");
// Використовуємо першу імпортовану книгу або створюємо зразкову
Book book = result.Items.Count > 0 
    ? Book.FromDto(result.Items[0], availableCopies: 1)
    : Book.Create("B-001", "978-0-13-235088-4", "Clean Code", 2008, "Robert Martin", availableCopies: 1);

Console.WriteLine($"Створено доменну сутність: {book}");

// Фіксуємо чіткі дати без звернення до DateTime.Now (наприклад, видача 1 вересня, повернення 15 вересня):
DateTime issueDate = new DateTime(2026, 9, 1);
DateTime returnDate = new DateTime(2026, 9, 15);

Loan loan = Loan.Open("L-001", book, "R-101", issueDate);
Console.WriteLine($"Книгу видано читачу:");
Console.WriteLine($"  {loan}");
Console.WriteLine($"  Стан книги: {book}");

loan.Close(book, returnDate);
Console.WriteLine($"Книгу повернено:");
Console.WriteLine($"  {loan}");
Console.WriteLine($"  Стан книги: {book}");

Console.WriteLine("\n=== Сценарій 2: Порушення інваріантів ===");

// 1. Порожній ISBN (ArgumentException)
TryDo("Створення книги з порожнім ISBN", () => 
    Book.Create("B-002", "   ", "Title", 2020));

// 2. Некоректний рік видання (ArgumentOutOfRangeException)
TryDo("Створення книги з некоректним роком (з майбутнього)", () => 
    Book.Create("B-003", "978-1-23-456789-0", "Future Book", 2099));

// 3. Спроба видати книгу, коли примірники закінчилися (InvalidOperationException)
Book singleBook = Book.Create("B-004", "978-0-201-48567-7", "DDD", 2003, availableCopies: 0);
TryDo("Видача відсутнього примірника (залишок 0)", () => 
    singleBook.IssueCopy());

// 4. Повернення раніше дати видачі (ArgumentOutOfRangeException)
Book activeBook = Book.Create("B-005", "978-0-13-449416-6", "Architecture", 2017, availableCopies: 1);
Loan activeLoan = Loan.Open("L-002", activeBook, "R-102", new DateTime(2026, 9, 1));
TryDo("Дата повернення раніше дати видачі", () => 
    activeLoan.Close(activeBook, new DateTime(2026, 8, 20)));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [!] {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [OK] {title}: {ex.GetType().Name} — {ex.Message}");
    }
}
return 0;