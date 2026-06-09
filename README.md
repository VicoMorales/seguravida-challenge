# SeguraVida Claims

Monorepo para el challenge tecnico Fullstack Engineer de SeguraVida S.A. El sistema implementa un modulo de Gestion de Siniestros con API .NET 8, SPA Angular 20, SQL Server en Docker, JWT mock, auditoria de cambios de estado, testing, CI/CD y observabilidad con Serilog, Loki y Grafana.

## Stack

- Backend: .NET 8, ASP.NET Core, Clean Architecture, DDD tactico, MediatR, FluentValidation.
- Persistencia: EF Core, SQL Server 2022 en Docker, migraciones, seed data y stored procedure de reporte.
- Seguridad: JWT Bearer mock con roles `OPERATOR`, `ADJUSTER`, `AUDITOR`.
- Frontend: Angular 20, standalone components, signals, typed forms, Angular Material, Tailwind CSS y `.sass`.
- Observabilidad: Serilog JSON, Loki y Grafana provisionado.
- Calidad: xUnit, FluentAssertions, Moq, WebApplicationFactory, ESLint, Prettier.
- CI/CD: GitHub Actions para build, tests, lint y Docker build.

## Estructura

```text
seguravida-claims/
|-- src/
|   |-- backend/
|   |   |-- SeguraVida.Claims.Api/
|   |   |-- SeguraVida.Claims.Application/
|   |   |-- SeguraVida.Claims.Domain/
|   |   |-- SeguraVida.Claims.Infrastructure/
|   |   `-- SeguraVida.Claims.Tests/
|   `-- frontend/
|       `-- seguravida-claims-web/
|-- infra/
|   |-- docker/
|   |-- grafana/
|   `-- loki/
|-- docs/
|-- .github/workflows/
|-- docker-compose.yml
`-- SeguraVida.Claims.sln
```

## Requisitos Previos

- .NET SDK 8.
- Node.js 22.13+ recomendado.
- npm.
- Docker Desktop.

## Levantar Todo Con Docker

```powershell
docker compose up frontend api database loki grafana --build
```

URLs:

- Frontend: `http://localhost:4200/login`
- Swagger: `http://localhost:8080/swagger`
- Health API: `http://localhost:8080/health`
- Loki ready: `http://localhost:3100/ready`
- Grafana: `http://localhost:3000` (`admin` / `admin`)

La API aplica migraciones al iniciar en Docker mediante `Database__ApplyMigrations=true`, por lo que la base queda con schema, indices, seed data y stored procedure.

## Usuarios Mock

```text
operator@seguravida.com -> OPERATOR
adjuster@seguravida.com -> ADJUSTER
auditor@seguravida.com -> AUDITOR
```

Login:

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "operator@seguravida.com" }
```

Usar el token como `Bearer {token}` en Swagger.

## Backend Local

```powershell
dotnet restore .\SeguraVida.Claims.sln --configfile .\NuGet.config
dotnet build .\SeguraVida.Claims.sln --configuration Release --no-restore
dotnet test .\SeguraVida.Claims.sln --configuration Release --no-restore
dotnet run --project .\src\backend\SeguraVida.Claims.Api\SeguraVida.Claims.Api.csproj --urls http://127.0.0.1:5080
```

Swagger local: `http://127.0.0.1:5080/swagger`

## Frontend Local

```powershell
cd .\src\frontend\seguravida-claims-web
npm install
npm run lint
npm run build
npm start
```

El frontend local arranca en `http://localhost:4200/` y consume `http://localhost:8080/api` por defecto.

## Endpoints Principales

- `POST /api/auth/login`
- `GET /api/claims?page&pageSize&search&status&branch&fromDate&toDate`
- `GET /api/claims/{id}`
- `POST /api/claims`
- `POST /api/claims/{id}/start-review`
- `POST /api/claims/{id}/approve`
- `POST /api/claims/{id}/reject`
- `POST /api/claims/{id}/pay`
- `GET /api/reports/claims-summary?fromDate&toDate`

## Observabilidad

Los logs de la API salen en JSON y se envian a Loki cuando `Loki__Url` esta configurado. Grafana queda provisionado con datasource Loki y dashboard `SeguraVida Claims Observability`.

Busquedas utiles en Grafana Explore:

```logql
{app="seguravida-claims-api"} |= "ClaimCreated"
{app="seguravida-claims-api"} |= "ClaimStatusChanged"
{app="seguravida-claims-api"} |= "ClaimApproved"
{app="seguravida-claims-api"} |= "ClaimRejected"
{app="seguravida-claims-api"} |= "ClaimPaid"
```

Los eventos de negocio registran IDs tecnicos (`claimId`, `policyId`, `userId`, `correlationId`) y no imprimen `document_id`, `full_name` ni `email` en claro.

## CI/CD

`.github/workflows/ci.yml` contiene jobs separados para:

- backend build;
- backend tests;
- frontend install;
- frontend lint;
- frontend build;
- Docker build API;
- Docker build Frontend.

## Supuestos Y Pendientes

- Se usa SQL Server como motor relacional equivalente para acelerar el take-home. La migracion a Oracle esta documentada en `docs/database-model.md`.
- El login es mock por email y rol; no implementa passwords porque el objetivo es validar autorizacion, no identidad real.
- El cliente OpenAPI esta preparado, pero el frontend usa servicios manuales tipados para mantener control del MVP.
- En un escenario regulado real se podria complementar la auditoria de aplicacion con triggers, CDC o temporal tables.

- [Frontend architecture](docs/frontend-architecture.md)
- [Observabilidad](docs/observability.md)
- [Testing strategy](docs/testing-strategy.md)
- [Defense guide](docs/defense-guide.md)
