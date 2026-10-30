# V1 0.1: recorrido comercial

Implementa el núcleo de [V1 0.1: recorrido comercial completo](negocio/alcance-y-pendientes.md#v1-01-recorrido-comercial-completo) y sus definiciones de [Cliente, Oportunidad, Cotización y Evento](negocio/dominio-y-flujo-comercial.md). El [contexto de negocio](negocio/README.md) y las [políticas de precios y pagos](negocio/precios-y-politicas.md) están versionados en el repositorio. El criterio técnico se demuestra con un caso particular y uno municipal en `Sinod.Tests/CommercialFlowTests.cs`. La validación del equipo con casos reales sigue siendo el criterio de aceptación del producto.

## Recursos y operaciones

Todas las rutas empiezan en `/api`. GET por ID devuelve 404 si no existe. POST de recursos devuelve 201 y Location; PUT devuelve 200; DELETE devuelve 204. Los datos inválidos producen 400 y los conflictos comerciales 409.

| Recurso | Operaciones |
| --- | --- |
| `/companies` | GET lista, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` |
| `/pricing-profiles` | GET lista, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` |
| `/services` | GET lista, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` |
| `/clients` | GET lista con `?search=`, GET `/{id}`, POST, PUT/PATCH `/{id}`, DELETE `/{id}` |
| `/opportunities` | GET lista con `?search=` y/o `?clientId=`, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` sin historial |
| `/opportunities/{id}/quotations` | GET versiones, POST nueva versión, GET `/{quotationId}` |
| `/opportunities/{id}/quotations/{quotationId}/send` | POST: registrar manualmente el envío |
| `/opportunities/{id}/quotations/{quotationId}/accept` | POST: registrar la aceptación de esa versión enviada |
| `/opportunities/{id}/deposits` | GET anticipos, POST recepción de anticipo |
| `/opportunities/{id}/win` | POST con `availabilityConfirmed` y `reason`; devuelve el evento |
| `/opportunities/{id}/lose` | POST con `reason`; conserva la oportunidad y las cotizaciones |
| `/events` | GET con `?search=` y/o `?clientId=`, GET `/{id}`, PUT `/{id}` para realizar/cancelar |
| `/health` | GET: 200/503 según conectividad PostgreSQL, sin caché |

OpenAPI se publica en `/openapi/v1.json` en Development. `Sinod.Api/Sinod.Api.http` contiene ambos recorridos ejecutables en el cliente HTTP de Rider.

## Recorrido

1. Registrar empresas y perfiles de precios cuando corresponda y servicios del catálogo. Un perfil establece precio y duración base; el servicio puede sobrescribir el precio.
2. Buscar por cliente o teléfono y registrar al cliente si no existe. Se admite un prospecto sin oportunidades.
3. Crear una oportunidad en `Discovery`. Solo es obligatorio el cliente; fecha, lugar y servicios pueden completarse después mediante PUT. Las fechas se envían en UTC, terminadas en `Z`.
4. Generar una versión de cotización con servicios, cantidades, duración, descuento porcentual por línea y transporte. Para cotizar son obligatorios fecha y dirección del evento. Se conserva precio, nombre del servicio, duración base, importes, cliente/empresa, lugar, fecha, condición municipal y texto. Cada nueva versión recibe un número consecutivo y pasa la oportunidad a `QuoteInProgress`.
5. Registrar el envío de una versión (`QuoteSent`), luego su aceptación (`QuoteAccepted`). El envío real y la conversación permanecen en WhatsApp. Se puede aceptar una versión anterior enviada; su ID queda registrado y solo puede haber una aceptada por oportunidad.
6. Para cualquier cliente distinto de Municipalidad, registrar uno o más anticipos recibidos, con fecha y referencia de comprobante. Deben sumar al menos el 50% del total aceptado. Se redondea el mínimo hacia arriba al céntimo y no se permiten anticipos mayores al total. Municipalidad no requiere ni registra anticipo para este cierre.
7. Confirmar disponibilidad y registrar el motivo mediante `/win`. Se marca `Won` y crea un evento `Pending` en una sola transacción. Repetir un cierre devuelve el mismo evento; una carrera concurrente puede devolver 409 y debe reconsultarse. Un índice único garantiza como máximo un evento por oportunidad.
8. Buscar la oportunidad y el evento por nombre del cliente, nombre de empresa o teléfono, incluso con espacios, guiones o prefijo internacional. El detalle de la oportunidad reúne versiones, anticipos, motivo de cierre e ID del evento. Desde el evento se vuelve por `opportunityId` y `quotationId` a la propuesta aceptada.

## Cálculo explícito de V1 0.1

Moneda: CRC. Cada importe se redondea a dos decimales, alejándose de cero en los empates.

- Precio aplicado: precio específico del servicio, si existe; en otro caso, precio del perfil.
- Subtotal de línea = precio aplicado × cantidad × duración solicitada / duración base. No se cotiza por debajo de la duración base.
- Descuento de línea = subtotal × `discountPercent / 100`. Se admite de 0 a 30%, ingresado por el operador.
- `Company.BaseRate` se interpreta como porcentaje de recargo: 5 significa 5%. Sin empresa, el recargo es 0%.
- Recargo = suma de líneas después de descuento × porcentaje de empresa / 100.
- Total = subtotal − descuentos + recargo + transporte.

Esta versión usa prorrateo lineal de duración. El operador valida e ingresa los descuentos y el transporte; no se implementan todavía las reglas automáticas por hora adicional, combinación de personajes ni cálculo por Google Maps. La nota de precios no establece una tarifa mínima de transporte utilizable. Estas decisiones permiten registrar una propuesta revisada sin inventar esa tarifa. Deben revisarse con el negocio antes de automatizar precios más complejos.

El texto se ingresa explícitamente o se genera con el detalle, la fecha, el lugar y el total. Se guarda en la misma versión. Cambiar precios, nombres, empresa o información de la oportunidad después no modifica las versiones anteriores. La excepción municipal queda fijada en la versión generada, por lo que cambiar la empresa actual del cliente no elimina el requisito de anticipo de una propuesta ya aceptada.

## Historial y límites de edición

Las cotizaciones generadas y los anticipos recibidos son registros conservados. Una corrección de propuesta genera una nueva versión. No hay PUT ni DELETE de cotizaciones o anticipos. Tampoco se expone POST/DELETE directo de eventos: nacen del cierre y se cancelan con motivo, conservando el vínculo comercial.

Se pueden borrar clientes, empresas, perfiles y servicios sin referencias; se devuelve 409 cuando existen dependencias. Solo una oportunidad en descubrimiento y sin historial puede eliminarse. Las demás se pierden con motivo. Las oportunidades aceptadas o cerradas no se editan. No se cambia el cliente después de generar la primera cotización. Una oportunidad con anticipos no se marca perdida sin resolver antes el pago; los reembolsos quedan fuera de este alcance.

El evento puede pasar de `Pending` a `Completed` o `Cancelled`; la cancelación exige motivo. Los estados terminales no se revierten. La aceptación y el cierre no envían mensajes ni verifican automáticamente disponibilidad: el operador la confirma explícitamente.

## Migración y verificación

`CompleteCommercialFlow` añade snapshots, partidas, anticipos, condiciones de cierre e índices únicos. Cambia los borrados en cascada por restricciones. Las cotizaciones preexistentes se conservan y numeran por oportunidad según ID. Como el modelo anterior no guardaba importes ni partidas, no se inventa ese historial: las versiones incompletas no pueden enviarse, aceptarse ni usarse para un cierre nuevo. Los eventos antiguos conservan `quotationId` nulo. Si ya existen múltiples eventos o cotizaciones aceptadas por oportunidad, la migración se detiene con una explicación y requiere resolver esos datos antes de aplicarla.

La API no aplica migraciones al arrancar. Desde la raíz:

```bash
dotnet tool restore
dotnet ef database update --project Sinod.Api -- --environment Development
dotnet run --project Sinod.Api --launch-profile https
```

Las pruebas de integración crean una base con nombre aleatorio, aplican la migración inicial, insertan registros legados, actualizan el esquema y eliminan únicamente esa base al terminar.

En local, basta con tener Docker Desktop/Engine iniciado. Si `SINOD_TEST_POSTGRES` no está definida, el fixture usa Testcontainers para levantar PostgreSQL 18, espera a que esté listo y obtiene su conexión mediante un puerto asignado dinámicamente. El contenedor se comparte entre los tests de `CommercialFlowTests` y se elimina al terminar la clase, incluso cuando falla una prueba. La primera ejecución puede tardar más mientras se descargan las imágenes. No se usa el volumen de desarrollo ni se necesita un contenedor llamado `sinod-tests`.

```bash
dotnet test Sinod.Tests/Sinod.Tests.csproj
```

En Rider, ejecutar `CommercialFlowTests` desde el icono junto a la clase o desde Unit Tests. No hace falta configurar variables en cada instalación ni versionar `.idea` o un `.runsettings`. Si el proyecto ya estaba abierto al cambiar el `.csproj`, esperar a que Rider lo recargue y restaure el paquete. No es necesario iniciar la API ni aplicar migraciones manualmente para ejecutar las pruebas. Si Docker no está disponible y no se configura un servidor externo, las pruebas fallan al iniciar el fixture en lugar de omitirse.

Para usar un servidor externo, definir `SINOD_TEST_POSTGRES` con una cadena de conexión cuyo usuario tenga permiso para crear y eliminar bases. En ese caso no se inicia ningún contenedor y solo se crea y elimina la base temporal. GitHub Actions ya configura esa variable para su servicio PostgreSQL 18; conserva el mismo workflow. Si se había configurado la variable manualmente en Rider o en la terminal, quitarla para activar Testcontainers.
