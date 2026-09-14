&#x09;# BookKing

Наскрізний проєкт з крос-платформного програмування.



&#x09;## Предметна область

Бібліотека.



&#x09;## Сутності

\- Book — книга

\- BookCopy — примірник книги

\- Reader — читач

\- Loan — видача



&#x09;## Призначення

Застосунок призначений для обліку видачі примірників книг читачам та їх повернення.



&#x09;## Середовище

.NET 10.0, Windows 11 x64.



&#x09;## Структура solution

BookKing/

├── BookKing.sln

├── README.md

├── .gitignore

└── src/

&#x20;   ├── Core/

&#x20;   │   ├── Core.csproj

&#x20;   │   └── EnvironmentInfo.cs

&#x20;   └── Cli/

&#x20;       ├── Cli.csproj

&#x20;       └── Program.cs



&#x09;## Build

dotnet build



&#x09;## Run

dotnet run --project src/Cli



&#x09;## Запуск опублікованого Windows self-contained застосунку:



publish\\win-x64-self\\Cli.exe

&#x09;

&#x09;## Publish



&#x20;   Windows x64 — self-contained

dotnet publish src\\Cli -c Release -f net10.0 -r win-x64 --self-contained true -o publish\\win-x64-self



&#x20;   Windows x64 — framework-dependent

dotnet publish src\\Cli -c Release -f net10.0 -r win-x64 --self-contained false -o publish\\win-x64-fdd



&#x20;   Linux x64 — self-contained

dotnet publish src\\Cli -c Release -f net10.0 -r linux-x64 --self-contained true -o publish\\linux-x64-self



&#x20;   Linux x64 — framework-dependent

dotnet publish src\\Cli -c Release -f net10.0 -r linux-x64 --self-contained false -o publish\\linux-x64-fdd



&#x09;## Порівняння режимів публікації

| RID       | Режим                | Розмір publish | Потрібен встановлений runtime |

|-----------|----------------------|----------------|-------------------------------|

| win-x64   | self-contained       |  77 МБ         | ні                            |

| win-x64   | framework-dependent  | 0,2 МБ         | так (.NET 10)                 |

| linux-x64 | self-contained       |  79 МБ         | ні                            |

| linux-x64 | framework-dependent  | 0,1 МБ         | так (.NET 10)                 |

