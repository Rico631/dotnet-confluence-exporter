# Tasks

## 1. Подготовка и baseline

- [x] 1.1 Зафиксировать baseline вывода инструмента до переименования: выполнить `dotnet run --project AppDocs/AppDocs.csproj` и сохранить stdout в файл вне репозитория (например, `$env:TEMP\appdocs-baseline.txt`); проверить, что файл создан и содержит JSON документации обработчика `AddCartItem`.
- [x] 1.2 Удалить локальные сборочные каталоги `AppDocs/bin` и `AppDocs/obj` (они перечислены в `.gitignore`); проверить, что `Test-Path AppDocs/bin` и `Test-Path AppDocs/obj` возвращают `False`, а `git status` не показывает изменений в отслеживаемых файлах.

## 2. Переименование проекта

- [x] 2.1 Переместить каталог проекта: `git mv AppDocs ConfluenceExporter`; проверить, что `Test-Path ConfluenceExporter` = `True`, `Test-Path AppDocs` = `False`, а `git status` помечает файлы как `renamed:`.
- [x] 2.2 Переместить файл проекта: `git mv ConfluenceExporter/AppDocs.csproj ConfluenceExporter/ConfluenceExporter.csproj`; проверить, что в каталоге `ConfluenceExporter` ровно один файл `*.csproj` и он называется `ConfluenceExporter.csproj`.
- [x] 2.3 Обновить `WebApp4.slnx`: заменить `<Project Path="AppDocs/AppDocs.csproj" Id="b2fb251c-cfd5-4be4-8f48-b663c570c0ee" />` на `<Project Path="ConfluenceExporter/ConfluenceExporter.csproj" Id="b2fb251c-cfd5-4be4-8f48-b663c570c0ee" />`; проверить, что `Test-Path ConfluenceExporter/ConfluenceExporter.csproj` = `True` и значение `Id` не изменилось.

## 3. Корневой namespace и имя сборки

- [x] 3.1 Заменить корневой сегмент namespace в коде: во всех 29 файлах `.cs` каталога `ConfluenceExporter` заменить `AppDocs.` на `ConfluenceExporter.` (41 вхождение в объявлениях `namespace` и директивах `using`, в том числе в `Program.cs`); проверить, что `git grep -n "AppDocs"` не возвращает результатов.
- [x] 3.2 Проверить, что изменились только строки `namespace`/`using`: вывод `git diff -U0 -- ConfluenceExporter` содержит только строки, начинающиеся с `namespace ` или `using `; внутренние сегменты (`ApiDocumentGenerator`, `CodeDocumentation`, `Models`, `Internal`, `Analysis`, `Renders`) и имена типов сохранены.
- [x] 3.3 Проверить, что `RootNamespace` и `AssemblyName` не заданы явно и выводятся из имени проекта: `Select-String -Path ConfluenceExporter/ConfluenceExporter.csproj -Pattern 'RootNamespace|AssemblyName'` не находит совпадений, а после сборки (задача 5.1) существует файл `ConfluenceExporter/bin/Debug/net10.0/ConfluenceExporter.dll`.

## 4. Документация

- [x] 4.1 Обновить `README.md`: добавить назначение инструмента (`ConfluenceExporter` — сбор документации приложения и рендер XHTML для Confluence) и команды сборки и запуска (`dotnet build WebApp4.slnx`, `dotnet run --project ConfluenceExporter/ConfluenceExporter.csproj`); проверить, что `Select-String -Path README.md -Pattern 'AppDocs'` не находит совпадений, а упоминание `ConfluenceExporter` присутствует.

## 5. Проверка

- [x] 5.1 Собрать решение: `dotnet build WebApp4.slnx`; проверить, что сборка завершилась без ошибок, проект `ConfluenceExporter` включён в сборку и создан `ConfluenceExporter/bin/Debug/net10.0/ConfluenceExporter.dll`. Тестовые проекты в решении отсутствуют, поэтому регрессия подтверждается этой задачей и задачами 5.2–5.3.
- [x] 5.2 Проверить отсутствие остатков старого имени: `git grep -n "AppDocs"` по отслеживаемым файлам возвращает пустой результат (в том числе для `WebApp4.slnx`).
- [x] 5.3 Проверить сохранение поведения: выполнить `dotnet run --project ConfluenceExporter/ConfluenceExporter.csproj` и сравнить stdout с baseline из задачи 1.1 (`Compare-Object $baseline $new`); различий быть не должно.
