# API Design

La API usa ASP.NET Core 8, controllers, MediatR, FluentValidation, JWT Bearer, Swagger y middleware centralizado de errores.

## Endpoints

### Auth

`POST /api/auth/login`

Body:

```json
{
  "email": "operator@seguravida.com"
}
```

Usuarios mock:

- `operator@seguravida.com`
- `adjuster@seguravida.com`
- `auditor@seguravida.com`

### Claims

`GET /api/claims`

Query params:

- `page`
- `pageSize`
- `search`
- `status`
- `branch`
- `fromDate`
- `toDate`

`search` busca por numero de siniestro, numero de poliza o documento del asegurado. El documento se usa solo como criterio de busqueda y no se devuelve ni se registra en logs.

`GET /api/claims/{id}`

Devuelve el detalle del siniestro con dos secciones de lectura:

- `policy`: numero, ramo, vigencia, prima, suma asegurada y estado.
- `insuredParty`: nombre operativo, documento enmascarado y email enmascarado.

No se devuelve documento ni email completos.

### Policies

`GET /api/policies/{policyNumber}`

Endpoint protegido para `OPERATOR`. Se usa en el registro de siniestro para resolver la relacion `INSURED_PARTY -> POLICY -> CLAIM` antes de enviar el formulario.

Respuesta:

```json
{
  "policy": {
    "policyId": "...",
    "policyNumber": "POL-AUTO-001",
    "branch": "AUTO",
    "premium": 1200,
    "startDate": "2026-01-01",
    "endDate": "2026-12-31",
    "insuredAmount": 30000,
    "status": "ACTIVE"
  },
  "insuredParty": {
    "fullName": "Carlos Mendoza",
    "maskedDocumentId": "DNI****01",
    "maskedEmail": "c***@example.com"
  }
}
```

`POST /api/claims`

Body:

```json
{
  "policyNumber": "POL-AUTO-001",
  "type": "ACCIDENT",
  "incidentDate": "2026-06-01",
  "reportedDate": "2026-06-01",
  "claimedAmount": 1200,
  "description": "Minor vehicle collision"
}
```

El caso de uso resuelve la poliza por `policyNumber`. Internamente el dominio conserva `policyId` para relaciones y auditoria.

`POST /api/claims/{id}/start-review`

`POST /api/claims/{id}/approve`

`POST /api/claims/{id}/reject`

`POST /api/claims/{id}/pay`

### Reports

`GET /api/reports/claims-summary`

Query params:

- `fromDate`
- `toDate`

## Roles

- `OPERATOR`: listar, ver detalle y crear siniestro.
- `ADJUSTER`: listar, ver detalle, iniciar revision, aprobar, rechazar y marcar como pagado.
- `AUDITOR`: listar, ver detalle y ver reportes.

## Error format

```json
{
  "traceId": "...",
  "statusCode": 400,
  "message": "...",
  "errors": []
}
```

Mapeo:

- `ValidationException`: 400.
- `DomainException`: 400.
- `NotFoundException`: 404.
- `Unauthorized`: 401.
- `Forbidden`: 403 por middleware de ASP.NET Core.
- Error inesperado: 500.

## Swagger

Swagger queda habilitado en todos los ambientes del challenge:

- Local: `http://127.0.0.1:5080/swagger`
- Docker: `http://localhost:8080/swagger`

Incluye esquema JWT Bearer. El flujo es:

1. Ejecutar `POST /api/auth/login`.
2. Copiar `accessToken`.
3. Click en `Authorize`.
4. Usar `Bearer {token}`.

## Seguridad de datos sensibles

La API no registra `document_id`, `full_name` ni `email` en eventos de negocio. Los endpoints que muestran datos del asegurado devuelven documento y email enmascarados. Los eventos usan:

- `claimId`
- `policyId`
- `userId`
- `role`
- `eventType`
- `correlationId`

## Observabilidad HTTP

Todos los requests pasan por middleware de `CorrelationId` y por `UseSerilogRequestLogging`. Los eventos de negocio agregan `EventType` para que Loki/Grafana pueda filtrarlos con LogQL.
