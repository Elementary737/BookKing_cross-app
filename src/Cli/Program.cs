using System.Reflection;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

Console.WriteLine("BookKing - практикум з крос-платформного програмування");
Console.WriteLine("Студентка: Гулай Ірина Костянтинівна, ФЕІ-34");
Console.WriteLine("Предметна область: Бібліотека (книги, примірники, читачі, видачі)");
Console.WriteLine(new string('-', 52));

var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();

// Підтримка окремої змінної середовища CROSSAPP_DATA_PATH
var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storage = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy,
    FileName = options.Storage.FileName
};

// Отримуємо готовий абсолютний шлях до файлу (замість старого захардкодженого)
var dataPath = storage.ResolveFullPath(AppContext.BaseDirectory);

// Робота з секретом
var apiKey = config["Integration:ApiKey"];
Console.WriteLine($"Файл даних: {dataPath}");
Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");
Console.WriteLine($"Термін позичання (LoanDays): {options.Library.LoanDays} дн.");