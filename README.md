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
    │   ├── SampleData.cs            # Початковий набір зразкових книг
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
    │   ├── Reports/                # Аналітичні звіти та DTO (Лабораторна 7)
    │   │   ├── StockLine.cs        # DTO для відображення рядка книги в каталозі
    │   │   ├── GroupSummary.cs     # DTO для результатів групування
    │   │   └── ReportService.cs    # Статичні методи звітів на LINQ to Objects
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
        └── Program.cs              # Маршрутизація звітів та CLI-інтерфейс (Лабораторна 7)
```

&#x09;## Build

dotnet build

Запуск аналітичних звітів (Лабораторна 7):

Виводить топ-5 найновіших видань каталогу, відсортованих за спаданням року випуску з детермінованою стабілізацією за ISBN:
dotnet run --project src/Cli -- report-top

Фільтрує та відображає раритетний фонд бібліотеки, вибираючи лише книги, видані до 1950 року:
dotnet run --project src/Cli -- report-low

Формує багаторівневу аналітику згрупованих даних: розподіл за авторами з підрахунком копій та хронологічний зріз за десятиліттями:
dotnet run --project src/Cli -- report-groups

Обчислює зведені інвентарні показники фонду: кількість унікальних назв, суму примірників, мінімум, максимум, середній тираж і діапазон років:
dotnet run --project src/Cli -- report-sum

Демонструє різницю між відкладеним виконанням фільтруючого ітератора Where та негайною матеріалізацією даних через ToList:
dotnet run --project src/Cli -- report-demo

## Звіт до Лабораторної роботи №7 (Аналітика через LINQ to Objects)

### Дослідження відкладеного виконання (Deferred Execution)

Оператор Where у LINQ to Objects реалізує відкладене виконання (deferred execution): під час оголошення запиту var query = localBooks.Where(b => b.Year < 1950); обчислення над колекцією не виконуються, а створюється об'єкт-ітератор, який лише зберігає логіку фільтрації. Безпосереднє виконання запиту та обхід елементів ініціюється викликом термінального методу Count().

Оскільки між першим і другим викликами Count() до вихідної колекції localBooks було додано новий запис, повторний виклик Count() заново пройшов по поточному стану джерела і повернув оновлене значення. Натомість метод ToList() виконав негайну матеріалізацію послідовності у фіксовану колекцію snapshot у пам'яті. Подальше повне очищення вихідного списку через localBooks.Clear() не вплинуло на snapshot, і snapshot.Count зберіг попередній знімок даних.