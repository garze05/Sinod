# Instrucciones para agentes de IA

Estas reglas aplican a todo el repositorio y a cualquier agente de IA que modifique el proyecto.

## Documentación obligatoria

- Actualizar `README.md` en cada cambio realizado por un agente, reflejando el resultado y manteniendo sincronizadas las capacidades, requisitos y formas de uso del proyecto.
- Actualizar también la documentación afectada en `docs/`, los ejemplos HTTP y los contratos documentados cuando corresponda. Revisar especialmente `docs/flujo-comercial-v1.md`, `docs/desarrollo-local.md` y el contexto de negocio en `docs/negocio/`.
- Describir el comportamiento final implementado: rutas, reglas de negocio, validaciones, configuración, migraciones y limitaciones que cambien. Corregir o retirar instrucciones que hayan quedado obsoletas.
- Hacer actualizaciones útiles y proporcionales al cambio. No cumplir esta regla agregando texto vacío, fechas o afirmaciones que no reflejen el comportamiento real.

## Pruebas que representan el negocio

- Crear o actualizar tests en `Sinod.Tests` para toda funcionalidad crítica nueva o modificada y para las reglas de negocio afectadas.
- Para un bug, añadir o ajustar una prueba de regresión que represente el caso real y distinga el comportamiento incorrecto del esperado. Cuando sea posible, comprobar que falla antes de aplicar la corrección.
- Probar resultados observables del negocio: importes, límites, estados, transiciones, historial, errores y persistencia, según corresponda. Incluir casos límite y rechazos relevantes; no limitarse a comprobar que un método se ejecuta.
- En funcionalidades HTTP y de persistencia, mantener pruebas de integración sobre la API y PostgreSQL real cuando esas capas formen parte del comportamiento que se valida. El fixture de `CommercialFlowTests` usa Testcontainers en local o `SINOD_TEST_POSTGRES` en CI.
- Los mocks pueden aislar dependencias externas cuando sea apropiado, pero no deben sustituir la regla de negocio ni la persistencia que el test pretende verificar.
- No eludir fallos deshabilitando u omitiendo tests, excluyéndolos mediante filtros, eliminando aserciones necesarias, debilitando expectativas o cambiando reglas de negocio únicamente para obtener una suite verde. Solo modificar expectativas si cambia legítimamente el requisito, y reflejar ese cambio en la documentación.
- Los cambios exclusivamente de documentación o configuración de agentes no requieren tests artificiales que repliquen texto o detalles de implementación. Revisar su coherencia y ejecutar igualmente la suite existente.

## Contexto de negocio

- Consultar primero el README, `docs/flujo-comercial-v1.md` y los tests existentes para entender el comportamiento implementado.
- Usar [docs/negocio/README.md](docs/negocio/README.md) como entrada al contexto de negocio adicional versionado: [contexto empresarial](docs/negocio/contexto-empresarial.md), [dominio y flujo comercial](docs/negocio/dominio-y-flujo-comercial.md), [precios y políticas](docs/negocio/precios-y-politicas.md) y [alcance y pendientes](docs/negocio/alcance-y-pendientes.md).
- Distinguir reglas confirmadas, propuestas pendientes de validar y comportamiento implementado. Las notas de negocio no prueban que una funcionalidad ya exista; contrastarlas con la documentación funcional y los tests.
- No inventar reglas si falta contexto o existen contradicciones. Contrastar la información disponible y pedir aclaración cuando sea necesaria para definir el resultado esperado. Incorporar las decisiones pertinentes a la documentación del repositorio y a los tests.

## Criterio de finalización

Una funcionalidad o corrección de bug no se considera implementada ni arreglada hasta que se cumplan todas estas condiciones:

1. El código cumple el comportamiento esperado del negocio.
2. `README.md` y toda la documentación afectada están actualizados y son coherentes con el código.
3. Los tests relevantes están creados o actualizados en `Sinod.Tests`.
4. La suite completa pasa, sin omisiones ni atajos que oculten fallos:

   ```bash
   dotnet test Sinod.Tests/Sinod.Tests.csproj
   ```

En local se necesita Docker iniciado para Testcontainers. Si se usa `SINOD_TEST_POSTGRES`, el servidor indicado debe estar disponible y permitir crear y eliminar bases temporales. No hace falta iniciar la API ni aplicar migraciones manualmente para estos tests.

Si no es posible ejecutar los tests o estos fallan, informar el bloqueo o fallo y dejar explícito que la validación está pendiente; no declarar el trabajo terminado. En la entrega, resumir qué cambió, qué documentación se actualizó y qué pruebas se ejecutaron con su resultado real.
