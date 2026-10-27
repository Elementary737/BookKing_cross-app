using Core.Reports;
using Core.Services;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    // ТЕСТ 4: Параметризований тест звітності ([Theory])
    [Theory]
    [InlineData(6, 2)] 
    [InlineData(2, 1)] 
    [InlineData(1, 0)] 
    public void VintageOrLowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        var result = TestData.Books().Where(b => b.AvailableCopies < threshold).ToList();
        Assert.Equal(expected, result.Count);
    }

    // ТЕСТ 5: Тест детермінованості топу (стабільне сортування за спаданням примірників)
    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        var top = TestData.Books()
            .OrderByDescending(b => b.AvailableCopies)
            .Take(2)
            .Select(b => b.Isbn);

        Assert.Equal(new[] { "ISBN-BIG", "ISBN-MID" }, top);
    }
}