# BEACON

Prototype de movimiento y combate 2D en Unity (URP 2023.3+). Incluye sistemas de movimiento avanzado (coyote, buffer, dash con Perfect Dodge), combate híbrido (melee/ranged), parry y feedback (hitstop, shake).

## Requisitos
- Unity 6 (2023.3+) con URP 2D.
- Input System habilitado en modo Both.

## Estructura rápida
- `Assets/_Project/Scripts/Player`: controlador, FSM de estados y datos de movimiento.
- `Assets/_Project/Scripts/Combat`: controlador de combate, combos/armas, parry y feedback.
- `Assets/_Project/Scripts/Core`: salud, resonance y utilidades.
- `Assets/_Project/Documentation`: `SetupGuide.md` y `TweakReference.md`.

## Pruebas
Se usa Unity Test Framework con dos suites:
- `Assets/Tests/EditMode` para lógica pura (ej. cálculos de datos).
- `Assets/Tests/PlayMode` para flujo in-engine (dash/parry, interacción FSM).

Para ejecutar: `Window > General > Test Runner` en Unity y correr EditMode/PlayMode.

## CI (GitHub Actions)
Workflow en `.github/workflows/unity.yml`:
- Cache de librerías de Unity.
- Ejecuta EditMode y PlayMode tests headless.

## Cómo clonar/abrir
```bash
git clone <repo-url>
cd BEACON
```
Abrir con Unity Hub apuntando a la carpeta raíz.

## Licencia
Pendiente (añadir MIT/Apache/propietaria según necesidad).
