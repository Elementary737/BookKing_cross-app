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
│   ├── sample.csv          # Книги у форматі CSV (Лабораторна 3)
│   ├── sample.json         # Книги у форматі JSON (Лабораторна 3)
│   ├── mixed.csv           # Змішані дані B; та R; (Лабораторна 3)
│   └── books.json          # Збереження каталогу на диск (Лабораторна 5, створюється під час запуску)
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── SampleData.cs           # Початковий набір зразкових книг для сховища
    │   ├── Dto/                    # DTO (формат серіалізації та імпорту)
    │   │   ├── BookDto.cs
    │   │   └── ReaderDto.cs
    │   ├── Domain/                 # Доменна модель та інваріанти (Лабораторна 4)
    │   │   ├── Book.cs
    │   │   ├── Loan.cs
    │   │   └── LoanStatus.cs
    │   ├── Abstractions/           # Контракти (Лабораторна 5)
    │   │   └── IBookStore.cs       # Інтерфейс сховища книг
    │   ├── Storage/                # Реалізації сховищ (Лабораторна 5)
    │   │   ├── InMemoryBookStore.cs # Сховище в оперативній пам'яті (RAM)
    │   │   ├── FileBookStore.cs    # Файлове персистентне сховище (JSON)
    │   │   ├── CachingBookStore.cs # Декоратор кешування (Додаткове завдання 1)
    │   │   └── StoreFactory.cs     # Фабрика вибору сховища (Додаткове завдання 3)
    │   ├── Services/               # Сервісний шар (Лабораторна 5)
    │   │   └── LendingService.cs   # Бізнес-операції (видача, повернення, предикативний пошук)
    │   └── Import/                 # Логіка парсингу та імпорту
    │       ├── ImportResult.cs
    │       ├── BookCsvImporter.cs
    │       ├── BookJsonImporter.cs
    │       └── MixedCsvImporter.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs              # Composition Root та демонстрація сценаріїв

&#x09;## Інваріанти

| \*\*Метод\*\*            | \*\*Інваріант (бізнес-правило)\*\*                     | \*\*Тип винятку\*\*               |

|----------------------|----------------------------------------------------|-------------------------------|

| `Book.Create`        | Ключі `id`, `isbn` та назва непорожні              | `ArgumentException`            |

| `Book.Create`        | Рік (1450..поточний) та кількість ($\\ge 0$) валідні | `ArgumentOutOfRangeException`  |

| `Book.IssueCopy`     | Заборонено видачу, коли залишок примірників = 0   | `InvalidOperationException`    |

| `Loan.Open`          | Ідентифікатори `id` та `readerId` непорожні        | `ArgumentException`            |

| `Loan.OpenForReader` | Читач не може мати більше 5 активних видач         | `InvalidOperationException`    |

| `Loan.Close`         | Дата повернення не може бути раніше дати видачі    | `ArgumentOutOfRangeException`  |

| `Loan.Close`         | Не можна закрити чужу книгу або вже закриту видачу | `InvalidOperationException`    |

| `Loan.TransitionTo`  | Заборонено невалідне переведення статусу видачі    | `InvalidOperationException`    |



&#x09;## Build

dotnet build



&#x09;## Збірка та запуск



&#x20;   Збірка розв'язку:

dotnet build



&#x20;   Запуск стандартного CSV-імпорту (за замовчуванням data/sample.csv):

dotnet run --project src\\Cli



&#x20;   Запуск з явним шляхом до CSV:

dotnet run --project src\\Cli -- data\\sample.csv


Запуск (Лабораторна 5):
Режим оперативної пам'яті (InMemory):

dotnet run --project src/Cli
(Створює InMemoryBookStore, дані після завершення скидаються до початкових).

Файловий режим (Persistent JSON):

Bash
dotnet run --project src/Cli -- --file
(Створює FileBookStore, дані зберігаються та оновлюються у файлі data/books.json).
