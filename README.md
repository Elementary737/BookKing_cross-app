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

## 1. Попередні вимоги (Prerequisites)

- **.NET 10.0 SDK** (або новіша версія):
  ```bash
  dotnet --version
  ```

---

## 2. Збірка та запуск застосунку (Api + Blazor Web)

Для повноцінної роботи веб-інтерфейсу потрібно одночасно запустити два процеси в окремих терміналах із кореневої папки `BookKing`.

### Крок 1. Збірка всього рішення
```bash
dotnet build
```

### Крок 2. Запуск бекенду (`src/Api`)
У першому терміналі виконайте:
```bash
dotnet run --project src/Api --urls http://localhost:5080
```
Сервер запуститься за адресою `http://localhost:5080` та автоматично ініціалізує локальну базу даних SQLite (`src/Api/data/app.db`).

### Крок 3. Запуск клієнта Blazor (`src/Web`)
У другому терміналі виконайте:
```bash
dotnet run --project src/Web --urls http://localhost:5090
```
Після запуску відкрийте у браузері сторінку каталогу книг:
`http://localhost:5090/books`

---

## 3. Функціонал веб-інтерфейсу (Лабораторна 12)

### Основні можливості
- **Перегляд каталогу (`/books`):** завантаження та відображення таблиці книг з `Api` (`ISBN`, `Назва`, `Автор`, `Рік`, `Примірники`) з індикатором завантаження.
- **Створення та редагування (`/books/new`, `/books/{id}`):** форма `EditForm` з двома рівнями перевірки:
  - *Клієнтська валідація* (`DataAnnotationsValidator` + `BookModel`) — миттєво перевіряє обов'язкові поля, діапазон року (`1000..2100`) та кількість примірників (`0..1000`).
  - *Серверна валідація* (`ValidationProblem` `400 Bad Request`) — перевіряє унікальність `ISBN` у базі даних та правила домену на стороні `Api`, виводить повідомлення під відповідним полем і автоматично очищує його при редагуванні поля.
- **Стійкість до недоступності сервера:** при зупинці процесу `Api` інтерфейс перехоплює `HttpRequestException` та відображає зрозуміле попередження користувачу без падіння застосунку.

### Реалізовані додаткові завдання
1. **Видалення книги з підтвердженням:** двокрокове підтвердження в таблиці (`Видалити` → `Підтвердити` / `Скасувати`) з викликом `DELETE /api/books/{id}`.
2. **Розширений пошук та сортування:** фільтрація за назвою/ISBN, окремий фільтр за автором (`author`) та вибір сортування (`За ISBN`, `За назвою`, `За автором`, `За кількістю примірників`).
3. **Спільна бібліотека контрактів (`src/Contracts`):** винесення DTO, моделей форм та контрактів помилок в окремий проєкт без дублювання коду між `Api` та `Web`.
4. **Підтримка скасування запитів (`CancellationToken`):** скасування попередніх HTTP-запитів при швидкому повторному пошуку або виході зі сторінки через `CancellationTokenSource` та реалізацію `IDisposable`.