using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Core.Events;
using Cli;
using Core.Reports;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("BookKing - практикум з крос-платформного програмування");
Console.WriteLine("Студентка: Гулай Ірина Костянтинівна, ФЕІ-34");
Console.WriteLine("Предметна область: Бібліотека (книги, примірники, читачі, видачі)");
Console.WriteLine(new string('-', 52));

var books = SampleData.Books();
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top":
    {
        var top = ReportService.TopNewest(books, 5);
        Console.WriteLine($"[ЗВІТ] Топ найновіших видань у каталозі ({top.Count} позицій):");
        Console.WriteLine(new string('-', 72));
        PrintBooks(top);
        break;
    }

    case "report-low":
    {
        const int thresholdYear = 1950;
        var vintage = ReportService.VintageBooks(books, thresholdYear);
        Console.WriteLine($"[ЗВІТ] Знайдено {vintage.Count} раритетних видань (до {thresholdYear} року):");
        Console.WriteLine(new string('-', 72));
        PrintBooks(vintage);
        break;
    }

    case "report-groups":
    {
        Console.WriteLine("[ЗВІТ 1] Розподіл книжкового фонду за авторами:");
        Console.WriteLine(new string('-', 60));
        Console.WriteLine($"{"Автор",-26} | {"Назв",5} | {"Примірників",12}");
        Console.WriteLine(new string('-', 60));
        var byAuthor = ReportService.ByAuthor(books);
        foreach (var g in byAuthor)
            Console.WriteLine($"{g.Key,-26} | {g.Count,5} | {g.TotalCopies,12}");

        Console.WriteLine();
        Console.WriteLine("[ЗВІТ 2] Хронологічний розподіл фонду за десятиліттями:");
        Console.WriteLine(new string('-', 45));
        Console.WriteLine($"{"Період",-12} | {"Назв",5} | {"Примірників",12}");
        Console.WriteLine(new string('-', 45));
        var byDecade = ReportService.ByDecade(books);
        foreach (var g in byDecade)
            Console.WriteLine($"{g.Key,-12} | {g.Count,5} | {g.TotalCopies,12}");

        break;
    }
    case "report-sum":
    {
        var t = ReportService.Totals(books);
        Console.WriteLine("[ЗВІТ] Загальні підсумки та статистика книжкового фонду:");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine($"• Унікальних назв у каталозі:      {t.Titles}");
        Console.WriteLine($"• Загальний фонд примірників:      {t.TotalCopies} шт.");
        Console.WriteLine($"• Мінімум примірників на назву:    {t.MinCopies} шт.");
        Console.WriteLine($"• Середня кількість на назву:      {t.AverageCopies:F1} шт.");
        Console.WriteLine($"• Максимум примірників в однієї:   {t.MaxCopies} шт.");
        Console.WriteLine($"• Хронологічний діапазон фонду:    {t.MinYear} – {t.MaxYear} рр.");
        break;
    }

    case "report-demo":
    {
        Console.WriteLine("[ДОСЛІДЖЕННЯ] Демонстрація механізму відкладеного виконання (Deferred Execution):");
        Console.WriteLine(new string('-', 72));
        RunDeferredDemo();
        break;
    }

    case "report-join":
    {
        Console.WriteLine("[ДОДАТКОВЕ 1] Зведений звіт GroupJoin (Видання x Видачі):");
        Console.WriteLine(new string('-', 76));
        Console.WriteLine($"{"Назва книги",-36} | {"ISBN",-15} | {"Всього",6} | {"Активних",8}");
        Console.WriteLine(new string('-', 76));

        var bookOsm = books.First(b => b.Id == "B-004");
        var bookOmer = books.First(b => b.Id == "B-001");
        var bookBeckett = books.First(b => b.Id == "B-009");

        var sampleLoans = new List<Loan>
        {
            Loan.Open("L-001", bookOsm, "R-01", DateTime.Now.AddDays(-10)),
            Loan.Open("L-002", bookOsm, "R-02", DateTime.Now.AddDays(-5)),
            Loan.Open("L-003", bookOmer, "R-01", DateTime.Now.AddDays(-2)),
            Loan.Open("L-004", bookBeckett, "R-03", DateTime.Now.AddDays(-20))
        };

        var joinReport = ReportService.BookLoansReport(books, sampleLoans);
        foreach (var r in joinReport)
            Console.WriteLine($"{r.BookTitle,-36} | {r.Isbn,-15} | {r.TotalLoansCount,6} | {r.ActiveLoansCount,8}");

        break;
    }

    case "report-ext":
    {
        Console.WriteLine("[ДОДАТКОВЕ 2] Власний метод-розширення TopBy у 2 звітах:");
        Console.WriteLine(new string('-', 76));

        Console.WriteLine("1. Топ-3 книги за найбільшою кількістю примірників (TopBy b.AvailableCopies):");
        Console.WriteLine(new string('-', 76));
        var topCopies = ReportService.TopCopiesByExtension(books, 3);
        PrintBooks(topCopies);

        Console.WriteLine();
        Console.WriteLine("2. Топ-3 найновіших книги фонду (TopBy b.Year):");
        Console.WriteLine(new string('-', 76));
        var topNewest = ReportService.TopNewestByExtension(books, 3);
        PrintBooks(topNewest);

        break;
    }

    case "report-syntax":
    {
        Console.WriteLine("[ДОДАТКОВЕ 3] Звіт за десятиліттями у синтаксисі запитів (Query Syntax):");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"{"Період",-12} | {"Назв",5} | {"Примірників",12}");
        Console.WriteLine(new string('-', 50));

        var querySyntaxResult = ReportService.ByDecadeQuerySyntax(books);
        foreach (var g in querySyntaxResult)
            Console.WriteLine($"{g.Key,-12} | {g.Count,5} | {g.TotalCopies,12}");

        break;
    }

    case "report-bench":
    {
        Console.WriteLine("[ДОДАТКОВЕ 4] Порівняння продуктивності Stopwatch (100 000 об'єктів):");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("Генерація 100 000 записів та виконання фільтрації...");

        var (withListMs, directMs) = ReportService.BenchmarkToList(100_000);

        Console.WriteLine($"• Варіант ІЗ проміжним ToList() : {withListMs,4} мс");
        Console.WriteLine($"• Варіант БЕЗ ToList() (стрім)  : {directMs,4} мс");
        Console.WriteLine(new string('-', 72));

        var diff = Math.Abs(withListMs - directMs);
        if (directMs < withListMs)
        {
            Console.WriteLine($"Результат: варіант БЕЗ ToList() швидший на {diff} мс.");
        }
        else if (withListMs < directMs)
        {
            Console.WriteLine($"Результат: варіант ІЗ ToList() швидший на {diff} мс.");
        }
        else
        {
            Console.WriteLine("Результат: час виконання обох варіантів однаковий.");
        }

        break;
    }

    default:
        Console.WriteLine("Невідома команда. Доступні опції:");
        Console.WriteLine("  report-top    - топ найновіших видань");
        Console.WriteLine("  report-low    - фільтр раритетних книг до 1950 року");
        Console.WriteLine("  report-groups - аналітичне групування (за авторами та десятиліттями)");
        Console.WriteLine("  report-sum    - підсумкова статистика фонду");
        Console.WriteLine("  report-demo   - перевірка відкладеного виконання LINQ");
	Console.WriteLine("  report-join   - зведений звіт GroupJoin (видання x видачі)");
	Console.WriteLine("  report-ext    - перевірка власного методу-розширення TopBy");
	Console.WriteLine("  report-syntax - звіт за десятиліттями у синтаксисі запитів (Query Syntax)");
	Console.WriteLine("  report-bench  - бенчмарк Stopwatch на 100 000 записах (вплив ToList)");
        break;
}


return 0;

static void PrintBooks(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($"{l.Isbn,-16} {l.Title,-32} {l.Author ?? "-",-20} {l.Year} р. ({l.AvailableCopies} прим.)");
}

static void RunDeferredDemo()
{
    var localBooks = SampleData.Books().ToList();

    var query = localBooks.Where(b => b.Year < 1950);
    Console.WriteLine($"1. Оголошено query (Where b.Year < 1950). Раритетних до змін: {query.Count()}");

    Console.WriteLine("2. Додано новий елемент у джерело localBooks.Add(...)...");
    localBooks.Add(Book.Create("B-TEMP", "978-0000000000", "Стародавній манускрипт", 1850, "Невідомий"));
    Console.WriteLine($"3. Повторний query.Count(): {query.Count()} (запит заново пройшов по джерелу)");

    Console.WriteLine("4. Матеріалізація результату у фіксований snapshot через query.ToList()...");
    var snapshot = query.ToList();

    Console.WriteLine("5. Повне очищення вихідного списку localBooks.Clear()...");
    localBooks.Clear();

    Console.WriteLine($"6. Залишилось у snapshot.Count: {snapshot.Count} (матеріалізовані дані збережено)");
}