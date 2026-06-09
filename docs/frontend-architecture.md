# Frontend Architecture

## Vision

La SPA Angular 20 consume la API de Gestion de Siniestros y prioriza una UI operativa: login mock, listado con filtros, detalle con timeline, registro, acciones por rol y reporte agregado.

Stack:

- standalone components;
- lazy loading por rutas;
- signals y computed signals;
- typed reactive forms;
- HttpClient;
- interceptors;
- guards;
- Angular Material;
- Tailwind CSS;
- `.sass` para estilos globales propios.

## Estructura

```text
src/app/
|-- core/
|   |-- auth/
|   |-- config/
|   |-- guards/
|   |-- interceptors/
|   `-- layout/
|-- shared/
|   `-- utils/
`-- features/
    |-- auth/
    |-- claims/
    |   |-- application/
    |   |-- data-access/
    |   |-- domain/
    |   `-- presentation/
    `-- reports/
```

## Routing

- `/login`
- `/claims`
- `/claims/new`
- `/claims/:id`
- `/reports`

Las rutas de feature usan `loadComponent`.

## Claims List

La pantalla de listado soporta:

- busqueda por numero de siniestro, numero de poliza o documento;
- filtro por estado;
- filtro por ramo;
- rango de fechas `fromDate` / `toDate`;
- paginacion con Material paginator;
- boton `Register Claim` visible solo para `OPERATOR`.

## Claim Registration

El registro no solicita datos manuales del asegurado. Al ingresar el numero de poliza, la pantalla consulta `GET /api/policies/{policyNumber}` y muestra datos de poliza + asegurado en modo solo lectura.

Los campos sensibles llegan enmascarados desde backend:

- documento: `maskedDocumentId`;
- email: `maskedEmail`.

El formulario envia solamente los datos del siniestro y el `policyNumber`; las fechas se serializan como `YYYY-MM-DD` para el contrato `DateOnly` de .NET.

## Claim Detail

El detalle muestra:

- datos del siniestro;
- datos de la poliza;
- datos del asegurado;
- linea de auditoria y acciones por rol.

La pantalla no consume datos completos de documento o email.

## Auth Y Roles

`AuthService` mantiene la sesion con signals y `localStorage`.

- `OPERATOR`: puede crear siniestros.
- `ADJUSTER`: puede iniciar revision, aprobar, rechazar y pagar.
- `AUDITOR`: puede entrar a reportes y consultar historial.

Guards:

- `authGuard`
- `roleGuard`

Interceptors:

- `authInterceptor`: agrega `Authorization: Bearer`.
- `errorInterceptor`: maneja `401` y `403`.

Directiva de visibilidad:

- `*appHasRole="'OPERATOR'"`
- `*appHasRole="['ADJUSTER', 'AUDITOR']"`

La directiva centraliza el ocultamiento por rol. La seguridad real sigue en backend.

## Signals

Se usan signals para:

- sesion actual;
- loading/error;
- listado y total de claims;
- detalle actual;
- reporte agregado.

`computed` deriva rol y permisos de UI. Los effects se evitan salvo side effects claros.

## Angular Material, Tailwind Y Sass

Material se usa para controles formales: buttons, form fields, selects, tables, paginator, spinner, toolbar e icons.

Tailwind se usa para layout, spacing y composicion, siguiendo un estilo rapido de frontend sin crear demasiada CSS accidental.

`.sass` queda para tema global, ajustes de Material y estilos que conviene mantener fuera de templates.

## OpenAPI Generator

Se mantiene instalado y con script `openapi:generate`, pero el MVP usa cliente manual tipado.

Decision:

- El contrato todavia es pequeno.
- Los servicios manuales (`ClaimsApiService`, `ReportsApiService`) son claros y faciles de ajustar.
- Cuando Swagger se congele, se puede generar cliente en `data-access/api-client`.

```powershell
npm run openapi:generate
```

## Docker

El frontend tiene Dockerfile multi-stage:

- build con Node 22.13;
- runtime con Nginx;
- fallback a `index.html` para rutas SPA.
