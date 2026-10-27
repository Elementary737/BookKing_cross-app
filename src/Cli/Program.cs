using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Core.Events;
using Cli;
using Core.Reports;

Console.WriteLine("BookKing - практикум з крос-платформного програмування");
Console.WriteLine("Студентка: Гулай Ірина Костянтинівна, ФЕІ-34");
Console.WriteLine("Предметна область: Бібліотека (книги, примірники, читачі, видачі)");
Console.WriteLine(new string('-', 52));

var timeoutSeconds = 8;
var timeoutArgIndex = Array.IndexOf(args, "--timeout");
if (timeoutArgIndex != -1 && timeoutArgIndex + 1 < args.Length && int.TryParse(args[timeoutArgIndex + 1], out var parsedSeconds))
{
    timeoutSeconds = parsedSeconds;
}

var dataPath = args.Where((arg, index) => !arg.StartsWith("--") && (timeoutArgIndex == -1 || index != timeoutArgIndex + 1))
                   .FirstOrDefault() ?? "data/sample.json";

var timeoutDuration = TimeSpan.FromSeconds(timeoutSeconds);
using var userCts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    userCts.Cancel();
};

using var timeoutCts = new CancellationTokenSource(timeoutDuration);
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);


var store = new InMemoryBookStore(); 
var service = new LendingService(store);

// Додаткове 8.3
var progress = new Progress<double>(percentage =>
{
    var barLength = 20;
    var filled = (int)Math.Round(barLength * (percentage / 100.0));
    var bar = new string('#', filled) + new string('-', Math.Max(0, barLength - filled));

    Console.Write($"\r[Прогрес]: [{bar}] {percentage,5:F1}%");
});

try
{
    Console.WriteLine($"Початок імпорту з {dataPath}...");
    var added = await service.ImportAsync(new JsonCatalogImporter(), dataPath, linkedCts.Token, progress);
    Console.WriteLine($"\nІмпортовано записів: {added}");
}
catch (OperationCanceledException)
{
    if (timeoutCts.IsCancellationRequested)
    {
        Console.WriteLine($"\n[Таймаут]: Час очікування операції вичерпано (Timeout {timeoutDuration.TotalSeconds:F0}s).");
    }
    else
    {
        Console.WriteLine("\n[Скасовано]: Імпорт перервано користувачем (Ctrl+C).");
    }
}
catch (DomainException ex)
{
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"Файл не знайдено: {dataPath}");
}


return 0;