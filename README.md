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

## Запуск у Docker

Система підтримує запуск API-сервісу в ізольованому Linux-контейнері за допомогою Docker Compose із збереженням бази даних SQLite у зовнішньому томі (`./data`).

### Основні команди керування
* **Збирання образу та запуск сервісу:** `docker compose up --build` (для запуску у фоновому режимі додайте прапорець `-d`: `docker compose up --build -d`)
* **Перегляд стану та логів контейнера:** `docker compose ps` та `docker compose logs -f api`
* **Зупинка контейнера (локальні дані в `./data/app.db` зберігаються):** `docker compose down`
* **Повна зупинка з видаленням named volumes:** `docker compose down -v`

### Доступ з хоста та налаштування клієнта
* **Адреса API на хості:** `http://localhost:8080`
* **Перевірка стану (Healthcheck):** `http://localhost:8080/health` (повертає статус `200 OK` та `Healthy`)
* **Ендпоінти каталогу книг:** `http://localhost:8080/api/v1/books`
* **Конфлікт портів:** якщо порт `8080` на хостовій машині вже зайнятий іншим застосунком, змініть ліву частину зіставлення портів у файлі `docker-compose.yml` (наприклад, на `"8081:8080"`).
* **Підключення Blazor Web-клієнта:** щоб вебзастосунок (`src/Web`) звертався до контейнеризованого API, у файлі `src/Web/appsettings.json` потрібно вказати базову адресу `http://localhost:8080` у параметрі `ApiBaseUrl`.

### Міграції бази даних
Застосування міграцій Entity Framework Core відбувається автоматично при старті контейнера через виклик `Database.Migrate()` у `src/Api/Program.cs`. Файл бази даних створюється на хості за шляхом `./data/app.db`.

### Перевірка кросплатформності та збірки без кешу
* У файлах `Dockerfile` та `docker-compose.yml` використовуються виключно прямі слеші (`/`) та відносні шляхи (`./data:/app/data`) без прив'язки до Windows.
* Для коректної роботи скриптів у Linux-середовищі у файлі `.editorconfig` задано закінчення рядків `end_of_line = lf` (зокрема для `[*.sh]`).
* Перевірка незалежного збирання образу без використання локального кешу виконується командами `docker builder prune -f`, `docker compose build --no-cache` та `docker compose up -d`.