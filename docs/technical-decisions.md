# Technical Decisions

## .NET 8

.NET 8 es el target del backend porque es LTS, estable para APIs empresariales y cumple el requisito del challenge. Aunque la maquina local tenga SDK superior, todos los proyectos apuntan a `net8.0`.

## Angular 20

Angular 20 se usa porque el challenge lo pide y soporta standalone components, signals, typed forms, guards, interceptors y lazy routes.

Se eligio Angular Material para controles formales y Tailwind CSS para acelerar layout/composicion manteniendo `.sass` para tema global y ajustes propios.

## Clean Architecture

La solucion separa `Domain`, `Application`, `Infrastructure` y `Api` para mantener reglas de negocio independientes de HTTP, EF Core y frameworks.

- `Domain`: entidades, enums, excepciones, reglas y maquina de estados.
- `Application`: comandos, queries, DTOs, validadores, handlers y puertos.
- `Infrastructure`: EF Core, repositorios, Unit of Work, JWT, seed y reportes.
- `Api`: controllers, auth, Swagger, middlewares y Serilog.

## DDD Tactico

`Claim` es el agregado principal. Expone metodos de negocio para transicionar estado y genera historial internamente. Esto impide que un controller o repositorio cambie estado sin auditoria.

La deteccion de duplicados vive en Application porque necesita consultar persistencia, pero delega la similitud a una politica de dominio.

## Oracle vs SQL Server

Decision: usar SQL Server en Docker como motor relacional equivalente para el take-home.

El challenge menciona Oracle, pero SQL Server reduce friccion local y mantiene los conceptos importantes: modelo normalizado, indices, migraciones, seed data y stored procedure.

Para migrar a Oracle:

- cambiar provider EF Core;
- ajustar connection string;
- regenerar migraciones;
- revisar tipos `uniqueidentifier`, `datetimeoffset`, `decimal` y `date`;
- convertir `dbo.GetClaimsSummary` a PL/SQL;
- actualizar scripts bajo `infra/docker`.

## Auditoria

Se eligio auditoria por logica de aplicacion/dominio. Cada metodo de estado en `Claim` agrega `ClaimStatusHistory` y la unidad de trabajo persiste todo junto.

En un escenario regulado real se podria complementar con triggers, CDC o temporal tables para defensa ante cambios directos en base.

## OpenAPI Generator

OpenAPI Generator queda instalado y con script `openapi:generate`, pero el MVP usa servicios manuales tipados.

Motivo: el contrato es pequeno y cambiar rapido; el cliente generado aportaria mas ruido que valor en esta etapa. Cuando Swagger se estabilice, se puede generar en `features/claims/data-access/api-client`.

## Observabilidad

Serilog escribe JSON compacto por consola y envia a Loki cuando existe `Loki:Url`. Grafana queda provisionado con datasource y dashboard para eventos de negocio.

Los logs evitan datos personales sensibles en claro y usan IDs tecnicos mas `CorrelationId`.

## CI/CD

GitHub Actions separa build/test/lint/build frontend y Docker build. No despliega porque el challenge no lo requiere.
