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
│   ├── sample.csv          # Книги у форматі CSV (Лабораторна 3)
│   ├── sample.json         # Книги у форматі JSON (Лабораторна 3)
│   ├── mixed.csv           # Змішані дані B; та R; (Лабораторна 3)
│   └── books.json          # Збереження каталогу на диск (Лабораторна 5)
│
├── logs/
│   └── sink_notifications.log # Журнал сповіщень FileSink (Лабораторна 6)
│
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── SampleData.cs           # Початковий набір зразкових книг
    │   ├── INotificationSink.cs    # Контракт каналу сповіщень (Лабораторна 6)
    │   │
    │   ├── Dto/
    │   │   ├── BookDto.cs
    │   │   └── ReaderDto.cs
    │   │
    │   ├── Domain/
    │   │   ├── Book.cs
    │   │   ├── Loan.cs
    │   │   └── LoanStatus.cs
    │   │
    │   ├── Events/
    │   │   └── LibraryChanged.cs   # Доменні події та типи змін (Лабораторна 6)
    │   │
    │   ├── Abstractions/
    │   │   └── IBookStore.cs
    │   │
    │   ├── Storage/
    │   │   ├── InMemoryBookStore.cs
    │   │   ├── FileBookStore.cs
    │   │   ├── CachingBookStore.cs
    │   │   └── StoreFactory.cs
    │   │
    │   ├── Services/
    │   │   └── LendingService.cs   # Події, DI (Sink), ImportBooks (Лабораторна 6)
    │   │
    │   └── Import/
    │       ├── ImportResult.cs
    │       ├── BookCsvImporter.cs
    │       ├── BookJsonImporter.cs
    │       └── MixedCsvImporter.cs
    │
    └── Cli/
        ├── Cli.csproj
        ├── Sinks.cs                # ConsoleSink, FileSink, CompositeNotificationSink (Лабораторна 6)
        └── Program.cs
```

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

Запуск демонстраційного сценарію Лабораторної 6:
dotnet run --project src\Cli

Перевірка створеного лог-файлу сповіщень:
type logs\sink_notifications.log

## Звіт до Лабораторної роботи №6 (Події, Делегати та Сповіщення)

### 1. Доменні події предметної області
Подіями вважаються лише операції мутації агрегатів: `BookAdded` (поповнення фонду), `CopyIssued` (видача примірника читачу із фіксацією терміну) та `CopyReturned` (повернення на баланс). Операції читання/пошуку (`Find`, `All`) подіями не є, оскільки не змінюють стан системи.

### 2. Різниця між подією та прямим викликом логера
Подія реалізує патерн Publish-Subscribe і слабку зв'язність (Loose Coupling): сервіс нічого не знає про слухачів, тоді як прямий виклик логера нав'язує жорстку залежність. Два конкретні наслідки:

Принцип OCP: Додавання нових обробників (SMS, моніторинг, кеш) не потребує зміни коду LendingService — підписка виконується ззовні.

Ізоляція та тестування: Модульні тести перевіряють бізнес-правила без створення моків логера (Changed просто рівний null), а збій у сторонньому логері не ламає виконання самої операції повернення чи видачі.
}