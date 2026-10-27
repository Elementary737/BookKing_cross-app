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

```text
BookKing/
├── BookKing.sln
├── README.md
├── .gitignore
│
├── data/
│   ├── sample.csv              # Книги у форматі CSV (Лабораторна 3)
│   ├── sample.json             # Книги у форматі JSON для потокового імпорту (Лабораторна 8)
│   ├── mixed.csv               # Змішані дані B; та R; (Лабораторна 3)
│   └── books.json              # Збереження каталогу на диск (Лабораторна 5)
│
├── logs/
│   └── sink_notifications.log  # Журнал сповіщень FileSink (Лабораторна 6)
│
├── src/
│   ├── Core/
│   │   ├── Core.csproj
│   │   ├── DomainException.cs      # Доменні винятки валідації даних (Лабораторна 8)
│   │   ├── EnvironmentInfo.cs
│   │   ├── SampleData.cs           # Початковий набір зразкових книг
│   │   ├── INotificationSink.cs    # Контракт каналу сповіщень (Лабораторна 6)
│   │   │
│   │   ├── Dto/
│   │   │   ├── BookDto.cs          # DTO десеріалізації для потокового читання (Лабораторна 8)
│   │   │   ├── ReaderDto.cs
│   │   │   └── ImportResult.cs
│   │   │
│   │   ├── Domain/
│   │   │   ├── Book.cs             # Фабрика Book.FromDto та бізнес-інваріанти
│   │   │   ├── Loan.cs
│   │   │   └── LoanStatus.cs
│   │   │
│   │   ├── Events/
│   │   │   └── LibraryChanged.cs   # Доменні події додавання книг (Лабораторна 6)
│   │   │
│   │   ├── Reports/                # Аналітичні звіти та DTO (Лабораторна 7)
│   │   │   ├── StockLine.cs
│   │   │   ├── GroupSummary.cs
│   │   │   ├── BookLoanSummary.cs
│   │   │   ├── LinqExtensions.cs
│   │   │   └── ReportService.cs
│   │   │
│   │   ├── Abstractions/
│   │   │   ├── IBookStore.cs
│   │   │   └── ICatalogImporter.cs # Контракт потокового читання IAsyncEnumerable (Лабораторна 8)
│   │   │
│   │   ├── Storage/
│   │   │   ├── InMemoryBookStore.cs
│   │   │   ├── FileBookStore.cs
│   │   │   ├── CachingBookStore.cs
│   │   │   ├── StoreFactory.cs
│   │   │   └── JsonCatalogImporter.cs # Потокове читання System.Text.Json + IProgress (Лабораторна 8)
│   │   │
│   │   ├── Services/
│   │   │   └── LendingService.cs   # Метод ImportAsync з підтримкою CancellationToken (Лабораторна 8)
│   │   │
│   │   └── Import/
│   │       ├── BookCsvImporter.cs
│   │       ├── BookJsonImporter.cs
│   │       └── MixedCsvImporter.cs
│   │
│   └── Cli/
│       ├── Cli.csproj
│       ├── Sinks.cs
│       └── Program.cs              # LinkedTokenSource, таймаути, IProgress-бар (Лабораторна 8)
│
└── tests/
    └── Core.Tests/
        ├── Core.Tests.csproj
        └── ImportTests.cs          # Юніт-тести потокового імпорту, таймауту, CancellationToken та прогресу
```

# 1. Збірка
dotnet build

# 2. Стандартний запуск (sample.json, таймаут 30с, прогрес-бар)
dotnet run --project src/Cli

# 3. Запуск із вказанням файлу
dotnet run --project src/Cli -- "data/sample.json"

# 4. Демонстрація таймауту (обмеження 1с)
dotnet run --project src/Cli -- --timeout 1s

# 5. Тести (11/11 успішних)
dotnet test

# 6. Збір покриття коду
dotnet test --collect:"XPlat Code Coverage"

Для більш зрозумілого виводу
# 7. Встановлення генератора звітів (одноразово)
dotnet new tool-manifest --force
dotnet tool install dotnet-reportgenerator-globaltool

# 8. Генерація та перегляд звіту (19.8% Core)
dotnet tool run reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary
type coveragereport\Summary.txt