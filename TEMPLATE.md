# CleanApi — `dotnet new` template

[![CI](https://github.com/kerols1234/CleanApiTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/kerols1234/CleanApiTemplate/actions/workflows/ci.yml)
[![Template Validation](https://github.com/kerols1234/CleanApiTemplate/actions/workflows/template-validation.yml/badge.svg)](https://github.com/kerols1234/CleanApiTemplate/actions/workflows/template-validation.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-yellow.svg)](LICENSE)

This repository is a **`dotnet new` custom template**. Installing it adds a `cleanapi` template you can use to scaffold new Clean-Architecture .NET 10 Web API solutions.

> This file documents the template itself and is **excluded** from generated projects.
> For what the generated project contains and how to run it, see [README.md](README.md) and the [docs/](docs/) guides.

## Install

From this repository's root:

```bash
dotnet new install .
```

To update after editing the template, reinstall with `--force`:

```bash
dotnet new install . --force
```

To uninstall:

```bash
dotnet new uninstall <full-path-to-this-folder>
```

## Generate a project

```bash
dotnet new cleanapi -n MyCompany.MyApi
```

This creates a `MyCompany.MyApi/` folder with the full solution. The template engine renames the `CleanApi` source token everywhere — solution, projects, folders, namespaces, project references, and config — and generates a fresh `UserSecretsId` for the API project.

### Feature toggles

Optional subsystems can be excluded at generation time (all default to **on**):

| Option | Default | Turns off |
| --- | --- | --- |
| `--UseFirebase` | `true` | Firebase push notifications (a no-op notifier is used instead) |
| `--UseSentry` | `true` | Sentry error tracking |
| `--UseOpenTelemetry` | `true` | OpenTelemetry tracing + metrics |

Example — a lean build with none of those:

```bash
dotnet new cleanapi -n MyCompany.MyApi --UseFirebase false --UseSentry false --UseOpenTelemetry false
```

When a feature is turned off its code, package references, and DI wiring are removed from the generated project entirely. (Hangfire, Redis/HybridCache, and email are always included but stay dormant until configured — they are the `#if` pattern to copy if you want to make them toggleable too.)

Then:

```bash
cd MyCompany.MyApi
docker compose up -d          # SQL Server + Redis + Seq
dotnet run --project src/MyCompany.MyApi.Api
```

See the generated project's `README.md` for everything else.

## What's inside the template

- `.template.config/template.json` — template manifest (`shortName: cleanapi`, `sourceName: CleanApi`).
- `global.json` — pins the .NET SDK feature band (10.0.x).
- `Directory.Packages.props` — Central Package Management; **all** package versions are pinned here.
- `Directory.Build.props` — shared TFM, nullable, analyzers, and warning policy.

## Requirements

- .NET SDK **10.0.300** or newer.
- (Optional) Docker, for `docker compose` local infrastructure and the Testcontainers integration tests.

## Licensing

The template itself is [MIT](LICENSE). Generated projects ship with a copy of that `LICENSE` — replace it with your own project's license.

## Packaging as a NuGet template pack (optional)

Add a `.csproj` with `<PackageType>Template</PackageType>` that includes this folder as content, then `dotnet pack` and publish. Consumers install with `dotnet new install <PackageId>`.
