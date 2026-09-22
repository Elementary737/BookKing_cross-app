using Core;
using Core.Dto;
using Core.Import;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("BookKing - практикум з крос-платформного програмування");
Console.WriteLine("Студентка: Гулай Ірина Костянтинівна, ФЕІ-34");
Console.WriteLine(new string('-',52));

Console.WriteLine($"ОС : {report.OsDescription}"); 
Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}"); 
Console.WriteLine($"Runtime : {report.FrameworkDescription}"); 
Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}"); 
Console.WriteLine($"RID (визначено): {report.DetectedRid}");
Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Бібліотека (книги, примірники, читачі, видачі)");

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<BookDto> result = BookCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (BookDto b in result.Items.Take(5))
{
    Console.WriteLine(
        $"  {b.Id,-6} {b.Isbn,-16} {b.Title,-32} {b.Year,4}  {b.Author ?? "-"}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
        Console.WriteLine($"  ! {error}");
}

return 0;
