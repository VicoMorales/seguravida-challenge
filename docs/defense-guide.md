# Defense Guide

## Mensaje Principal

El proyecto prioriza reglas de negocio defendibles, separacion de responsabilidades y facilidad de ejecucion local. La API no depende del frontend para proteger reglas: cada invariante vive en Domain o Application y esta cubierta por tests.

## Puntos Para Defender

- Clean Architecture: `Api` no contiene reglas; `Application` orquesta casos de uso; `Domain` protege invariantes; `Infrastructure` implementa detalles tecnicos.
- DDD tactico: `Claim` es el agregado principal y encapsula la maquina de estados.
- Auditoria: cada cambio de estado genera `CLAIM_STATUS_HISTORY` desde el dominio, no desde controllers.
- Seguridad: JWT mock con roles y autorizacion por endpoint; el frontend oculta acciones solo como mejora UX.
- Logs: Serilog emite eventos auditables sin datos personales sensibles.
- Persistencia: SQL Server se usa como equivalente relacional estable; la migracion a Oracle esta documentada.
- Frontend: Angular 20 standalone, signals, Material para controles y Tailwind para layout rapido y consistente.
- CI/CD: pipeline separado por backend, frontend y Docker build.

## Evidencia Tecnica

- `dotnet build` Release correcto.
- `dotnet test` Release correcto: 12 tests.
- `npm run lint` correcto.
- `npm run build` correcto.
- `docker compose config` correcto.

## Decisiones Con Trade-Off

- SQL Server sobre Oracle XE: reduce friccion local del evaluador. Cambiar a Oracle implica provider EF Core, connection string, migraciones, tipos SQL y PL/SQL.
- Cliente OpenAPI no generado aun: el contrato todavia es pequeno; servicios manuales tipados evitan ruido. El script queda preparado para cuando Swagger se estabilice.
- Auditoria por aplicacion: testable y explicita. En produccion regulada se podria complementar con triggers, CDC o temporal tables.
- Auth mock: suficiente para probar roles y autorizacion; no intenta resolver identidad real.

## Preguntas Esperables

**Por que la maquina de estados esta en Domain?**  
Porque es una regla central del negocio y debe ser igual para API, jobs, tests o cualquier entrada futura.

**Como se evita saltar la auditoria?**  
Los cambios se hacen con metodos del agregado que agregan historial internamente. No hay setters publicos para estado.

**Que pasa con datos sensibles?**  
Existen en base porque el dominio los necesita, pero los logs de negocio solo usan IDs tecnicos y correlation id.

**Como probar observabilidad?**  
Levantar compose completo, ejecutar acciones de negocio y buscar `ClaimCreated`, `ClaimApproved`, etc. en Grafana Explore.

## Limitaciones Conocidas

- No hay refresh tokens ni passwords reales.
- No hay UI visual avanzada de dashboard; el reporte es tabla funcional.
- No se implementan triggers defensivos de auditoria.
- No se hizo despliegue real; CI llega hasta build/test/docker build.
