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

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var dataPath = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "data/sample.json";

var store = new InMemoryBookStore(); 
var service = new LendingService(store);

try
{
    var added = await service.ImportAsync(new JsonCatalogImporter(), dataPath, cts.Token);
    Console.WriteLine($"Імпортовано записів: {added}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Імпорт скасовано.");
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