# Sistema de Negocio de OkiDoki

## Stack
- ASP.NET Core 10 Minimal APIs
- C#
- PostgreSQL

## Projects
1. Sinod API
2. Sinod Test Suite

## Local development

Requirements: .NET SDK 10, Docker Engine running and OpenSSL.
Execute from project's root directory (make sure to chmod +x setup-local.sh):

```bash
bash scripts/setup-local.sh 
docker compose up -d --wait
dotnet tool restore
dotnet ef database update --project Sinod.Api -- --environment Development
dotnet run --project Sinod.Api --launch-profile https
```

The script generates a local password, retains it upon subsequent runs, and configures the connection string in .NET User Secrets. It does not print the password.
`compose.yaml` starts only PostgreSQL.

### /api/health
Tests the real connection to PostgreSQL.

- `200` with `{"status":"Healthy"}` if the check is succesful.
- `503` with `{"status":"Unhealthy"}` if the connection failed.

## V1 0.1

Recorrido implementado: cliente → oportunidad → cotización versionada → envío/aceptación manual → anticipo del 50% (excepto Municipalidad) → evento pendiente. Incluye CRUD de catálogo y clientes, búsquedas por cliente/teléfono y protección del historial.

- [Decisión de arquitectura: Minimal APIs](docs/decisiones/0001-minimal-apis.md)
- [Rutas, reglas y ejemplos de V1 0.1](docs/flujo-comercial-v1.md)
- [Desarrollo local](docs/desarrollo-local.md)
- Ejemplos HTTP: `Sinod.Api/Sinod.Api.http`
- OpenAPI en Development: `/openapi/v1.json`

```bash
dotnet test Sinod.Tests/Sinod.Tests.csproj
```

Con Docker iniciado, Rider y `dotnet test` levantan automáticamente un PostgreSQL 18 mediante Testcontainers y eliminan el contenedor al terminar. No hace falta configurar variables ni levantar `sinod-tests` manualmente. Si `SINOD_TEST_POSTGRES` está definida, se usa ese servidor y solo se crea y elimina una base temporal; CI ya configura esa variable. Ver la [guía del flujo](docs/flujo-comercial-v1.md#migración-y-verificación).
