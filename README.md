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

\## Структура проєкту

BookKing/

├── BookKing.sln

├── README.md

├── .gitignore

├── data/

│   ├── sample.csv          # Книги у форматі CSV (валідні та пошкоджені записи)

│   ├── sample.json         # Книги у форматі JSON

│   └── mixed.csv           # Змішані дані (рядки B; для книг та R; для читачів)

└── src/

&#x20;   ├── Core/

&#x20;   │   ├── Core.csproj

&#x20;   │   ├── EnvironmentInfo.cs

&#x20;   │   ├── Dto/

&#x20;   │   │   ├── BookDto.cs

&#x20;   │   │   └── ReaderDto.cs

&#x20;   │   └── Import/

&#x20;   │       ├── ImportResult.cs

&#x20;   │       ├── BookCsvImporter.cs

&#x20;   │       ├── BookJsonImporter.cs

&#x20;   │       └── MixedCsvImporter.cs

&#x20;   └── Cli/

&#x20;       ├── Cli.csproj

&#x20;       └── Program.cs



&#x09;## Build

dotnet build



&#x09;## Збірка та запуск



&#x20;   Збірка розв'язку:

dotnet build



&#x20;   Запуск стандартного CSV-імпорту (за замовчуванням data/sample.csv):

dotnet run --project src\\Cli



&#x20;   Запуск з явним шляхом до CSV:

dotnet run --project src\\Cli -- data\\sample.csv



&#x20;   Запуск JSON-імпорту:

dotnet run --project src\\Cli -- data\\sample.json



&#x20;   Запуск обробки різнорідних рядків:

dotnet run --project src\\Cli -- --mixed data\\mixed.csv



&#x20;   Перевірка обробки непідтримуваного формату:

dotnet run --project src\\Cli -- data\\sample.txt

