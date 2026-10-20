namespace Core.Reports;

public sealed record BookLoanSummary(
    string BookTitle,
    string Isbn,
    int TotalLoansCount,
    int ActiveLoansCount
);