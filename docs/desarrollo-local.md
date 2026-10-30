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

La [decisión de Minimal APIs](decisiones/0001-minimal-apis.md) explica la organización. El [recorrido de V1 0.1](flujo-comercial-v1.md) describe rutas, reglas, migraciones, pruebas y supuestos de precios. OpenAPI está en `/openapi/v1.json` en Development y los ejemplos ejecutables en `Sinod.Api/Sinod.Api.http`.

Para un cambio de esquema, generar y revisar una nueva migración; conservar las ya aplicadas:

```bash
dotnet ef migrations add NombreDelCambio --project Sinod.Api --output-dir Data/Migrations -- --environment Development
dotnet ef database update --project Sinod.Api -- --environment Development
```

`docker compose stop` conserva los datos. No usar `docker compose down -v` salvo que se quiera eliminar deliberadamente el volumen de desarrollo. Las pruebas usan otra base, con nombre aleatorio, y nunca eliminan la base configurada para desarrollo.

## Pruebas

Con Docker iniciado, ejecutar los tests desde Rider o con `dotnet test Sinod.Tests/Sinod.Tests.csproj`. Testcontainers levanta un PostgreSQL 18 con puerto dinámico y lo elimina al terminar; no hace falta ejecutar Compose ni levantar `sinod-tests` para las pruebas. Si `SINOD_TEST_POSTGRES` está definida, se usa ese servidor en su lugar, como ocurre en CI. Ver los detalles en la [guía del flujo](flujo-comercial-v1.md#migración-y-verificación).
