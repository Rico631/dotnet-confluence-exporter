# Code Documentation Analysis Specification

## Purpose

Позволяет явно выбирать контракт обработчика запросов при анализе документации, чтобы один и тот же анализатор мог использоваться для разных моделей `RequestHandlerName` без скрытого глобального состояния.

## Requirements

### Requirement: Analyzer accepts explicit handler contract
Система MUST поддерживать явную настройку имени интерфейса обработчика запросов при вызове анализа документации.

#### Scenario: Default contract remains compatible
- **WHEN** вызывается анализ без явной настройки
- **THEN** используется контракт `IRequestHandler` по умолчанию

#### Scenario: Explicit custom contract is honored
- **WHEN** вызывается анализ с параметром `RequestHandlerName`, равным `ICommandHandler`
- **THEN** анализирует только типы, реализующие этот интерфейс, и формирует документацию для их запросов и ответов

### Requirement: Analysis uses configured contract for collection
Система MUST передавать выбранное имя интерфейса в `HandlerCollector`, чтобы отбор обработчиков и извлечение типов запросов происходили на основе одного и того же контракта.

#### Scenario: Contract is propagated to collector
- **WHEN** создаётся `HandlerCollector` через анализатор
- **THEN** коллекционер использует значение `RequestHandlerName`, переданное из настроек анализа

#### Scenario: Contract mismatch is not silently guessed
- **WHEN** в проекте присутствуют несколько контрактов обработчиков
- **THEN** используется явно переданное имя интерфейса, а не вычисление по случайному или неявному правилу
