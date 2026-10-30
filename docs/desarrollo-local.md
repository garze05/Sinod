# Desarrollo local

Requisitos: SDK .NET 10, Docker Desktop/Engine iniciado y OpenSSL. Ejecutar desde la raíz:

```bash
bash scripts/setup-local.sh
docker compose up -d --wait
dotnet tool restore
dotnet ef database update --project Sinod.Api -- --environment Development
dotnet run --project Sinod.Api --launch-profile https
```

El script conserva la contraseña local y configura `ConnectionStrings:Sinod` en User Secrets. Los secretos no se versionan. Compose levanta PostgreSQL 18 y conserva sus datos en un volumen. La API ejecuta EF Core con Npgsql; no aplica migraciones automáticamente.

`GET /api/health` responde 200 con `{"status":"Healthy"}` si PostgreSQL es accesible y 503 con `{"status":"Unhealthy"}` si falla. Solo mide conectividad; no confirma que el esquema esté actualizado. La respuesta no se almacena en caché ni expone detalles de conexión.

## Organización

- `Sinod.Api/Endpoints`: rutas Minimal API, contratos HTTP y etiquetas OpenAPI.
- `Sinod.Api/Contracts`: DTOs y validación de entrada.
- `Sinod.Api/Services`: consultas, cálculo y transiciones del negocio.
- `Sinod.Api/Entities`: entidades persistidas y valores del dominio.
- `Sinod.Api/Data`: DbContext y migraciones EF Core.
- `Sinod.Tests`: pruebas HTTP de salud y del recorrido comercial sobre PostgreSQL real.
- `docs/negocio`: contexto empresarial, dominio, políticas y alcance pendientes, versionados con el código.

La [decisión de Minimal APIs](decisiones/0001-minimal-apis.md) explica la organización. El [recorrido de V1 0.1](flujo-comercial-v1.md) describe rutas, reglas, migraciones, pruebas y supuestos de precios. OpenAPI está en `/openapi/v1.json` en Development y los ejemplos ejecutables en `Sinod.Api/Sinod.Api.http`.

Para un cambio de esquema, generar y revisar una nueva migración; conservar las ya aplicadas:

```bash
dotnet ef migrations add NombreDelCambio --project Sinod.Api --output-dir Data/Migrations -- --environment Development
dotnet ef database update --project Sinod.Api -- --environment Development
```

`docker compose stop` conserva los datos. No usar `docker compose down -v` salvo que se quiera eliminar deliberadamente el volumen de desarrollo. Las pruebas usan otra base, con nombre aleatorio, y nunca eliminan la base configurada para desarrollo.

## Pruebas

Con Docker iniciado, ejecutar los tests desde Rider o con `dotnet test Sinod.Tests/Sinod.Tests.csproj`. Testcontainers levanta un PostgreSQL 18 con puerto dinámico y lo elimina al terminar; no hace falta ejecutar Compose ni levantar `sinod-tests` para las pruebas. Si `SINOD_TEST_POSTGRES` está definida, se usa ese servidor en su lugar, como ocurre en CI. Ver los detalles en la [guía del flujo](flujo-comercial-v1.md#migración-y-verificación).

## Criterio de finalización para agentes de IA

Las instrucciones obligatorias para todo el repositorio están en [AGENTS.md](../AGENTS.md). Cada cambio realizado por un agente debe incluir una actualización útil del README y de los documentos afectados por el comportamiento final. Si cambia una ruta, regla, validación, configuración o migración, actualizar también sus ejemplos e instrucciones.

Crear o ajustar tests en `Sinod.Tests` para las funcionalidades críticas y las reglas de negocio afectadas. Una corrección de bug debe tener una prueba de regresión que represente el caso real. Validar resultados observables, casos límite y rechazos relevantes, usando PostgreSQL real cuando la persistencia sea parte del comportamiento. No deshabilitar tests ni debilitar aserciones para ocultar un problema.

Antes de declarar terminado un cambio, ejecutar `dotnet test Sinod.Tests/Sinod.Tests.csproj` sin filtros y comprobar que la suite completa pasa sin omisiones. Si no se puede ejecutar o falla, la validación queda pendiente y debe indicarse en la entrega. Los cambios exclusivamente documentales no necesitan tests artificiales, pero sí revisar coherencia, enlaces y ejecutar la suite existente.

Ante dudas de negocio, consultar [docs/negocio](negocio/README.md) y las fuentes indicadas en `AGENTS.md` antes de inventar requisitos. El contexto está dentro del repositorio y no requiere carpetas personales ni servicios de sincronización. Distinguir las políticas confirmadas y las propuestas pendientes del comportamiento implementado; documentar las decisiones de negocio relevantes y representarlas en los tests.
