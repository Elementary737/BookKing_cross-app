using Core;

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