# CaCuSibAutoPlan - Embedded Progress

Esta versión elimina la ventana emergente de progreso y mantiene la barra únicamente dentro de la interfaz principal.

Notas importantes:
- ESAPI se ejecuta en el hilo original de Eclipse para evitar el error `AtomicAccess.cpp`.
- `OptimizeVMAT()` y `CalculateDose()` son llamadas síncronas; durante esas llamadas largas Eclipse/WPF puede quedar ocupado.
- La barra se actualiza por etapas antes y después de cada módulo, pero ESAPI no expone porcentaje interno del optimizador.
