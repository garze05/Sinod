# Alcance y pendientes de negocio

Fuente: notas de versiones, alcances, problemas de V1 y riesgos de SINOD. Ver la [procedencia y los criterios de uso](README.md). Los nombres de las entregas se conservan como referencia de planificación; no implican que todo su contenido esté implementado o validado por el equipo.

## V1 0.1: recorrido comercial completo

Objetivo: demostrar con casos reales que el equipo puede registrar una consulta, cotizarla, encontrarla y convertirla en un evento sin perder el historial.

El alcance de negocio es identificar al cliente, crear una oportunidad, registrar lo solicitado, generar una versión de cotización, registrar manualmente envío y aceptación, aplicar la política de anticipo y crear el evento. La consulta posterior debe funcionar por datos del cliente o teléfono sin depender del nombre de un archivo.

El criterio de aceptación del producto exige completar el recorrido con al menos un caso que requiere anticipo y otro municipal. En ambos se debe poder mostrar el cliente, lo ofrecido, el precio registrado, el motivo del cierre y el evento pendiente. Los tests automatizados verifican comportamiento técnico; la validación del equipo con casos reales sigue pendiente de realizarse o acreditarse por separado.

Las rutas y las decisiones implementadas se describen en la [guía del flujo comercial](../flujo-comercial-v1.md). Esta importación de contexto no añade funciones nuevas a la API.

## V1 0.2: consolidación del uso diario

La siguiente entrega prevista busca que el equipo pueda recotizar, retomar o cerrar casos, gestionar sus seguimientos y consultar el historial sin reconstruirlo desde archivos o conversaciones.

| Necesidad de la planificación | Situación documentada en el proyecto |
| --- | --- |
| Conservar varias cotizaciones e identificar enviada y aceptada | El núcleo de versiones, envío y aceptación está implementado. |
| Buscar por cliente, fecha, lugar, servicio y estado | Hay búsqueda por cliente, teléfono y empresa y filtro por cliente; faltan los filtros ampliados y la agenda. |
| Registrar contactos y seguimientos con responsable y fecha | Pendiente. |
| Mostrar seguimientos pendientes y vencidos en el portal | Pendiente. |
| Perder o pausar oportunidades sin borrar historial | Pérdida implementada; pausa y reactivación pendientes. |
| Marcar eventos realizados o cancelados | Implementado en el núcleo actual; la liquidación final sigue pendiente. |
| Validar precios, descuentos y transporte con casos reales | Requiere validación de negocio y resolver los datos incompletos de las notas. |

## Necesidades adicionales documentadas

### Búsqueda y organización de casos

Encontrar una oportunidad o evento por fecha, cliente, teléfono, institución, servicio, lugar o estado. El resultado debe permitir revisar qué se ofreció, el precio, la situación de la reserva y sus versiones de cotización. La ausencia de empresa no debe ocultar al cliente. La agenda debe distinguir eventos próximos y realizados. La búsqueda semántica dentro de documentos es una ampliación futura y no bloquea esta búsqueda estructurada.

### Generación de cotizaciones

Las notas proponen una plantilla que se complete con los datos del caso y mantenga el cálculo de precios separado del documento, permita corregir información variable, conserve versiones y ofrezca descarga en Word cuando se requiera edición externa. También proponen asistencia LLM para generar o regenerar el texto con revisión humana.

La API actual conserva versiones, importes y texto, pero no implementa generación de archivos Word ni integración LLM. Elegir formato de plantilla, proveedor y reglas de edición sigue siendo trabajo de definición e implementación.

### Seguimiento comercial

Registrar contactos indicando fecha, responsable y quién escribió o respondió. Programar, completar o reprogramar tareas sin cambiar automáticamente la etapa comercial; mostrar pendientes y vencidas junto al caso. Una respuesta puede motivar completar o reprogramar una tarea mediante una acción humana.

Los días 1, 3 y 7 son ejemplos de configuración, no una cadencia obligatoria. El chat permanece en WhatsApp y SINOD no detecta automáticamente quién escribió por última vez. Los recordatorios en dispositivos y la detección automática se definen después de validar el seguimiento dentro del portal.

### Medios y Google Drive

Organizar fotos y videos por servicio, permitir previsualizarlos y seleccionarlos para responder a clientes e identificar servicios con material insuficiente o de mala calidad. La propuesta es consumir una carpeta organizada de Google Drive. La solución detallada, los permisos y la integración siguen pendientes. La generación de medios con IA es posterior a resolver la organización básica.

### Iniciativas posteriores

Las notas de V2 nombran integración avanzada con WhatsApp, manejo o evaluación de personal y búsqueda de colaboradores, y publicidad en redes sociales. Esos títulos no contienen requisitos suficientes para implementar. La integración avanzada de WhatsApp, la generación de medios con IA y la búsqueda semántica no bloquean el núcleo comercial ni la consolidación inicial.

## Decisiones que requieren confirmación

- Recuperar el catálogo y el motor previo de precios, si están disponibles, y contrastarlos con cotizaciones reales.
- Confirmar los valores y las fórmulas pendientes de [precios y políticas](precios-y-politicas.md), incluida la tarifa mínima de transporte y los casos de descuentos acumulados.
- Definir quién puede ajustar un precio, qué autorización requiere y cómo se registra su motivo.
- Precisar la evidencia de aceptación y recepción de anticipos. El registro manual actual permite avanzar, pero no equivale a un mecanismo definitivo de comprobantes o autorización.
- Definir qué recursos se agendan, cómo se asignan y cómo se detectan o resuelven conflictos de disponibilidad. La confirmación humana actual no implementa esa agenda.
- Definir saldo final, liquidación del evento y tratamiento operativo de cancelaciones y pagos, sin asumir que los resuelve el registro actual de anticipos.
- Seleccionar APIs e integraciones según los requisitos vigentes cuando se implemente cada función. Las notas no fijan un proveedor de WhatsApp ni resuelven costos, límites, permisos u observabilidad.

Las decisiones nuevas deben quedar versionadas en esta carpeta y, si afectan al comportamiento de la API, reflejarse también en la documentación funcional y en tests de negocio.
