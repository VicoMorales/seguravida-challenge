# Testing Strategy

## Current automated coverage

- Backend integration tests cover claim creation, claim detail with policy/insured-party sections, and policy lookup by number with masked document/email.
- Frontend build validates the Angular templates and typed client contracts.

## Unit tests

Los tests unitarios cubren reglas de dominio:

- poliza vigente en fecha de incidente;
- fecha de incidente posterior a reporte;
- monto reclamado mayor a suma asegurada;
- duplicado de siniestro;
- transiciones invalidas;
- aprobacion sin monto;
- aprobacion sin notas;
- happy path de estado `REPORTED -> UNDER_REVIEW -> APPROVED -> PAID`;
- rechazo desde `UNDER_REVIEW`;
- historial por cada cambio de estado.

## Integration test

Se agrego una prueba de integracion con `WebApplicationFactory`:

- login mock por API;
- creacion de siniestro por API;
- consulta de detalle;
- validacion de historial inicial.

La prueba usa EF Core InMemory para no depender de Docker/SQL Server en CI.

## Commands

```powershell
dotnet build .\SeguraVida.Claims.sln --configuration Release --no-restore
dotnet test .\SeguraVida.Claims.sln --configuration Release --no-build --no-restore
```

Resultado verificado en Etapa 5:

- 12 tests superados.

## Frontend

Validaciones:

```powershell
cd .\src\frontend\seguravida-claims-web
npm run lint
npm run build
```

Resultado verificado:

- lint correcto;
- build correcto.

## CI

GitHub Actions ejecuta jobs separados para backend build, backend tests, frontend install, frontend lint, frontend build y Docker build de API/frontend.
