# Copilot Instructions for CodexTest

## Project Overview
- **Type:** .NET 9 C# solution with main app (`CodexTest`) and tests (`CodexTest.Tests`).
- **Architecture:** Clean separation of Domain, Entities, Repositories, Services, Middleware, Validators, and Models.
- **Data Access:** Entity Framework Core via `WeatherDbContext.cs` and repository pattern (`Repositories/`).
- **Business Logic:** Encapsulated in `Services/`.
- **DTOs & Mapping:** DTOs in `Models/`, mapping logic in `Mappings/`.
- **Validation:** Uses FluentValidation (`Validators/`).
- **Exception Handling:** Centralized in `Middleware/ExceptionHandlingMiddlewareExtensions.cs` with custom exceptions in `Exceptions/`.
- **Configuration:** Managed via `appsettings.json` and `appsettings.Development.json`.

## Developer Workflows
- **Build:** Use VS Code task `build` or run `dotnet build CodexTest/CodexTest.sln`.
- **Run (with hot reload):** Use VS Code task `watch` or run `dotnet watch run --project CodexTest/CodexTest.sln`.
- **Publish:** Use VS Code task `publish` or run `dotnet publish CodexTest/CodexTest.sln`.
- **Test:** Place tests in `CodexTest.Tests/`. Run with `dotnet test CodexTest.Tests/CodexTest.Tests.csproj`.

## Project-Specific Patterns
- **Repositories** abstract all data access. Never access `DbContext` directly outside repositories.
- **Services** contain business logic and depend on repositories via interfaces.
- **Custom Exceptions** (e.g., `ConflictException`, `NotFoundException`) are thrown for error cases and handled by middleware.
- **Validation** is performed using FluentValidation before processing requests.
- **Mapping** between entities and DTOs is handled in `Mappings/`.

## Integration & Dependencies
- **Entity Framework Core** for database access.
- **FluentValidation** for request validation.
- **Docker**: `compose.yaml` and `Dockerfile` exist for containerization.

## Examples
- To add a new API endpoint: create a model in `Models/`, a validator in `Validators/`, update mappings, add logic to a service, and expose via controller (if present).
- To add a new exception: create it in `Exceptions/` and handle it in the middleware.

## References
- Key files: `Program.cs`, `WeatherDbContext.cs`, `Repositories/`, `Services/`, `Middleware/ExceptionHandlingMiddlewareExtensions.cs`, `Validators/`, `Mappings/`, `Models/`, `Exceptions/`.

---

**Update this file if you introduce new architectural patterns, workflows, or conventions.**
