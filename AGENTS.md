# Repository Guidelines

## Project Structure & Module Organization
- `src/Engine/` contains the core .NET library source.
- `src/Engine/EFCore/` holds EF Core abstractions and database configuration helpers.
- `src/Engine/Wolverine/` contains Wolverine messaging integration.
- `tests/` is reserved for future test projects.
- `DddOutboxSseWorkshop.slnx` is the solution entry point.

## Build, Test, and Development Commands
- `dotnet restore` restores NuGet dependencies for the solution.
- `dotnet build DddOutboxSseWorkshop.slnx` builds all projects.
- `dotnet test` runs all test projects once they are added under `tests/`.

## Coding Style & Naming Conventions
- Follow standard .NET conventions: `PascalCase` for types/methods, `camelCase` for locals/parameters.
- Use 4-space indentation and file-scoped namespaces where appropriate.
- Keep namespaces aligned with folder structure (e.g., `Engine.EFCore` for `src/Engine/EFCore`).

## Endpoint Conventions (QuickApi)
- Endpoints are implemented with QuickApi Minimal Endpoints, not classic MVC controllers.
- Register endpoints with `builder.Services.AddMinimalEndpoints(...)` and set base path to `api/v1`.
- Activate endpoint mapping with `app.UseMinimalEndpoints()`.
- In each feature, define an endpoint class inheriting from the corresponding QuickApi base type, for example:
  - `PostMinimalEndpoint<Request, Response>("orders")`
- Keep request/response contracts close to the feature (nested records/classes in the feature file).
- Implement business flow in a dedicated handler exposing:
  - `Task<Response> Handle(Request request, CancellationToken cancellationToken)`
- Example route composition: base path `api/v1` + endpoint path `orders` => `POST /api/v1/orders`.

## Testing Guidelines
- Add test projects under `tests/` (e.g., `tests/Engine.Tests/`).
- Name test classes after the subject under test (e.g., `EmailSenderTests`).
- Use `dotnet test` from the repo root to execute the full suite.

## Commit & Pull Request Guidelines
- Commit messages are short, imperative sentences (e.g., "Add base engine module").
- PRs should include:
  - A concise description of the change and its motivation.
  - Any relevant configuration notes (e.g., EF Core or Wolverine setup changes).
  - Screenshots or logs only when behavior is user-visible or runtime-related.

## Configuration & Secrets
- Keep connection strings and credentials out of the repo.
- Prefer environment variables or user secrets for local development.
