# Proposal

## Why

Сгенерированная Confluence-документация показывает handler и дерево вызовов, но не позволяет быстро открыть соответствующий исходный код в GitLab. Из-за этого проверка реализации требует ручного поиска файлов в репозитории.

## What Changes

- Добавить в документацию handler относительный путь исходного файла.
- Добавить в элементы call tree относительные пути исходных файлов методов.
- При настроенном GitLab repository URL рендерить handler и методы call tree как ссылки на файлы в выбранном GitLab reference.
- Сохранить текущий текстовый вывод, если GitLab URL или путь исходного файла отсутствует.
- Корректно формировать URL для относительных путей с вложенными каталогами и специальными символами.

## Capabilities

### New Capabilities

- `code-documentation-source-links`: Ссылки из сгенерированной Confluence-документации на исходные файлы handler и методов call tree в GitLab.

### Modified Capabilities

- Нет.

## Impact

- `ConfluenceExporter.CodeDocumentation.Models`: модели `HandlerDocumentation`, `MethodDocumentation` и параметры рендера.
- `ConfluenceExporter.CodeDocumentation.Analysis`: получение относительных путей из Roslyn symbols и передача их в модели документации.
- `ConfluenceExporter.CodeDocumentation.Renders.HandlerDocumentationRenderer`: генерация ссылок в XHTML для Confluence.
- Тесты рендера и анализа исходных путей.