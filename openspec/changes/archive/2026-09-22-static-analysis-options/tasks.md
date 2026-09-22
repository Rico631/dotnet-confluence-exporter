# Tasks

## 1. API and configuration

- [x] 1.1 Add `AnalysisOptions` with `RequestHandlerName` defaulting to `IRequestHandler`, and verify the default value is covered by a unit test or compile-time usage.
- [x] 1.2 Update `DocumentationAnalyzer.AnalyzeAsync<T>` to accept `AnalysisOptions` and pass the value into `HandlerCollector`, and verify the method signature remains clear for multi-call usage in other projects.
- [x] 1.3 Update `HandlerCollector` constructor to receive the configured contract name and keep backward compatibility when no options are supplied, and verify the collector still finds current MediatR-style handlers.

## 2. Validation and regression checks

- [x] 2.1 Add or adjust tests covering a custom handler contract (for example `ICommandHandler`) and verify the custom `RequestHandlerName` is respected.
- [x] 2.2 Run the relevant .NET test/build validation for the project and confirm the analyzer still works with the default `IRequestHandler` path.
