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
├── Dockerfile                      # Multi-stage Docker-файл для збирання та запуску API
├── docker-compose.yml              # Оркестрація сервісів api та web, мережа та volume для SQLite
├── .dockerignore                   # Виключення bin/, obj/, .git/, data/ із контексту збірки Docker
├── README.md
├── .gitignore
│
├── data/                           # Каталог на хості, примонтований у контейнер для збереження БД SQLite
│   └── app.db                      # Файл бази даних SQLite (створюється та зберігається між перезапусками)
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
│   │   ├── Program.cs              # Конфігурація SQLite, реєстрація EfCatalogStore, автоміграції та /health
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
│   │   ├── Dockerfile              # Multi-stage Docker-файл для збирання та запуску Blazor-клієнта
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

## Демонстрація контейнеризації та розгортання (Docker)

Перед початком демонстрації переконайтеся, що Docker Desktop запущений, а попередньо створені контейнери зупинені, щоб уникнути конфліктів портів (`8080` та `5090`):

```bash
docker compose down
docker rm -f test-api test-multiarch 2>nul
```
## Захист та демонстрація роботи (Docker)

Перед початком демонстрації переконайтеся, що Docker Desktop запущений, а попередньо створені контейнери зупинені, щоб уникнути конфліктів портів (`8080` та `5090`):

```bash
docker compose down
docker rm -f test-multiarch 2>nul
```

---

### Основне завдання (згідно з планом захисту)

#### 1. Запуск контейнерів та перевірка їх стану
```bash
docker compose up -d --build
docker compose ps
```

#### 2. Перевірка `/health` та виконання CRUD-сценарію
- **Healthcheck (браузер або термінал):**
  ```bash
  curl http://localhost:8080/health
  ```
- **CRUD-сценарій через вебінтерфейс (`http://localhost:5090`) або API (`http://localhost:8080/api/v1/books`):**
  1. Відкрити `http://localhost:5090` у браузері та створити нову книгу (**Create**).
  2. Переглянути створений запис у каталозі або запитом `curl http://localhost:8080/api/v1/books` (**Read**).
  3. Відредагувати дані книги через інтерфейс (**Update**).
  4. За потреби видалити тестовий запис (**Delete**), залишивши хоча б одну книгу для перевірки персистентності на наступному кроці.

#### 3. Перевірка збереження бази даних на хості (`./data/app.db`)
1. Перевірити наявність файлу бази даних на хості:
   ```bash
   dir data
   ```
   *(У директорії `./data/` відображається файл `app.db`).*
2. Перезапустити контейнер API та повторно виконати `GET`-запит:
   ```bash
   docker compose restart api
   curl http://localhost:8080/api/v1/books
   ```
   *(Створені книги зберігаються після перезапуску контейнера завдяки монтуванню каталогу `./data:/app/data`).*

#### 4. Архітектура Multi-stage збірки (`Dockerfile`)
- **Етап 1 (`FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build`):** використовується повний .NET SDK для відновлення NuGet-пакетів (`dotnet restore`), компіляції проєкту та публікації релізної збірки (`dotnet publish`).
- **Етап 2 (`FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final`):** використовується легкий runtime-образ ASP.NET Core, у який копіюються лише скомпільовані `.dll`-файли з папки `/app/publish`.
- **Що не потрапило у фінальний образ:** компілятори та утиліти SDK, початковий код (`.cs`, `.csproj`, `.sln`), проміжні артефакти збірки (`obj/`, `bin/`) та локальний кеш NuGet-пакетів.

#### 5. Зміна порту та рядка підключення без перезбирання коду
Усі налаштування передаються через змінні середовища у файлі `docker-compose.yml` або під час запуску через `docker run -e ... -p ...`:
- **Зміна порту:** у секції `ports` файлу `docker-compose.yml` змінити зовнішній порт хоста (наприклад, `"8085:8080"` замість `"8080:8080"`) або передати внутрішній порт через змінну `ASPNETCORE_URLS=http://+:8085`.
- **Зміна рядка підключення до БД:** змінити значення змінної середовища `ConnectionStrings__DefaultConnection` (наприклад, `Data Source=/app/data/custom_books.db`) і виконати `docker compose up -d` без перезбирання образу (`--build` не потрібен).

---

### Додаткові завдання (по порядку від 1 до 4)

#### Додаткове 1: оптимізація образу та ізоляція контексту збірки (`.dockerignore` та кешування шарів)
- У корені проєкту налаштовано `.dockerignore`, який виключає папки `bin/`, `obj/`, `.git/`, `.vs/` та локальні файли БД (`data/`), зменшуючи розмір контексту збірки.
- У `Dockerfile` спочатку копіюються файли проєктів (`*.csproj`) і виконується `dotnet restore`, а лише потім копіюється решта вихідного коду — це дозволяє кешувати шар із NuGet-пакетами при змінах у коді:
  ```bash
  docker images bookking-api
  ```

#### Додаткове 2: багатоплатформне збирання образу (`docker buildx`)
Перед запуском тестового контейнера на порту `8080` зупинити поточний стек Compose:
```bash
docker compose down
```

Перевірка доступних цільових платформ та паралельне збирання образу API під архітектури `linux/amd64` та `linux/arm64`:
```bash
docker buildx ls
docker buildx build --platform linux/amd64,linux/arm64 -t bookking-api:multiarch .
```

Локальний запуск зібраного образу під архітектуру хоста (`linux/amd64`) та перевірка API:
```bash
docker buildx build --platform linux/amd64 -t bookking-api:multiarch --load .
docker run -d --name test-multiarch -p 8080:8080 bookking-api:multiarch
docker ps
curl http://localhost:8080/api/v1/books
```

Очищення тестового контейнера після перевірки:
```bash
docker stop test-multiarch
docker rm test-multiarch
```

#### Додаткове 3: оркестрація сервісів та збереження даних (`Docker Compose` + Volumes)
Запуск зв'язки з двох сервісів — `api` (Minimal API на .NET 10) та `web` (Blazor Server) — у спільній bridge-мережі Docker із монтуванням тому для збереження бази даних SQLite:
```bash
docker compose up -d
docker compose ps
```
- Вебінтерфейс `web` (`http://localhost:5090`) взаємодіє з `api` за внутрішнім DNS-іменем сервісу (`http://api:8080`).
- Після перезапуску (`docker compose restart`) стан бази даних зберігається у примонтованому томі `./data:/app/data`.

#### Додаткове 4: публікація та завантаження образу з GitHub Container Registry (`ghcr.io`)
Авторизація в реєстрі (за запитом ввести GitHub Personal Access Token із правом `write:packages`), тегування образу в нижньому регістрі та відправка у `ghcr.io`:
```bash
docker login ghcr.io -u Elementary737
docker tag bookking-api:multiarch ghcr.io/elementary737/bookking-api:latest
docker push ghcr.io/elementary737/bookking-api:latest
```

Перевірка зворотного завантаження образу з реєстру (`pull`):
```bash
docker rmi ghcr.io/elementary737/bookking-api:latest
docker pull ghcr.io/elementary737/bookking-api:latest
docker images ghcr.io/elementary737/bookking-api
```
- Опублікований пакет доступний у вебінтерфейсі GitHub Packages: `https://github.com/Elementary737?tab=packages`