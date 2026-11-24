# BookKing

Наскрізний проєкт з крос-платформного програмування.

## Предметна область
Бібліотека.

## Сутності
- `Book` — книга
- `BookCopy` — примірник книги
- `Reader` — читач
- `Loan` — видача

## Призначення
Застосунок призначений для обліку видачі примірників книг читачам та їх повернення, а також керування каталогом книг через Web API та веб-інтерфейс Blazor.

## Середовище
.NET 10.0, Windows 11 x64.

## Структура проєкту

```text
BookKing/
├── BookKing.sln
├── README.md
├── .gitignore
│
├── data/
│   ├── sample.csv                  # Книги у форматі CSV (Лабораторна 3)
│   ├── sample.json                 # Книги у форматі JSON для потокового імпорту (Лабораторна 8)
│   ├── mixed.csv                   # Змішані дані B; та R; (Лабораторна 3)
│   └── books.json                  # Збереження каталогу на диск (Лабораторна 5)
│
├── logs/
│   └── sink_notifications.log      # Журнал сповіщень FileSink (Лабораторна 6)
│
├── src/
│   ├── Contracts/                  # Спільна бібліотека контрактів і моделей (Лабораторна 12)
│   │   ├── Contracts.csproj
│   │   ├── BookDto.cs              # DTO книги для обміну між Api та Web
│   │   ├── BookModel.cs            # Модель форми Blazor з атрибутами DataAnnotations
│   │   ├── PagedResult.cs          # Пагінований результат пошуку
│   │   ├── SaveBookRequest.cs      # Запит на створення/оновлення книги
│   │   └── ValidationProblemResponse.cs # Модель відповіді сервера 400 ValidationProblem
│   │
│   ├── Core/
│   │   ├── Core.csproj
│   │   ├── DomainException.cs      # Доменні винятки валідації даних та бізнес-правил
│   │   ├── EnvironmentInfo.cs
│   │   ├── SampleData.cs           # Початковий набір зразкових книг
│   │   ├── INotificationSink.cs    # Контракт каналу сповіщень (Лабораторна 6)
│   │   ├── Configuration/          # Моделі параметрів конфігурації
│   │   ├── Dto/                    # DTO десеріалізації для потокового читання
│   │   ├── Domain/                 # Доменні сутності (Book, Loan, LoanStatus) та інваріанти
│   │   ├── Events/                 # Доменні події додавання книг (LibraryChanged)
│   │   ├── Reports/                # Аналітичні звіти та LINQ-розширення
│   │   ├── Abstractions/           # Інтерфейси IBookStore, ICatalogImporter
│   │   ├── Storage/                # InMemory, File, Caching сховища та JsonCatalogImporter
│   │   ├── Services/               # LendingService з підтримкою CancellationToken
│   │   └── Import/                 # CSV та JSON імпортери
│   │
│   ├── Infrastructure/
│   │   ├── Infrastructure.csproj   # EF Core, Microsoft.EntityFrameworkCore.Sqlite
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs     # Контекст бази даних EF Core, Fluent API
│   │   │   └── EfCatalogStore.cs   # Реалізація IBookStore через EF Core
│   │   └── Migrations/             # Згенеровані EF Core міграції схеми БД
│   │
│   ├── Api/
│   │   ├── Api.csproj              # Веб-проєкт (Minimal API)
│   │   ├── Program.cs              # Конфігурація SQLite, реєстрація EfCatalogStore, автоміграції
│   │   ├── Api.http                # Шаблони HTTP-запитів для тестування ендпоінтів
│   │   ├── Endpoints/
│   │   │   └── BookEndpoints.cs    # Маршрутизація CRUD, пошук (search, author), сортування (sortBy)
│   │   ├── Validation/
│   │   │   └── ValidationFilter.cs # Фільтр валідації запитів із логуванням 400 Bad Request
│   │   └── data/
│   │       └── app.db              # Локальна база даних SQLite (ігнорується в Git)
│   │
│   ├── Web/                        # Веб-інтерфейс на Blazor Interactive Server (Лабораторна 12)
│   │   ├── Web.csproj
│   │   ├── Program.cs              # Реєстрація типізованого клієнта AddHttpClient<BooksApiClient>
│   │   ├── appsettings.json        # Конфігурація ApiBaseUrl ("http://localhost:5080")
│   │   ├── Services/
│   │   │   └── BooksApiClient.cs   # Типізований HTTP-клієнт (CRUD, CancellationToken, обробка 400/404)
│   │   └── Components/
│   │       ├── Layout/
│   │       │   └── NavMenu.razor   # Навігаційне меню (Home, Каталог книг)
│   │       └── Pages/
│   │           ├── Books.razor     # Список книг, пошук, фільтр за автором, сортування, видалення
│   │           └── BookEdit.razor  # Форма створення та редагування книги (клієнтська + серверна валідація)
│   │
│   └── Cli/
│       ├── Cli.csproj              # Консольний клієнт (Лабораторні 1–9)
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Sinks.cs
│       └── Program.cs
│
└── tests/
    ├── Core.Tests/
    │   ├── Core.Tests.csproj       # Юніт-тести потокового імпорту, таймауту, CancellationToken
    │   └── ImportTests.cs
    │
    └── Api.Tests/
        ├── Api.Tests.csproj        # Інтеграційні тести API (WebApplicationFactory)
        └── BookApiIntegrationTests.cs
```

---

## Архітектура

Залежності спрямовані всередину до доменної моделі:
- `Core` — доменні сутності (`Book`, `Loan`, `LoanStatus`), бізнес-інваріанти (`LendingService`), винятки (`DomainException`) та інтерфейси (`IBookStore`, `ICatalogImporter`). Не залежить від інших проєктів та інфраструктурних пакетів.
- `Contracts` — спільні DTO та контракти (`BookDto`, `SaveBookRequest`, `PagedResult`, `ValidationProblemResponse`) для взаємодії між `Api` та `Web`.
- `Infrastructure` -> `Core` — реалізація сховищ на базі EF Core та SQLite (`AppDbContext`, `EfCatalogStore`, міграції).
- `Api` -> `Core`, `Contracts`, `Infrastructure` — Minimal API (`BookEndpoints`), валідація (`ValidationFilter`), обробка помилок (`ProblemDetails`), конфігурація DI.
- `Cli` -> `Core` — консольний клієнт для імпорту, звітів та роботи зі сховищами.
- `Web` -> `Contracts` — Blazor-клієнт (`BooksApiClient`), що взаємодіє з `Api` лише через HTTP-контракт.
- `Core.Tests` -> `Core` — модульні тести домену та архітектурний тест ізоляції.
- `Api.Tests` -> `Api`, `Contracts` — інтеграційні тести ендпоінтів API.

