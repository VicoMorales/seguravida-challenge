# Architecture

## Vision General

SeguraVida Claims se implementa como monorepo con separacion clara entre backend, frontend, infraestructura y documentacion. El backend sigue Clean Architecture con DDD tactico; el frontend usa feature folders y facades con signals.

```mermaid
flowchart LR
    User["Usuario"] --> Web["Angular 20 SPA"]
    Web --> Api["ASP.NET Core API"]
    Api --> App["Application: use cases"]
    App --> Domain["Domain: Claim aggregate"]
    App --> Infra["Infrastructure"]
    Infra --> Db["SQL Server"]
    Api --> Loki["Loki"]
    Loki --> Grafana["Grafana"]
```

## Clean Architecture

Las dependencias apuntan hacia el dominio. `Domain` no conoce frameworks, persistencia ni HTTP.

```mermaid
flowchart TD
    Api["Api: controllers, auth, swagger, middleware"] --> Application["Application: commands, queries, validators, ports"]
    Infrastructure["Infrastructure: EF Core, JWT, repositories"] --> Application
    Infrastructure --> Domain["Domain: entities, enums, exceptions, state machine"]
    Application --> Domain
    Tests["Tests"] --> Domain
    Tests --> Application
```

## DDD Tactico

El agregado `Claim` protege invariantes:

- poliza vigente en fecha de incidente;
- fecha de incidente menor o igual a fecha de reporte;
- monto reclamado menor o igual a suma asegurada;
- transiciones validas;
- aprobacion con monto y notas de peritaje;
- historial obligatorio por cada cambio de estado.

Los metodos de negocio son `StartReview`, `Approve`, `Reject` y `MarkAsPaid`. No se exponen setters publicos indiscriminados para campos criticos.

## Flujo De Registro

```mermaid
sequenceDiagram
    actor Operator
    participant Web as Angular SPA
    participant Api as ClaimsController
    participant App as CreateClaimCommandHandler
    participant Domain as Claim
    participant Db as SQL Server

    Operator->>Web: Completa formulario
    Web->>Api: POST /api/claims
    Api->>App: CreateClaimCommand
    App->>Db: Busca poliza y duplicados
    App->>Domain: Claim.Report(...)
    Domain-->>App: Claim + historial REPORTED
    App->>Db: Guarda agregado
    App-->>Api: claimId
    Api-->>Web: 201 Created
```

## Flujo De Cambio De Estado

```mermaid
stateDiagram-v2
    [*] --> REPORTED
    REPORTED --> UNDER_REVIEW
    UNDER_REVIEW --> APPROVED
    UNDER_REVIEW --> REJECTED
    APPROVED --> PAID
    REJECTED --> [*]
    PAID --> [*]
```

Cada transicion agrega un registro en `CLAIM_STATUS_HISTORY` desde el dominio y se persiste en la misma unidad de trabajo.

## Seguridad

La API usa JWT Bearer mock. Roles:

- `OPERATOR`: registra y consulta.
- `ADJUSTER`: consulta, inicia revision, aprueba, rechaza y paga.
- `AUDITOR`: solo lectura, historial y reportes.

Los controllers aplican autorizacion por rol y delegan reglas a Application/Domain.

## Observabilidad

Serilog emite JSON compacto a consola y, en Docker, envia eventos a Loki. Grafana se provisiona con datasource y dashboard. Los logs de negocio usan identificadores tecnicos y `CorrelationId`, sin datos personales sensibles.
