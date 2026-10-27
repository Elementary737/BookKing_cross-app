using Core;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    // ТЕСТ 6: Скасування операції через переданий CancellationToken
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new LendingService(new InMemoryBookStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    // ТЕСТ 7: Некоректний файл з викиданням DomainException та видаленням у finally
    [Fact]
    public async Task ImportAsync_WithInvalidData_ThrowsDomainException_AndDeletesFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        // Невалідний запис: порожній ISBN викличе помилку валідації сутності
        await File.WriteAllTextAsync(tempFile, "[{\"Id\": \"1\", \"Isbn\": \"\", \"Title\": \"Тест\", \"Year\": 2020}]");

        try
        {
            var service = new LendingService(new InMemoryBookStore());
            await Assert.ThrowsAsync<DomainException>(() =>
                service.ImportAsync(new JsonCatalogImporter(), tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
// Додаткове 8.2
    [Fact]
    public async Task ImportAsync_WhenTimeoutOccurs_FiresTimeoutTokenSpecifically()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
            [
              {"id": "B-001", "isbn": "978-01", "title": "Book 1", "year": 2020, "author": "Author 1", "availableCopies": 2},
              {"id": "B-002", "isbn": "978-02", "title": "Book 2", "year": 2021, "author": "Author 2", "availableCopies": 5}
            ]
            """;
            await File.WriteAllTextAsync(tempFile, json);

            // Токен користувача (не скасовуємо вручну)
            using var userCts = new CancellationTokenSource();
            
            // Токен таймауту на дуже короткий час (50 мс, тоді як кожен запис чекає 200 мс)
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            
            // Зв'язане джерело скасування
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

            var service = new LendingService(new InMemoryBookStore());
            var importer = new JsonCatalogImporter();

            // Перевіряємо, що імпорт переривається винятком скасування
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await service.ImportAsync(importer, tempFile, linkedCts.Token);
            });

            // Доказ: спрацював саме таймаут, а токен користувача залишився нескасованим
            Assert.True(timeoutCts.IsCancellationRequested);
            Assert.False(userCts.IsCancellationRequested);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
// Додаткове 8.3
    [Fact]
    public async Task ImportAsync_ReportsPercentageProgress_ForAnyFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
            [
              {"id": "B-001", "isbn": "978-01", "title": "Book 1", "year": 2020, "author": "Author 1", "availableCopies": 2},
              {"id": "B-002", "isbn": "978-02", "title": "Book 2", "year": 2021, "author": "Author 2", "availableCopies": 5}
            ]
            """;
            await File.WriteAllTextAsync(tempFile, json);

            var service = new LendingService(new InMemoryBookStore());
            var importer = new JsonCatalogImporter();
            var percentages = new List<double>();

            var progress = new Progress<double>(p => percentages.Add(p));

            await service.ImportAsync(importer, tempFile, CancellationToken.None, progress);
            await Task.Delay(50);

            Assert.NotEmpty(percentages);
            Assert.True(percentages.Last() >= 99.0);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}