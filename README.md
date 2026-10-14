# Sistema de Negocio de OkiDoki

## Stack
- ASP.NET Core API
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
dotnet run --project Sinod.Api --launch-profile https
```

The script generates a local password, retains it upon subsequent runs, and configures the connection string in .NET User Secrets. It does not print the password.
`compose.yaml` starts only PostgreSQL.

### /api/health
Tests the real connection to PostgreSQL.

- `200` with `{"status":"Healthy"}` if the check is succesful.
- `503` with `{"status":"Unhealthy"}` if the connection failed.
