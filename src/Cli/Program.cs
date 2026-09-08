using System.Runtime.InteropServices;
using System.Text.Json;

if (args.Contains("--json"))
{
    var information = new
    {
        OSDescription = RuntimeInformation.OSDescription,
        Environment = Environment.OSVersion.ToString(),
        Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
        DotNetVersion = Environment.Version.ToString(),
        Runtime = RuntimeInformation.FrameworkDescription,
        ApplicationDirectory = AppContext.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Бібліотека"
    };

    Console.WriteLine(JsonSerializer.Serialize(information));
}
else
{
	Console.WriteLine("BookKing - практикум з крос-платформного програмування");
	Console.WriteLine("Студентка: Гулай Ірина Костянтинівна, ФЕІ-34");
	Console.WriteLine(new string('-',52));

	Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}"); 
	Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}"); 
	Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}"); 
	Console.WriteLine($"Версія .NET (CLR) : {Environment.Version}"); 
	Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}"); 
	Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}"); 
	Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}"); 

	Console.WriteLine(new string('-', 52));
	Console.WriteLine("Предметна область: Бібліотека (книги, примірники, читачі, видачі)");
}