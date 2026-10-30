# ADR 0001: Minimal APIs para SINOD

- Estado: aceptada.
- Fecha: 2026-10-27.
- Alcance: API de V1 0.1, recorrido comercial completo.

## Contexto

SINOD usa ASP.NET Core 10, EF Core y PostgreSQL. La superficie HTTP inicial era pequeña: salud y un CRUD parcial de clientes. Necesitamos completar el recorrido cliente → oportunidad → versiones de cotización → aceptación → anticipo cuando corresponda → evento, manteniendo el historial y una estructura fácil de recorrer.

## Decisión

Usamos Minimal APIs agrupadas por recurso con `MapGroup`. `Program.cs` registra dependencias y conecta los módulos de `Endpoints/`. Cada módulo declara rutas, contratos, códigos HTTP y etiquetas OpenAPI. `Services/` contiene consultas y reglas del negocio; `Data/` mantiene EF Core y migraciones. Los contratos de entrada y salida son DTOs; no exponemos entidades ni sus navegaciones.

Se retiran `AddControllers`, `MapControllers` y los controladores. Conservamos `/api/health` y `/api/clients`, incluido PATCH de clientes. Añadimos PUT para reemplazos completos. El PATCH usa el paquete de JSON Patch para System.Text.Json, con una lista explícita de campos y operaciones permitidos y validación posterior al parche.

La validación de DTOs usa DataAnnotations y `AddValidation` de .NET 10. El filtro generado también valida las líneas de cotización. Las condiciones del negocio producen errores 400, 404 o 409 mediante `ProblemDetails`. Se rechazan propiedades JSON desconocidas para detectar errores de contrato. Los enums se devuelven como nombres, por ejemplo `Municipality`, `QuoteAccepted` y `Pending`; los valores numéricos definidos también se aceptan.

## Consecuencias

- Las rutas se encuentran en módulos pequeños y las reglas permanecen independientes de HTTP.
- Los tests HTTP usan `WebApplicationFactory`; los del negocio ejercitan el proveedor real PostgreSQL, incluyendo migraciones, búsquedas y concurrencia.
- La API mantiene un solo proyecto; no incorporamos un repositorio genérico, MediatR ni nuevos proyectos de capas.
- Al añadir un recurso hay que mapearlo explícitamente en `Program.cs` y declarar sus DTOs y reglas. Minimal APIs no elimina la necesidad de diseñar contratos ni de validar estados.
- La migración conserva las URL existentes, pero los clientes deben interpretar enums como nombres en las respuestas. PATCH sigue usando `application/json-patch+json`.

## Alternativas consideradas

Continuar con controladores era válido, pero no había filtros MVC, vistas ni una jerarquía de controladores que justificara mantener esa infraestructura. Poner toda la lógica en lambdas de `Program.cs` dificultaría revisar las reglas y se descartó. No se requiere una mejora de rendimiento para justificar esta decisión: buscamos organización y menos ceremonia en esta etapa.

## Referencias

- [Minimal APIs en ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0).
- [Validación de Minimal APIs en .NET 10](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0?view=aspnetcore-10.0#validation-support-in-minimal-apis).
- [JSON Patch con System.Text.Json](https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-10.0).
