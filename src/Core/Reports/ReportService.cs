using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1) Фільтрація раритетних видань (до вказаного року)
    public static IReadOnlyList<StockLine> VintageBooks(IEnumerable<Book> source, int beforeYear) =>
        source.Where(b => b.Year < beforeYear)
              .OrderBy(b => b.Year)
              .ThenBy(b => b.Isbn, StringComparer.Ordinal)
              .Select(b => new StockLine(b.Isbn, b.Title, b.Author, b.Year, b.AvailableCopies))
              .ToList();

    // 2) Топ-N найновіших видань
    public static IReadOnlyList<StockLine> TopNewest(IEnumerable<Book> source, int n) =>
        source.OrderByDescending(b => b.Year)
              .ThenBy(b => b.Isbn, StringComparer.Ordinal)
              .Take(n)
              .Select(b => new StockLine(b.Isbn, b.Title, b.Author, b.Year, b.AvailableCopies))
              .ToList();

    // 3a) Групування за авторами
    public static IReadOnlyList<GroupSummary> ByAuthor(IEnumerable<Book> source) =>
        source.GroupBy(b => b.Author ?? "Невідомий автор")
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(b => b.AvailableCopies)))
              .OrderByDescending(s => s.Count)
              .ThenBy(s => s.Key)
              .ToList();

// 3b) Групування за десятиліттями зі строгим хронологічним сортуванням від найдавніших до найновіших
    public static IReadOnlyList<GroupSummary> ByDecade(IEnumerable<Book> source) =>
        source.GroupBy(b => b.Year / 10 * 10)
              .OrderBy(g => g.Key) // Хронологічний порядок: 1880 -> 1890 -> ... -> 2020
              .Select(g => new GroupSummary($"{g.Key}-ті", g.Count(), g.Sum(b => b.AvailableCopies)))
              .ToList();

// 4) Повні агрегати фонду: назви, примірники (сума, min, avg, max) та часові межі
    public static (int Titles, int TotalCopies, int MinCopies, double AverageCopies, int MaxCopies, int MinYear, int MaxYear) Totals(IEnumerable<Book> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0, 0d, 0, 0, 0)
            : (list.Count,
               list.Sum(b => b.AvailableCopies),
               list.Min(b => b.AvailableCopies),
               list.Average(b => b.AvailableCopies),
               list.Max(b => b.AvailableCopies),
               list.Min(b => b.Year),
               list.Max(b => b.Year));
    }
// Додаткове 1: GroupJoin (Книги x Видачі)
    public static IReadOnlyList<BookLoanSummary> BookLoansReport(
        IEnumerable<Book> books,
        IEnumerable<Loan> loans) =>
        books.GroupJoin(
            loans,
            b => b.Id,          // ключ у книзі
            l => l.BookId,      // ключ у видачі
            (book, bookLoans) =>
            {
                var loanList = bookLoans.ToList();
                return new BookLoanSummary(
                    book.Title,
                    book.Isbn,
                    loanList.Count,
                    loanList.Count(l => l.Status == LoanStatus.Active)
                );
            })
            .OrderByDescending(s => s.TotalLoansCount)
            .ThenBy(s => s.BookTitle)
            .ToList();

// Додаткове 2: TopBy у першому звіті (за кількістю примірників)
    public static IReadOnlyList<StockLine> TopCopiesByExtension(IEnumerable<Book> books, int n) =>
        books.TopBy(b => b.AvailableCopies, n)
             .Select(b => new StockLine(b.Isbn, b.Title, b.Author, b.Year, b.AvailableCopies))
             .ToList();

    // TopBy у другому звіті (за роком видання)
    public static IReadOnlyList<StockLine> TopNewestByExtension(IEnumerable<Book> books, int n) =>
        books.TopBy(b => b.Year, n)
             .Select(b => new StockLine(b.Isbn, b.Title, b.Author, b.Year, b.AvailableCopies))
             .ToList();

// Додаткове 3: Синтаксис запитів (Query Syntax: from ... group ... by ... into ...)
    public static IReadOnlyList<GroupSummary> ByDecadeQuerySyntax(IEnumerable<Book> books)
    {
        var query = from b in books
                    group b by b.Year / 10 * 10 into g
                    orderby g.Key ascending
                    select new GroupSummary($"{g.Key}-ті", g.Count(), g.Sum(x => x.AvailableCopies));

        return query.ToList();
    }

// Додаткове 4: Бенчмарк на 100 000 записах із ToList() і без нього
    public static (long WithListMs, long DirectMs) BenchmarkToList(int count = 100_000)
    {
        // 1. Генерація 100 000 об'єктів книг у пам'яті
        var dataset = Enumerable.Range(1, count)
            .Select(i => Book.Create($"B-{i}", $"978-{i:D10}", $"Book {i}", 1900 + (i % 125), "Author", (i % 10) + 1))
            .ToList();

        // Прогрів JIT-компілятора перед вимірюванням
        _ = dataset.Where(b => b.Year > 1950).Select(b => b.Year).Take(10).ToList();

        // 2. Варіант ІЗ проміжним ToList() у ланцюжку (створює проміжний буферний масив)
        var swWith = System.Diagnostics.Stopwatch.StartNew();
        var avgWith = dataset
            .Where(b => b.Year > 1950)
            .ToList() // Проміжна матеріалізація в купі (Heap)
            .Where(b => b.AvailableCopies > 5)
            .Select(b => b.Year)
            .Average();
        swWith.Stop();

        // 3. Варіант БЕЗ ToList() (чистий потоковий конвеєр ітераторів IEnumerable)
        var swDirect = System.Diagnostics.Stopwatch.StartNew();
        var avgDirect = dataset
            .Where(b => b.Year > 1950)
            .Where(b => b.AvailableCopies > 5)
            .Select(b => b.Year)
            .Average();
        swDirect.Stop();

        return (swWith.ElapsedMilliseconds, swDirect.ElapsedMilliseconds);
    }
}