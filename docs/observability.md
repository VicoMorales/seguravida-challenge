# Observability

## Componentes

- Serilog en API con JSON compacto.
- `Serilog.Sinks.Grafana.Loki` para enviar logs a Loki.
- Loki `3.2.1` como backend de logs.
- Grafana `11.2.2` con datasource y dashboard provisionados.

## Configuracion

En Docker, la API recibe:

```yaml
Loki__Url: "http://loki:3100"
```

Si `Loki:Url` esta vacio, la API solo escribe logs JSON por consola. Esto permite correr tests y desarrollo local sin Loki.

Archivos:

- `infra/loki/loki-config.yml`
- `infra/grafana/provisioning/datasources/loki.yml`
- `infra/grafana/provisioning/dashboards/dashboards.yml`
- `infra/grafana/dashboards/seguravida-claims-observability.json`

## Correlation ID

La API agrega o propaga `X-Correlation-Id`. Si el cliente no envia el header, se usa `HttpContext.TraceIdentifier`.

El middleware agrega el valor al contexto de Serilog para que aparezca en logs HTTP y eventos de negocio.

## Eventos Auditables

Eventos emitidos como logs estructurados:

- `ClaimCreated`
- `ClaimStatusChanged`
- `ClaimApproved`
- `ClaimRejected`
- `ClaimPaid`

Campos seguros:

- `EventType`
- `ClaimId`
- `PolicyId`
- `UserId`
- `Role`
- `CorrelationId`

## Busquedas En Grafana

En Grafana Explore, usar:

```logql
{app="seguravida-claims-api"} |= "ClaimCreated"
{app="seguravida-claims-api"} |= "ClaimStatusChanged"
{app="seguravida-claims-api"} |= "ClaimApproved"
{app="seguravida-claims-api"} |= "ClaimRejected"
{app="seguravida-claims-api"} |= "ClaimPaid"
```

El dashboard `SeguraVida Claims Observability` incluye panel de eventos de negocio y panel de errores manejados.

## Seguridad De Logs

No se deben loguear en claro:

- `document_id`
- `full_name`
- `email`

Los eventos de negocio usan IDs tecnicos: `partyId`, `policyId`, `claimId`, `userId` y `correlationId`.

Nota: durante tests locales en Windows pueden aparecer logs internos de ASP.NET DataProtection sobre llaves locales. No contienen datos personales del dominio.

## Validacion Manual

1. Levantar stack:

```powershell
docker compose up frontend api database loki grafana --build
```

2. Entrar a `http://localhost:4200/login`.
3. Crear un siniestro como `OPERATOR`.
4. Iniciar revision/aprobar/pagar como `ADJUSTER`.
5. Abrir `http://localhost:3000` y buscar los eventos en Explore o en el dashboard.
