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
}