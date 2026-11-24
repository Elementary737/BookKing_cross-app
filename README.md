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
├── Directory.Build.props           # Спільні налаштування компіляції (LangVersion, Nullable, Analyzers, TreatWarningsAsErrors)
├── .editorconfig                   # Правила стилю коду та форматування
├── README.md
├── .gitignore
│
├── src/
│   ├── Contracts/                  # Спільна бібліотека контрактів і моделей
│   │   ├── Contracts.csproj
│   │   ├── BookModels.cs           # DTO (BookDto, SaveBookRequest, PagedResult, ValidationProblemResponse) та BookModel
│   │   └── BookMappingExtensions.cs # Мапінг між сутностями Core та DTO
│   │
│   ├── Core/
│   │   ├── Core.csproj
│   │   ├── DomainException.cs      # Доменні винятки валідації даних та бізнес-правил
│   │   ├── EnvironmentInfo.cs
│   │   ├── Abstractions/           # Інтерфейси IBookStore, ICatalogImporter
│   │   ├── Configuration/          # Моделі параметрів конфігурації
│   │   ├── Domain/                 # Доменні сутності (Book, BookCopy, Loan, LoanStatus) та інваріанти
│   │   ├── Dto/                    # DTO десеріалізації для потокового читання
│   │   ├── Events/                 # Доменні події додавання книг (LibraryChanged)
│   │   ├── Import/                 # CSV та JSON імпортери
│   │   ├── Reports/                # Аналітичні звіти та LINQ-розширення
│   │   ├── Services/               # LendingService з підтримкою CancellationToken
│   │   └── Storage/                # InMemory, File, Caching сховища та JsonCatalogImporter
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
│   │   ├── Api.http                # Шаблони HTTP-запитів для тестування ендпоінтів /api/v1/books
│   │   ├── Endpoints/
│   │   │   └── BookEndpoints.cs    # Маршрутизація CRUD (/api/v1/books), пошук, фільтрація, сортування
│   │   ├── Infrastructure/
│   │   │   └── DomainExceptionHandler.cs # Глобальний обробник DomainException у форматі ProblemDetails
│   │   └── Validation/
│   │       └── ValidationFilter.cs # Фільтр валідації запитів із логуванням 400 Bad Request
│   │
│   ├── Web/                        # Веб-інтерфейс на Blazor Interactive Server
│   │   ├── Web.csproj
│   │   ├── Program.cs              # Реєстрація типізованого клієнта AddHttpClient<BooksApiClient>
│   │   ├── appsettings.json        # Конфігурація ApiBaseUrl ("http://localhost:5080")
│   │   ├── Services/
│   │   │   └── BooksApiClient.cs   # Типізований HTTP-клієнт до /api/v1/books (CRUD, CancellationToken, 400/404)
│   │   └── Components/
│   │       ├── Layout/
│   │       │   └── NavMenu.razor   # Навігаційне меню (Home, Каталог книг)
│   │       └── Pages/
│   │           ├── Books.razor     # Список книг, пошук, фільтр за автором, сортування, видалення
│   │           └── BookEdit.razor  # Форма створення та редагування книги (клієнтська + серверна валідація)
│   │
│   └── Cli/
│       ├── Cli.csproj              # Консольний клієнт
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
└── tests/
    ├── Core.Tests/
    │   ├── Core.Tests.csproj       # Юніт-тести домену, імпорту та архітектурної ізоляції
    │   ├── ImportTests.cs
    │   └── ArchitectureGuardTests.cs # Тест-вартовий ізоляції збірки Core від EF Core та Infrastructure
    │
    └── Api.Tests/
        ├── Api.Tests.csproj        # Інтеграційні тести API (WebApplicationFactory + тимчасова БД у %TEMP%)
        └── BookApiIntegrationTests.cs
```

---

## Команди для збірки, перевірки та запуску

### Перевірка якості коду, форматування та тестування

```bash
# Відновити пакети NuGet
dotnet restore

# Перевірити форматування коду згідно з .editorconfig
dotnet format --verify-no-changes

# Зібрати рішення в режимі суворої перевірки (попередження як помилки)
dotnet build -warnaserror

# Запустити всі модульні, архітектурні та інтеграційні тести
dotnet test
```

### Запуск застосунків

```bash
# 1. Запуск Minimal API (http://localhost:5080, OpenAPI/Swagger: /openapi/v1.json)
dotnet run --project src/Api

# 2. Запуск Blazor Web-інтерфейсу (у другому терміналі, після запуску Api)
dotnet run --project src/Web

# 3. Запуск консольного клієнта CLI
dotnet run --project src/Cli
```

---

## Архітектура

Залежності спрямовані всередину до доменної моделі:
- `Core` — доменні сутності (`Book`, `BookCopy`, `Loan`, `LoanStatus`), бізнес-інваріанти (`LendingService`), винятки (`DomainException`) та інтерфейси (`IBookStore`, `ICatalogImporter`). Не залежить від інших проєктів та інфраструктурних пакетів.
- `Contracts` — спільні DTO та контракти (`BookDto`, `SaveBookRequest`, `PagedResult`, `ValidationProblemResponse`) для взаємодії між `Api` та `Web`.
- `Infrastructure` -> `Core` — реалізація сховищ на базі EF Core та SQLite (`AppDbContext`, `EfCatalogStore`, міграції).
- `Api` -> `Core`, `Contracts`, `Infrastructure` — Minimal API (`BookEndpoints`), валідація (`ValidationFilter`), обробка помилок (`DomainExceptionHandler`, `ProblemDetails`), конфігурація DI.
- `Cli` -> `Core` — консольний клієнт для імпорту, звітів та роботи зі сховищами.
- `Web` -> `Contracts` — Blazor-клієнт (`BooksApiClient`), що взаємодіє з `Api` лише через HTTP-контракт.
- `Core.Tests` -> `Core` — модульні тести домену та архітектурний тест ізоляції (`ArchitectureGuardTests`).
- `Api.Tests` -> `Api`, `Contracts` — інтеграційні тести ендпоінтів API.

---

## Версіонування API та політика сумісності

Усі HTTP-маршрути каталогу книг використовують префікс версії у шляху: `/api/v1/books`.

### Політика підтримки версії `v1` після змін контракту
- **Зворотньо сумісні зміни (non-breaking):** додавання нових необов'язкових полів у відповіді або нових необов'язкових query-параметрів вносяться безпосередньо у `v1` без зміни версії маршруту.
- **Несумісні зміни контракту (breaking changes):** перейменування чи видалення полів DTO, зміна типів даних, зміна обов'язкових параметрів або семантики кодів відповідей потребують створення нової версії маршрутів (`/api/v2/books`).
- **Життєвий цикл `v1` після запуску `v2`:** маршрути `/api/v1/...` залишаються активними протягом перехідного періоду (deprecation window) із передачею заголовка `Deprecation: true` та посилання на актуальну версію документації, доки всі клієнти (`Web` та зовнішні інтеграції) не перейдуть на `v2`.