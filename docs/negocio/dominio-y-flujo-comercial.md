# Dominio y flujo comercial

Fuente: notas de entidades, flujo comercial y alcances de SINOD. Ver la [procedencia y los criterios de uso](README.md). Las diferencias entre el dominio previsto y la API actual se indican expresamente.

## Conceptos

| Concepto | Significado y relaciones |
| --- | --- |
| Cliente | Contacto que puede registrarse desde la primera consulta, con o sin empresa. Puede tener varias oportunidades independientes. «Prospecto» describe a un cliente sin oportunidad; no es una entidad adicional en V1. |
| Empresa o institución | Organización opcional asociada al cliente. Su tipo y sus condiciones comerciales intervienen en la propuesta. Municipalidad es la única excepción al anticipo. |
| Servicio | Oferta contratable, como personaje, inflable, pintacaritas o animación. Las notas distinguen personajes propios, alquilados o especiales y tamaños de inflables. La API actual organiza precio y duración base mediante perfiles de precios. |
| Oportunidad | Intención concreta de contratar un evento. Reúne lo solicitado, sus cotizaciones y su historial comercial. Puede existir antes de conocer todos los datos necesarios para cotizar. |
| Cotización | Propuesta económica versionada de una oportunidad. Conserva lo ofrecido y permite identificar la versión enviada y la aceptada. |
| Anticipo | Pago recibido que se registra para cumplir la condición de cierre de clientes distintos de Municipalidad. |
| Evento | Compromiso operativo que nace de una oportunidad ganada. Mantiene el vínculo con la oportunidad y la versión aceptada sin duplicar el historial comercial. Cada oportunidad puede crear como máximo un evento. |
| Usuario | Persona que accede al sistema. La definición de una entidad no acredita que existan autenticación, permisos o asignación de responsables operativos. |
| Interacción y seguimiento | Registro de contacto o tarea comercial con responsable y fecha. Son actividades del caso, no estados comerciales. Su gestión funcional está pendiente en la entrega actual. |
| Mensaje | Comunicación de un cliente o usuario; WhatsApp es el canal contemplado en las notas. Su integración automática está pendiente. |

## Recorrido de negocio

1. Recibir una consulta por WhatsApp, identificar o registrar al cliente y crear una oportunidad para el evento solicitado.
2. Reunir fecha, lugar y servicios; revisar disponibilidad y preparar el precio.
3. Generar una versión de cotización y registrar manualmente que se envió al cliente.
4. Si el cliente pide cambios, conservar la propuesta anterior y generar una nueva versión.
5. Si acepta, identificar la versión concreta aceptada y cumplir la condición de cierre de [precios y políticas](precios-y-politicas.md).
6. Confirmar disponibilidad y ganar la oportunidad; crear un único evento pendiente de realizar.
7. Realizar o cancelar el evento, conservando el caso comercial y su historial.

Si no se concreta la venta, registrar el motivo de pérdida y conservar el historial. Las notas también contemplan pausar y retomar oportunidades con una fecha de revisión; esa operación todavía no está expuesta por la API actual.

## Estados de la oportunidad

| Estado de negocio | Significado | Soporte actual |
| --- | --- | --- |
| Descubrimiento | Se aclara lo que necesita el cliente. | `Discovery`. |
| Preparando cotización | Se prepara o revisa una propuesta. | `QuoteInProgress`. |
| Cotización enviada | Se registró el envío de una versión y se espera respuesta. | `QuoteSent`. |
| Aceptada, pendiente de cierre | Se identifica la versión aceptada y falta completar el cierre. | `QuoteAccepted`. |
| Ganada | Se cumplieron las condiciones y se creó el evento. | `Won`. |
| Perdida | La contratación no se concretará; se conserva su motivo. | `Lost`. |
| En pausa | Se pospone la decisión con motivo y próxima revisión; al retomar se recupera la etapa anterior. | Pendiente; no existe en el enum ni en las operaciones actuales. |

Registrar una respuesta o un seguimiento no debe cambiar por sí solo la etapa comercial. Los días 1, 3 y 7 aparecen en las notas como ejemplos de seguimiento, no como plazos obligatorios ni como una regla de pérdida automática.

## Cotizaciones y conservación del historial

Una nueva versión no modifica ni elimina las anteriores. Debe conservar los servicios, cantidades o duración, precios aplicados, descuentos, transporte, total y texto efectivamente ofrecidos. Los cambios posteriores del catálogo o de las condiciones del cliente no reescriben esa propuesta.

Las notas definen versiones en borrador, enviadas, aceptadas, rechazadas o vencidas. La API actual permite crear, registrar envío y aceptar; aunque el modelo enumera rechazo y vencimiento, todavía no hay operaciones para gestionarlos. Vencer o rechazar una versión no implica por sí mismo perder la oportunidad.

En V1, marcar una versión como enviada registra una acción humana; no envía un mensaje por WhatsApp. La aceptación identifica una versión concreta, que puede ser anterior a la más reciente. La versión aceptada es la base del cierre y queda relacionada con el evento.

## Condiciones de cierre

- Para cualquier cliente distinto de Municipalidad se requiere aceptación y anticipo del 50% antes de crear el evento.
- Municipalidad confirma sin solicitar ni registrar anticipo para ese cierre; el pago puede quedar pendiente según su proceso.
- Se confirma disponibilidad antes de crear el evento. La API actual registra una confirmación humana; no consulta una agenda ni asigna recursos automáticamente.
- La condición municipal queda fijada en la versión cotizada. Cambiar después la empresa del cliente no permite omitir la política de pago de la versión aceptada.
- Reintentar o ejecutar cierres concurrentes no debe crear más de un evento para una oportunidad.

## Ciclo del evento y búsqueda

El evento nace pendiente y luego puede marcarse realizado o cancelado. La implementación actual exige motivo de cancelación y no permite revertir un estado terminal. Las notas contemplan registrar el importe finalmente generado y consultar los pagos por separado del precio cotizado; esa liquidación final todavía no está implementada.

El objetivo de búsqueda es localizar oportunidades y eventos por cliente, teléfono, empresa, fecha, lugar, servicio o estado, sin conocer códigos de cotización. La API actual ofrece búsqueda por cliente o teléfono y empresa en oportunidades y eventos, y filtro por cliente. Los filtros adicionales y la agenda forman parte del alcance pendiente; consultar las rutas y restricciones implementadas en la [guía técnica del flujo](../flujo-comercial-v1.md).
