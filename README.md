# CleanApi

A production-shaped **ASP.NET Core 10 Web API** built with Clean Architecture, Domain-Driven Design, and CQRS. Batteries included — every cross-cutting concern is wired up, guarded, and demonstrated with a working `Products` module.

## Highlights

- **Clean Architecture + DDD** — `Domain` → `Application` → `Infrastructure` → `Api`, with dependency inversion.
- **CQRS with MediatR** and pipeline behaviors: validation, logging, performance, unhandled-exception, and caching.
- **EF Core 10 (SQL Server)** — entity configurations, **database views** (keyless), **stored procedures**, owned value objects, soft-delete, auditing, and domain-event dispatch on save.
- **Repository + Specification** (Ardalis.Specification) alongside a `IApplicationDbContext` unit of work.
- **Identity + JWT** with **permission-based authorization** (roles carry permission claims; `[HasPermission(...)]` enforces them).
- **Result pattern** → RFC 7807 ProblemDetails, centralized in one translator.
- **Observability**: Serilog (console + file + Seq), Sentry, `IExceptionHandler` global error handling, health checks.
- **OpenAPI + Scalar** docs UI with JWT auth built in.
- **Resilience & throughput**: rate limiting, HybridCache (in-memory + optional Redis L2), Hangfire jobs + a `Channel`-based background service.
- **Documents & messaging**: MailKit email, QuestPDF, ClosedXML (Excel), Firebase push notifications — all behind interfaces.
- **FluentValidation**, dynamic paging/sorting, reusable `IQueryable`/`IEnumerable` extensions.
- **Docker** (chiseled, non-root) + `docker-compose` (SQL Server + Redis + Seq), **xUnit** unit + integration tests (Testcontainers), `.editorconfig`, and Central Package Management.

## Quick start

### 1. Start local infrastructure (SQL Server + Redis + Seq)

```bash
docker compose up -d
```

> No Docker? Point `ConnectionStrings:Default` at any SQL Server (or SQL LocalDB) instead.

### 2. Apply migrations and run

```bash
dotnet ef database update -p src/CleanApi.Infrastructure -s src/CleanApi.Api
dotnet run --project src/CleanApi.Api
```

In **Development**, migrations are applied and data is seeded automatically on startup, so `dotnet run` alone is usually enough.

Then open:

- **API docs (Scalar):** `http://localhost:5083/scalar`
- **Health:** `http://localhost:5083/health`
- **Hangfire dashboard:** `http://localhost:5083/hangfire`
- **Seq logs (if using compose):** `http://localhost:8081`

### 3. Log in

A seeded administrator is created on first run:

| Email | Password |
| --- | --- |
| `admin@example.com` | `Admin123!$` |

`POST /api/v1/auth/login` returns an access + refresh token. Send `Authorization: Bearer <token>` on subsequent calls (or click **Authorize** in Scalar).

## Project layout

```
src/
  CleanApi.Domain          Entities, value objects, domain events, repository & auth contracts
  CleanApi.Application      CQRS modules, pipeline behaviors, Result, paging, service interfaces
  CleanApi.Infrastructure  EF Core, migrations, repositories, Identity/JWT, email/pdf/excel/firebase, jobs, seeders
  CleanApi.Api             Program, controllers, DI, exception handlers, authorization, OpenAPI
tests/
  CleanApi.Domain.UnitTests
  CleanApi.Application.UnitTests
  CleanApi.Api.IntegrationTests   WebApplicationFactory + Testcontainers
```

## The `Products` module — the pattern to copy

Everything you need to add a feature is demonstrated under `src/CleanApi.Application/Modules/Products`:

- **Commands**: create, update, delete (soft), adjust-stock (transaction example).
- **Queries**: get-by-id, paged + cacheable list, category **summary (DB view)**, low-stock (**stored procedure**), Excel export, PDF export.
- One file per feature holds the request record, its FluentValidation validator, and its handler.
- Mapping is source-generated with **Mapperly** (`ProductMapper`).

Controllers in `src/CleanApi.Api/Controllers/ProductsController.cs` gate each action with `[HasPermission(Permissions.Products.*)]` and return `result.ToActionResult()`.

## Configuration & secrets

Config is layered: `appsettings.json` → `appsettings.{Environment}.json` → environment variables / user-secrets.

**External integrations are off by default and only activate when configured:**

| Integration | Enabled when… | Otherwise |
| --- | --- | --- |
| Redis (HybridCache L2) | `ConnectionStrings:Redis` is set | In-memory L1 cache only |
| Hangfire SQL storage | `Hangfire:UseSqlServerStorage` is `true` | In-memory storage |
| Sentry | `Sentry:Dsn` is set | Disabled |
| Firebase push | `Firebase:ServiceAccountPath` points to a service-account file | No-op |

**Secrets — never commit these.** In Development, a throwaway `Jwt:SigningKey` lives in `appsettings.Development.json` so the app runs immediately. For any real environment, provide secrets via user-secrets or environment variables:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<a strong 32+ char key>" --project src/CleanApi.Api
```

Production requires `Jwt:SigningKey`, `ConnectionStrings:Default`, and any integration secrets to be supplied externally (see `appsettings.Production.json`). The Firebase service-account JSON is `.gitignore`d.

## Library & licensing notes

This template deliberately favors permissively-licensed libraries. Be aware of the following:

- **MediatR is pinned to `12.5.0`** — the last Apache-2.0 release. MediatR 13+ is commercial; do not bump without a license review.
- **QuestPDF** runs under its free **Community** license (set in `Program.cs`). Commercial use above the revenue threshold requires a paid license.
- **ClosedXML (MIT)** is used for Excel instead of EPPlus (EPPlus 5+ requires a commercial license for commercial use).
- Tests use **NSubstitute** and **AwesomeAssertions** (both free) rather than Moq / FluentAssertions v8.
- Mapping uses **Mapperly** (source generator) rather than AutoMapper (also commercial since 2025).

## Testing

```bash
dotnet test
```

Unit tests run everywhere. The integration tests spin up a real SQL Server via **Testcontainers**; if Docker isn't available they skip themselves so the suite stays green.

## Docker

```bash
docker build -t cleanapi .
docker run -p 8080:8080 -e ConnectionStrings__Default="<your connection string>" cleanapi
```

The runtime image is the chiseled (distroless) ASP.NET base and runs as a non-root user.

## Adding a migration

```bash
dotnet ef migrations add <Name> -p src/CleanApi.Infrastructure -s src/CleanApi.Api -o Persistence/Migrations
```

To add a **view** or **stored procedure**, define its keyless result type in `Domain` (with `[DbView("...")]` for views), then create the SQL in a migration using the `CreateOrAlterView` / `CreateOrAlterStoredProcedure` helpers — the mapping is wired automatically by reflection.
