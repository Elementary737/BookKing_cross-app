using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Book> Books() =>
    [
	Book.Create("id-3", "ISBN-LOW", "Місто", 1928, "В. Підмогильний", 1),
        Book.Create("id-1", "ISBN-BIG", "Кобзар", 1840, "Т. Шевченко", 10),
        Book.Create("id-2", "ISBN-MID", "Тіні забутих предків", 1911, "М. Коцюбинський", 5)
    ];
}