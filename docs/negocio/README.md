# Contexto de negocio de OkiDoki

Esta carpeta reúne el contexto de negocio adicional de SINOD, versionado con el proyecto y disponible en cualquier sistema operativo. Fue elaborado a partir de las notas de OkiDoki en Obsidian consultadas el 29 de octubre de 2026. Para desarrollar o validar cambios, consultar estos documentos directamente; no se requiere acceso al vault original.

## Documentos

| Documento | Para qué consultarlo |
| --- | --- |
| [Contexto empresarial](contexto-empresarial.md) | Actividad de OkiDoki, clientes, servicios, comunicación y propósito de SINOD. |
| [Dominio y flujo comercial](dominio-y-flujo-comercial.md) | Cliente, oportunidad, cotización, evento, historial y condiciones de cierre. |
| [Precios y políticas](precios-y-politicas.md) | Anticipos, saldo, descuentos, recargos, transporte y datos pendientes de confirmar. |
| [Alcance y pendientes](alcance-y-pendientes.md) | Objetivos de las entregas, necesidades futuras y decisiones que todavía requieren validación. |

## Cómo usar y mantener este contexto

- Estas notas describen políticas de negocio y necesidades del producto. Una necesidad documentada no equivale a una funcionalidad implementada.
- El comportamiento actualmente expuesto por la API está descrito en [V1 0.1: recorrido comercial](../flujo-comercial-v1.md). Los tests de [CommercialFlowTests.cs](../../Sinod.Tests/CommercialFlowTests.cs) verifican una parte de ese comportamiento, incluidos el cierre con anticipo, la excepción municipal y la conservación del historial.
- Distinguir siempre entre política de negocio, propuesta pendiente de validar y decisión técnica de la versión actual. No convertir automáticamente una propuesta o un dato incompleto en código ni asumir que el comportamiento actual resuelve todo el alcance de negocio.
- Si aparece una contradicción, documentarla y confirmar el requisito antes de cambiar una regla crítica. No modificar estos documentos para justificar un bug o debilitar un test.
- Cuando se acuerde una regla nueva, actualizar aquí su definición, la documentación funcional afectada y los tests correspondientes. Esta carpeta es la referencia de negocio que deben mantener los agentes dentro del repositorio.

## Procedencia

Las fuentes siguientes son nombres de notas del material original, conservados para trazabilidad. No son rutas necesarias para trabajar con SINOD. Se adaptaron a Markdown estándar y se omitieron metadatos visuales, índices vacíos y material archivado marcado para ignorar.

| Documento del repositorio | Notas de origen |
| --- | --- |
| Contexto empresarial | `Negocio/Contexto Empresarial.md`; `Dominio y Reglas/Contexto del Proyecto.md`. |
| Dominio y flujo comercial | `Flujos.md`; `Alcances.md`; `Dominio y Reglas/Entidades/Cliente.md`, `Oportunidad.md`, `Cotización.md`, `Evento.md`, `Servicio.md`, `Usuario.md` y `Mensaje.md`. |
| Precios y políticas | `Negocio/Contexto Empresarial.md`; `Problemas y Objetivos/V1/Cálculo de precios Fuente de precios, reglas de descuento y cálculo de transporte.md`; `Alcances.md`. |
| Alcance y pendientes | `Versiones/V1 - 0.1 4oct-1nov.md`; `Versiones/V1 - 0.2. 1 nov-1dic.md`; `Alcances.md`; `Riesgos y preguntas.md`; notas de V1 sobre búsqueda, cotizaciones, seguimiento y medios; títulos de iniciativas de V2. |

Salvo `Negocio/Contexto Empresarial.md`, las notas de esta tabla pertenecían a la sección SINOD del material original. Sus nombres permiten entender el origen sin introducir dependencias de carpetas personales, servicios de sincronización o enlaces propios de Obsidian.
