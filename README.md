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
│   │   ├── DomainException.cs      # Доменні винятки валідації даних та бізнес-правил
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
│   ├── Api/
│   │   ├── Api.csproj              # Веб-проєкт (Minimal API)
│   │   ├── Program.cs              # Конфігурація конвеєра, OpenAPI, ExceptionHandler, FluentValidation
│   │   ├── Contracts/              # DTO та моделі запитів (BookDtos, PagedResult)
│   │   ├── Endpoints/              # Маршрутизація та ендпоінти (BookEndpoints)
│   │   ├── Validation/             # Фільтри валідації запитів (ValidationFilter)
│   │   └── Infrastructure/         # Глобальні обробники помилок (DomainExceptionHandler -> 409 Conflict)
│   │
│   └── Cli/
│       ├── Cli.csproj              # UserSecretsId, копіювання appsettings*.json
│       ├── appsettings.json        # Базова конфігурація (сховище, ліміти, логування)
│       ├── appsettings.Development.json # Перевизначення конфігурації для розробки
│       ├── Sinks.cs
│       └── Program.cs              # Багаторівнева конфігурація, User Secrets, CWD-імунітет
│
└── tests/
    ├── Core.Tests/
    │   ├── Core.Tests.csproj       # Юніт-тести потокового імпорту, таймауту, CancellationToken та прогресу
    │   └── ImportTests.cs
    │
    └── Api.Tests/
        ├── Api.Tests.csproj        # Інтеграційні тести API (WebApplicationFactory, Mvc.Testing)
        └── BookApiIntegrationTests.cs
```

:: ЧАСТИНА 1: ОСНОВНІ ЗАВДАННЯ

:: 1. Збірка всього рішення
dotnet build

:: 2. Запуск вебсерверу API
dotnet run --project src/Api --urls http://localhost:5080

:: 3. Отримання повного списку всіх книг (Основний GET)
curl -i "http://localhost:5080/api/books"

:: 4. Створення нової книги в каталозі (Основний POST)
curl -i -X POST http://localhost:5080/api/books -H "Content-Type: application/json" -d "{\"isbn\":\"978-617-7866-10-6\",\"title\":\"Кобзар\",\"author\":\"Тарас Шевченко\",\"year\":1840,\"copies\":5}"

:: 5. Отримання детальної інформації про книгу за її ID (Основний GET за ідентифікатором)
:: (Замініть {id} на реальний ID, отриманий при створенні)
curl -i "http://localhost:5080/api/books/{id}"

:: 6. Оновлення існуючої книги (Основний PUT)
curl -i -X PUT http://localhost:5080/api/books/{id} -H "Content-Type: application/json" -d "{\"isbn\":\"978-617-7866-10-6\",\"title\":\"Кобзар (Оновлене видання)\",\"author\":\"Тарас Шевченко\",\"year\":1840,\"copies\":10}"

:: 7. Видалення книги за ID (Основний DELETE)
curl -i -X DELETE http://localhost:5080/api/books/{id}

:: ЧАСТИНА 2: ДОДАТКОВІ ЗАВДАННЯ

:: Додаткове 1: Пагінація та фільтрація списку книг за сторінками та автором
curl -i "http://localhost:5080/api/books?page=1&pageSize=5&author=Шевченко"

:: Додаткове 2: Перевірка валідації (FluentValidation + EndpointFilter — відправляємо пусту назву та невірні роки/копії, очікується 400 Bad Request)
curl -X POST http://localhost:5080/api/books -H "Content-Type: application/json" -d "{\"isbn\":\"978-617-7866-10-6\",\"title\":\"\",\"author\":\"Тарас Шевченко\",\"year\":1100,\"copies\":-1}" | python -m json.tool

:: Додаткове 3: Перевірка доменних винятків (DomainExceptionHandler -> очікується 409 Conflict)
:: Повторна спроба додати книгу з тим самим ISBN, яка вже є в базі:
curl -i -X POST http://localhost:5080/api/books -H "Content-Type: application/json" -d "{\"isbn\":\"978-617-7866-10-6\",\"title\":\"Кобзар\",\"author\":\"Тарас Шевченко\",\"year\":1840,\"copies\":5}"

:: Додаткове 4: Запуск усіх тестів рішення (Юніт-тести Core.Tests + Інтеграційні тести Api.Tests)
dotnet test
