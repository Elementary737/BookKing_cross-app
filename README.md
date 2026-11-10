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
│   ├── Infrastructure/
│   │   ├── Infrastructure.csproj   # EF Core, Microsoft.EntityFrameworkCore.Sqlite
│   │   │
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs     # Контекст бази даних EF Core, Fluent API (BookRow, CopyRow)
│   │   │   └── EfCatalogStore.cs   # Реалізація IBookStore через EF Core
│   │   │
│   │   └── Migrations/             # Згенеровані EF Core міграції схеми БД
│   │       ├── 20261110_InitialCreate.cs
│   │       └── AppDbContextModelSnapshot.cs
│   │
│   ├── Api/
│   │   ├── Api.csproj              # Веб-проєкт (Minimal API)
│   │   ├── Program.cs              # Конфігурація SQLite ConnectionString, реєстрація EfCatalogStore, автоміграції
│   │   ├── Api.http                # Шаблони HTTP-запитів для тестування ендпоінтів
│   │   ├── Contracts/              # DTO та моделі запитів (BookDtos, PagedResult)
│   │   ├── Endpoints/              # Маршрутизація та ендпоінти Minimal API (BookEndpoints)
│   │   ├── Validation/             # Фільтри валідації запитів (ValidationFilter)
│   │   ├── Infrastructure/         # Глобальні обробники помилок (DomainExceptionHandler -> 409 Conflict)
│   │   └── data/
│   │       └── app.db              # Локальна база даних SQLite (ігнорується в Git)
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

🛠️ 1. Попередні вимоги (Prerequisites)

Перед початком роботи переконайся, що в системі встановлено необхідний інструментарій:

.NET 8.0 SDK (або новіша версія):

dotnet --version


Global tool dotnet-ef (для керування міграціями EF Core):

dotnet tool install --global dotnet-ef


CLI-утиліта sqlite3 (для прямих запитів до бази даних):

winget install SQLite.SQLite


🏗️ 2. Збірка та запуск проєкту (Build & Run)

Крок 1. Збірка всього рішення

У кореневій папці проєкту BookKing виконати:

dotnet build


Крок 2. Запуск Web API

dotnet run --project src/Api


Після успішного запуску в консолі з'явиться повідомлення про те, що сервер слухає порт (за замовчуванням http://localhost:5080):

info: Microsoft.Hosting.Lifetime[14]

Now listening on: http://localhost:5080

Зверни увагу: При першому запуску застосунок автоматично створить локальний файл бази даних SQLite за шляхом src/Api/data/app.db та застосує всі наявні міграції.

🗄️ 3. Керування міграціями (EF Core + SQLite)

Міграції фіксують зміни доменної моделі в C# та оновлюють схему бази даних SQLite.

Створення нової міграції (виконується після зміни entity-класів або AppDbContext):

dotnet ef migrations add <НазваМіграції> --project src/Infrastructure --startup-project src/Api


Застосування міграцій до бази даних вручну:

dotnet ef database update --project src/Infrastructure --startup-project src/Api


Видалення останньої незастосованої міграції:

dotnet ef migrations remove --project src/Infrastructure --startup-project src/Api


🧪 4. Тестування API ендпоінтів

Для тестування Minimal API можна використовувати файл src/Api/Api.http або інструмент curl у терміналі.

1. Додавання книги (POST):

curl -i -X POST "http://localhost:5080/api/books" -H "Content-Type: application/json" -d "{\"isbn\":\"978-0131103627\",\"title\":\"The C Programming Language\",\"year\":1988,\"author\":\"Brian W. Kernighan\",\"copies\":3}"


У разі успіху сервер поверне відповідь HTTP/1.1 201 Created із згенерованим id.

2. Отримання списку книг (GET):

curl -i "http://localhost:5080/api/books"


3. Отримання книги за ID (GET):

curl -i "http://localhost:5080/api/books/<id_книги>"


4. Оновлення книги (PUT):

curl -i -X PUT "http://localhost:5080/api/books/<id_книги>" -H "Content-Type: application/json" -d "{\"isbn\":\"978-0131103627\",\"title\":\"The C Programming Language (2nd Edition)\",\"year\":1988,\"author\":\"Brian W. Kernighan\",\"copies\":5}"


5. Видалення книги (DELETE):

curl -i -X DELETE "http://localhost:5080/api/books/<id_книги>"


🔍 5. Перевірка персистентності у базі даних SQLite

Для підтвердження збереження даних у локальній БД src/Api/data/app.db після перезапуску сервера використовуються CLI-команди sqlite3:

Перегляд усіх записів у таблиці Books:

sqlite3 -header -column src\Api\data\app.db "SELECT * FROM Books;"


Перегляд історії застосованих міграцій:

sqlite3 -header -column src\Api\data\app.db "SELECT * FROM __EFMigrationsHistory;"


Інтерактивний режим SQLite (CLI):

Вхід в інтерактивний режим:

sqlite3 src\Api\data\app.db


Корисні системні команди (починаються з крапки .)

.tables — відобразити всі таблиці в базі даних.

.schema Books — показати структуру таблиці Books (колонки та типи).

.headers on — увімкнути вивід заголовків колонок.

.mode column — увімкнути гарне форматування виводу у вигляді таблиці.

.exit — вийти з інтерактивного режиму.

Приклади SQL-запитів в інтерактивному режимі:

Важливо: Усі SQL-запити обов'язково мають закінчуватися крапкою з комою ;!

Отримати всі книги:

SELECT * FROM Books;


Додати нову книгу вручну:

INSERT INTO Books (Id, Isbn, Title, Author, Year, AvailableCopies) 
VALUES ('m1', '978-0000000000', 'Тестова книга', 'Автор', 2026, 5);


Оновити кількість примірників:

UPDATE Books SET AvailableCopies = 10 WHERE Id = 'm1';


Видалити книгу:

DELETE FROM Books WHERE Id = 'm1';


⚠️ Як вийти, якщо консоль зависла в стані ...> або ;->:

Якщо ви забули поставити ; в кінці SQL-запиту, консоль буде чекати продовження. Натисніть Ctrl + C, щоб скасувати поточне введення, після чого введіть .exit і натисніть Enter.