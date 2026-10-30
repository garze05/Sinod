# Precios y políticas comerciales

Fuente: «Contexto Empresarial», nota de cálculo de precios y decisiones de alcance de SINOD. Ver la [procedencia y los criterios de uso](README.md). Los valores de referencia de las notas necesitan validación con el negocio antes de automatizar nuevas reglas o cargar tarifas de producción.

## Reserva y pagos

- Para particulares, empresas, escuelas, colegios, universidades y cualquier otro cliente distinto de Municipalidad, la reserva exige un anticipo del 50%.
- Según la política empresarial, ese anticipo es no reembolsable y el saldo final se paga el día del evento antes de iniciar el show.
- Municipalidad es la única excepción: confirma sin anticipo y puede pagar después del evento según el proceso municipal correspondiente. Esa excepción no se extiende al resto de instituciones.
- La aceptación de una versión y la confirmación de disponibilidad siguen siendo necesarias en ambos casos.

La API actual registra anticipos recibidos y comprueba el mínimo para cerrar una oportunidad. No implementa el cobro del saldo final, una conciliación completa ni un flujo de reembolsos. La política de anticipo no reembolsable no debe interpretarse como un proceso de cancelación ya resuelto en el sistema.

## Catálogo y precios base

El negocio necesita una fuente central para precios, duración mínima o base, recargos y descuentos. Las notas señalan la necesidad de recuperar o reconstruir el catálogo y contrastarlo con cotizaciones reales, usando el motor previo de Python como referencia si está disponible.

El contexto empresarial menciona como referencia un paquete desde ₡60.000 de una hora de animación con botarga, equipo de sonido y show, y lo marca dentro de una estructura comercial en proceso. Ese ejemplo no es un precio universal ni un dato que deba insertarse automáticamente en el catálogo.

Personajes alquilados o especiales e inflables de distinto tamaño pueden requerir precios diferentes. En la API actual, el servicio usa un perfil con precio y duración base y puede tener un precio específico que prevalece sobre el del perfil.

## Descuentos propuestos en las notas

| Condición | Referencia de negocio |
| --- | --- |
| Contratar más de un servicio del mismo tipo | 15%. |
| Contratar una hora adicional | 14%. |
| Descuento acumulado máximo | 30%. |

Antes de automatizar, confirmar qué partidas califican, cómo se trata la duración adicional, cómo se combinan los descuentos y en qué orden se aplican. Las notas no definen una fórmula completa de acumulación.

En V1 0.1, el operador ingresa un descuento por línea entre 0% y 30%; no se detectan automáticamente las condiciones del 15% y del 14%. El precio por duración usa el prorrateo documentado en la [guía técnica](../flujo-comercial-v1.md#cálculo-explícito-de-v1-01), que debe validarse con casos reales antes de ampliarlo a las reglas propuestas.

## Recargos de referencia

| Tipo de cliente en las notas | Recargo propuesto |
| --- | --- |
| Escolar / colegio | 5%. |
| Universidad | 6%. |
| Corporativo / empresa | 10%. |
| Centro comercial | 10%. |
| Agencia de publicidad | 15%. |
| Sin tipo de cliente asignado | 0%. |

Estos porcentajes no se cargan ni se seleccionan automáticamente por tipo en la API actual. `Company.BaseRate` se configura como porcentaje y se conserva en la cotización. El tipo de organización y el recargo son datos distintos; elegir un tipo no asigna por sí solo una tarifa.

El modelo actual tampoco representa todos los tipos mencionados en las notas, como corporativo o agencia de publicidad. No deducir valores para municipalidades, asociaciones u otros tipos ausentes de esta tabla; confirmar sus tarifas con el negocio.

## Transporte propuesto

- Origen de referencia: Ulloa, Heredia, Costa Rica.
- La propuesta contempla calcular distancia por Google Maps y cobrar únicamente el trayecto de ida, sin considerar tráfico.
- Radio de referencia: 15 km; después, ₡400 por kilómetro adicional.
- Si falla el cálculo de distancia, se debe advertir y ofrecer una tarifa base por provincia.

**Datos incompletos:** la nota original escribe la tarifa mínima como «.000 colones», sin un importe utilizable. Tampoco define los importes alternativos por provincia. No interpretar ese texto como ₡0 ni completar esos valores por suposición. Confirmar además cómo se redondean los kilómetros y cómo se combina la tarifa mínima con el cargo adicional.

La API actual recibe un importe de transporte ingresado por el operador; no usa Google Maps ni aplica automáticamente el radio, la tarifa por kilómetro o la alternativa provincial.

## Cálculo y criterios de validación actuales

La moneda del cálculo actual es CRC. La implementación usa precio por cantidad y proporción de duración, aplica descuento por línea, suma el recargo sobre las líneas descontadas y añade transporte. Redondea importes a dos decimales y exige al menos la duración base. La fórmula exacta, sus decisiones y límites se mantienen en la [guía técnica del flujo](../flujo-comercial-v1.md#cálculo-explícito-de-v1-01).

Para ampliar el motor se necesitan cotizaciones reales y resultados esperados acordados con el negocio: importes por servicio, duración adicional, combinación de descuentos, recargo, transporte, total y anticipo exigido. Un test debe comprobar esos resultados, no reproducir mecánicamente la fórmula del código. Las facultades para ajustar precios manualmente y el registro de motivos de excepción siguen pendientes de definición.
