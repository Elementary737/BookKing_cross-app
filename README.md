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

│   ├── sample.csv          # Книги у форматі CSV (валідні та пошкоджені записи)

│   ├── sample.json         # Книги у форматі JSON

│   └── mixed.csv           # Змішані дані (рядки B; для книг та R; для читачів)

└── src/

&#x20;   ├── Core/

&#x20;   │   ├── Core.csproj

&#x20;   │   ├── EnvironmentInfo.cs

&#x20;   │   ├── Dto/            # DTO тижня 3 (формат перенесення даних)

&#x20;   │   │   ├── BookDto.cs

&#x20;   │   │   └── ReaderDto.cs

&#x20;   │   ├── Domain/         # НОВЕ: багата доменна модель (Лабораторна 4)

&#x20;   │   │   ├── Book.cs

&#x20;   │   │   ├── Loan.cs

&#x20;   │   │   └── LoanStatus.cs

&#x20;   │   └── Import/         # Логіка парсингу та імпорту файлів

&#x20;   │       ├── ImportResult.cs

&#x20;   │       ├── BookCsvImporter.cs

&#x20;   │       ├── BookJsonImporter.cs

&#x20;   │       └── MixedCsvImporter.cs

&#x20;   └── Cli/

&#x20;       ├── Cli.csproj

&#x20;       └── Program.cs



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

