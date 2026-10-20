namespace Core.Reports;

public sealed record OverdueIssueLine(string IssueId, string BookTitle, string ReaderName, int OverdueDays);