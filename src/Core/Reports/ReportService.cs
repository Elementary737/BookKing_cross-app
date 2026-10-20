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
}