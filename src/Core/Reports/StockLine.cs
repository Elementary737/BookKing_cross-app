namespace Core.Reports;

public sealed record StockLine(string Isbn, string Title, string? Author, int Year, int AvailableCopies);