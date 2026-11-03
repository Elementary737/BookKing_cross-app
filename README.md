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
│   │   ├── Configuration/          # Моделі параметрів конфігурації
│   │   │   └── AppOptions.cs       # AppOptions, StorageOptions (ResolveFullPath)
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
│   │   │   ├── FileBookStore.cs    # Атомарний запис (.tmp), кирилиця, camelCase
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
│       ├── Cli.csproj              # UserSecretsId, копіювання appsettings*.json
│       ├── appsettings.json        # Базова конфігурація (сховище, ліміти, логування)
│       ├── appsettings.Development.json # Перевизначення конфігурації для розробки
│       ├── Sinks.cs
│       └── Program.cs              # Багаторівнева конфігурація, User Secrets, CWD-імунітет
│
└── tests/
    └── Core.Tests/
        ├── Core.Tests.csproj
        └── ImportTests.cs          # Юніт-тести потокового імпорту, таймауту, CancellationToken та прогресу
```

# 1. Налаштування та перевірка секретів (User Secrets)
dotnet user-secrets set "Integration:ApiKey" "local-dev-key" --project src/Cli
dotnet user-secrets list --project src/Cli

# 2. Перевірка відсутності захардкодscope-шляхів
git grep "C:" src/
git grep "/home/" src/
git grep -F "\\" src/

# 3. Збірка та тестування
dotnet build
dotnet test

# 4. Демонстрація переносимості (CWD-імунітет)
dotnet run --project src/Cli
dotnet run --project src/Cli --property:RunWorkingDirectory=src/Cli

# 5. Перевизначення шляху через змінну середовища (CMD)
set "CROSSAPP_DATA_PATH=C:\temp\custom_data"
dotnet run --project src/Cli
set "CROSSAPP_DATA_PATH="

# 5.1. Перевизначення шляху через змінну середовища (PowerShell)
$env:CROSSAPP_DATA_PATH = "C:\temp\custom_data"
dotnet run --project src/Cli
Remove-Item Env:\CROSSAPP_DATA_PATH

# 6. Перевірка статусу Git (чистий репозиторій без secrets.json)
git status