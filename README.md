# CICD-Demo

A hands-on CI/CD learning project: ASP.NET Core Web API (.NET 10) + PostgreSQL + Vue 3 (TypeScript),
built, tested and deployed with GitHub Actions, Docker and Render.

## Features

- Product catalogue API: search, category/price/stock filters, sorting, pagination
- Product management (create / update / soft-delete) protected by an admin API key
- Catalogue statistics endpoint
- PostgreSQL storage with EF Core migrations (applied automatically on startup)
- Health checks: `/health/live` (no dependencies) and `/health/ready` (checks the database)
- Vue storefront with filters and pagination
- CI: backend build + tests (including migrations on real PostgreSQL), frontend lint/format/tests/build, Docker image builds
- Post-deployment smoke test that verifies the exact commit is live

## Repository layout

```
.github/workflows/        CI and post-deployment smoke test
docs/                     Deployment runbook (rollback, secrets, database)
scripts/                  smoke-test.sh
src/Backend/
  CICD.Demo.Api/          Configuration/ Contracts/ Data/ (+Migrations) Endpoints/ Models/ Validation/
  CICD.Demo.Api.Tests/    xUnit tests (in-memory DB; one optional real-PostgreSQL test)
src/Frontend/cicd-demo-frontend/   Vue 3 app (Vitest, ESLint, Prettier)
docker-compose.yml        Local stack: db + api + frontend
```

## Run locally

### Everything with Docker

```
docker compose up --build
```

- Frontend: http://localhost:8080
- API: http://localhost:5080 (try `/api/products`, `/health/ready`)
- PostgreSQL: `localhost:5432` (data persists in the `db-data` volume; `docker compose down -v` wipes it)

Defaults are for local use only; copy `.env.example` to `.env` to change them.

### Without Docker for the apps

```
docker compose up -d db                       # just the database
dotnet run --project src/Backend/CICD.Demo.Api --launch-profile http
cd src/Frontend/cicd-demo-frontend && npm ci && npm run dev
```

`appsettings.Development.json` already points the API at the compose database
(local-only credentials) and sets a dev admin key (`dev-admin-key`).

## API

| Method | Route | Notes |
| --- | --- | --- |
| GET | `/api/products` | `search`, `category`, `minPrice`, `maxPrice`, `inStock`, `sortBy` (`id`/`name`/`price`), `sortOrder`, `page`, `pageSize` (max 50) |
| GET | `/api/products/{id}` | |
| GET | `/api/products/categories` | |
| GET | `/api/products/stats` | counts, categories, inventory value |
| POST | `/api/products` | needs `X-Api-Key` |
| PUT | `/api/products/{id}` | needs `X-Api-Key` |
| DELETE | `/api/products/{id}` | needs `X-Api-Key`; soft delete |

Write endpoints return `403` when no `Admin__ApiKey` is configured and `401` for a missing/wrong key.

## Configuration

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings__Default` or `DATABASE_URL` | PostgreSQL connection (`DATABASE_URL` accepts `postgres://user:pass@host:port/db`) |
| `Admin__ApiKey` | Key required for write endpoints |
| `Cors__AllowedOrigins__0..n` | Allowed frontend origins |
| `Database__MigrateOnStartup` | `false` to skip automatic migrations |
| `VITE_API_BASE_URL` | Frontend build-time API address |

The API refuses to start without a database connection.

## Tests

```
dotnet test                                   # backend (in-memory database)
cd src/Frontend/cicd-demo-frontend && npm run test:unit -- --run
```

To also run the migration test against real PostgreSQL (it drops and recreates that database):

```
TEST_POSTGRES_CONNECTION="Host=localhost;Database=cicd_test;Username=cicd;Password=cicd_dev_password" dotnet test
```

## Database migrations

```
dotnet tool install --global dotnet-ef        # once
dotnet ef migrations add <Name> --project src/Backend/CICD.Demo.Api --output-dir Data/Migrations
```

A test fails if the model changes without a matching migration.

## Deployment

See [docs/deployment-runbook.md](docs/deployment-runbook.md) for the pipeline, Render setup, rollback and secrets.
