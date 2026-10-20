namespace Core.Reports;

public static class LinqExtensions
{
    // Власний узагальнений метод-розширення для відбору N найбільших елементів за вказаним ключем
    public static IEnumerable<TSource> TopBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        if (count <= 0)
            return [];

        return source.OrderByDescending(keySelector).Take(count);
    }
}