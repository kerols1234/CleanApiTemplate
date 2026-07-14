# Configuration

Configuration is layered: `appsettings.json` → `appsettings.{Environment}.json` → environment variables → user-secrets (Development). Strongly-typed options POCOs bind each section and are validated on startup where appropriate.

## Connection strings

| Key | Purpose | Required |
| --- | --- | --- |
| `ConnectionStrings:Default` | SQL Server (EF Core) | yes |
| `ConnectionStrings:Redis` | HybridCache L2 + rate-limit/health | no (L1-only if absent) |
| `ConnectionStrings:Hangfire` | Hangfire SQL storage | no (falls back to `Default`, then in-memory) |

Override via environment variables using the double-underscore syntax, e.g. `ConnectionStrings__Default=...`.

## Settings sections

| Section | Keys | Notes |
| --- | --- | --- |
| `Jwt` | `Issuer`, `Audience`, `SigningKey`, `AccessTokenMinutes`, `RefreshTokenDays` | `SigningKey` must be ≥ 32 chars and supplied via secrets/env outside Development |
| `Email` | `Host`, `Port`, `UseSsl`, `Username`, `Password`, `FromAddress`, `FromName` | MailKit SMTP |
| `Firebase` | `ServiceAccountPath` | empty ⇒ push notifications are a no-op |
| `Seed` | `SeedSampleData`, `AdminEmail`, `AdminPassword` | bootstrap admin user + sample data |
| `Database` | `ApplyMigrationsOnStartup`, `RunSeedersOnStartup` | both default true in Development |
| `Hangfire` | `UseSqlServerStorage` | `false` ⇒ in-memory storage |
| `Cors` | `AllowedOrigins` (array) | empty ⇒ permissive dev policy (no credentials) |
| `Sentry` | `Dsn`, `TracesSampleRate` | empty DSN ⇒ disabled |
| `OpenTelemetry` | `ServiceName`, `OtlpEndpoint` | empty endpoint ⇒ traced in-process, not exported |
| `Serilog` | standard Serilog config | console + rolling file; Seq via env in compose |

## Secrets

Never commit secrets. In Development a throwaway `Jwt:SigningKey` ships in `appsettings.Development.json` so the app runs immediately. Everywhere else:

```bash
# Development (per-developer, not committed)
dotnet user-secrets set "Jwt:SigningKey" "<32+ char key>" --project src/CleanApi.Api

# Production (environment variables / secret store)
export Jwt__SigningKey="<32+ char key>"
export ConnectionStrings__Default="Server=...;Database=...;User Id=...;Password=..."
```

The Firebase service-account JSON is `.gitignore`d (`firebase-service-account.json`).

## Environment profiles

| Environment | Behavior |
| --- | --- |
| `Development` | verbose logging, Scalar docs + OpenAPI enabled, migrations + seeding on startup, dev signing key present |
| `Production` | warnings-level logging, docs disabled, `ApplyMigrationsOnStartup=false`, HSTS on; **all secrets must be supplied externally** |

## Startup initialization

`WebApplicationExtensions.InitializeDatabaseAsync` optionally applies migrations (`Database:ApplyMigrationsOnStartup`) and runs seeders (`Database:RunSeedersOnStartup`). Failures are logged, not fatal, so the app still boots if the database is unreachable.
