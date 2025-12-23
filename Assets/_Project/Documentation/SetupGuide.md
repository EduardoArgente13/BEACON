# BEACON - Guía de Setup del Sistema de Movimiento

## Requisitos Previos

1. **Unity 6** (2023.3+) con URP 2D
2. **Input System Package** instalado (Window > Package Manager > Input System)
3. Cuando instales Input System, Unity te preguntará si quieres habilitar el nuevo backend. **Selecciona "Both"** para compatibilidad.

---

## Setup Paso a Paso

### 1. Importar Scripts

Los scripts ya están organizados en:
```
Assets/_Project/Scripts/
├── Core/
│   └── Singleton.cs
├── Player/
│   ├── PlayerController.cs
│   ├── PlayerInputHandler.cs
│   ├── PlayerInputActions.cs
│   ├── GroundChecker.cs
│   ├── WallChecker.cs
│   ├── Data/
│   │   └── PlayerMovementData.cs
│   └── States/
│       ├── IPlayerState.cs
│       ├── PlayerStateMachine.cs
│       ├── PlayerStateBase.cs
│       ├── GroundedState.cs
│       ├── AirborneState.cs
│       ├── WallSlideState.cs
│       └── DashState.cs
└── Managers/
    ├── TimeManager.cs
    └── GameFeedback.cs
```

### 2. Crear ScriptableObject de Datos

1. En Project window: `Right Click > Create > BEACON > Player > Movement Data`
2. Nombrar: `DefaultMovementData`
3. Guardar en `Assets/_Project/Data/`
4. Ajustar valores en Inspector (los defaults son buenos para empezar)

### 3. Configurar Layers

1. **Edit > Project Settings > Tags and Layers**
2. Crear layers:
   - `Ground` (Layer 6)
   - `Wall` (Layer 7)
   - `Player` (Layer 8)
   - `Enemy` (Layer 9)

3. **Edit > Project Settings > Physics 2D**
4. En Collision Matrix, configurar qué layers colisionan

### 4. Crear Player Prefab

1. Crear GameObject vacío, nombrar "Player"
2. Agregar componentes:
   - `Rigidbody2D`
     - Body Type: Dynamic
     - Collision Detection: Continuous
     - Interpolate: Interpolate
     - Freeze Rotation: ✓
   - `CapsuleCollider2D` (o BoxCollider2D): Ajustar tamaño
   - `PlayerController`
   - `PlayerInputHandler`
   - `GroundChecker`
   - `WallChecker` (opcional)

### 5. Configurar GroundChecker

1. Crear empty child: "FeetPosition"
2. Posicionarlo en la base del player
3. En `GroundChecker`:
   - Asignar FeetPosition como "Feet Position"
   - Asignar Ground Layer al "Ground Layer"
   - Ajustar raycast distance (~0.1)

### 6. Configurar WallChecker (Opcional)

1. Crear empty children: "WallCheck_Upper" y "WallCheck_Lower"
2. Posicionar a la altura del torso y rodillas
3. En `WallChecker`:
   - Asignar ambos transforms
   - Asignar Wall Layer

### 7. Asignar Referencias en PlayerController

En el Inspector del PlayerController:
- Movement Data: Arrastrar `DefaultMovementData`
- Los demás componentes se auto-asignan si están en el mismo GameObject

### 8. Crear Escena de Test

1. Crear suelo con BoxCollider2D en layer "Ground"
2. Crear plataformas flotantes
3. Crear paredes para test de wall slide
4. Agregar el Player prefab
5. Agregar cámara que siga al player (Cinemachine recomendado)

### 9. Managers de Feedback

1. Crear GameObject vacío "GameManagers"
2. Agregar `TimeManager` 
3. Agregar `GameFeedback`
4. Asignar Main Camera al GameFeedback (o dejar que la detecte automáticamente)

---

## Input Bindings

### Keyboard + Mouse
| Acción | Tecla |
|--------|-------|
| Mover | WASD / Flechas |
| Saltar | Space |
| Dash | Left Shift |
| Atacar | Click Izquierdo / J |
| Parry | Click Derecho / K |
| Skill | L |
| Interactuar | E |
| Pausa | Escape |

### Gamepad
| Acción | Botón |
|--------|-------|
| Mover | Left Stick |
| Saltar | A (South) |
| Dash | RT |
| Atacar | X (West) |
| Parry | LB |
| Skill | Y (North) |
| Interactuar | B (East) |
| Pausa | Start |

---

## Testing del Sistema

### 1. Movimiento Horizontal
- [ ] El player acelera suavemente, no es instantáneo
- [ ] El player desacelera suavemente al soltar
- [ ] El sprite flipea al cambiar dirección
- [ ] El control en aire es notablemente menor que en suelo

### 2. Sistema de Salto
- [ ] **Tap rápido** = salto corto
- [ ] **Hold largo** = salto alto (variable jump)
- [ ] **Coyote Time**: Puedes saltar ~0.1s después de caer de plataforma
- [ ] **Jump Buffer**: Presionar salto justo antes de aterrizar lo ejecuta al tocar suelo
- [ ] **Apex Hang**: Se siente "flotante" en el punto más alto

### 3. Sistema de Dash
- [ ] Dash funciona en 8 direcciones
- [ ] Dash sin input direccional = dash en dirección que miras
- [ ] Solo 1 dash aéreo hasta tocar suelo
- [ ] Cooldown previene spam
- [ ] i-frames: No tomas daño durante dash

### 4. Wall Mechanics
- [ ] Wall slide al presionar hacia una pared mientras caes
- [ ] Velocidad de caída reducida durante wall slide
- [ ] Wall jump te impulsa lejos de la pared
- [ ] Wall jump resetea air dash

### 5. Debug Info
- En Play Mode, verás info de debug en esquina superior izquierda:
  - Estado actual
  - Grounded status
  - Velocidad
  - Air dashes restantes
  - Timers de coyote/buffer

---

## Tweaking de Game Feel

### Para movimiento más "Tight" (Hollow Knight style)
```
Acceleration: 70-100
Deceleration: 70-100
Air Acceleration: 40-50
```

### Para movimiento más "Floaty" (Ori style)
```
Acceleration: 30-40
Deceleration: 25-35
Air Acceleration: 20-25
Fall Gravity Multiplier: 1.2-1.4
```

### Para salto más "Snappy"
```
Jump Force: 20-22
Jump Cut Multiplier: 0.4
Fall Gravity Multiplier: 2.0-2.5
```

### Para dash más "Punchy"
```
Dash Speed: 30-35
Dash Duration: 0.1-0.12
Dash End Speed Multiplier: 0.3
```

---

## Troubleshooting

### El player no detecta suelo
- Verificar que el suelo tiene BoxCollider2D
- Verificar que el suelo está en layer "Ground"
- Verificar que Ground Layer está asignado en GroundChecker
- Revisar Gizmos en Scene view para ver raycasts

### El player resbala en slopes
- Verificar `Max Slope Angle` en MovementData
- Si el slope es muy empinado, el player no podrá quedarse quieto ahí

### Input no responde
- Verificar que Input System package está instalado
- Verificar en Project Settings > Player > Active Input Handling = "Both" o "Input System"
- Verificar que PlayerInputHandler está enabled

### El dash no funciona
- Verificar cooldown (puede ser muy alto)
- Verificar air dashes remaining (debug info)
- Verificar que MovementData está asignado

### Wall slide no activa
- Verificar que WallChecker tiene sus transforms asignados
- Verificar que las paredes están en layer "Wall"
- Verificar que estás presionando hacia la pared mientras caes

---

## Próximos Pasos

Una vez que el movimiento se sienta bien, proceder a:

1. **Sistema de Combate** - Ataques, combos, hitboxes
2. **Sistema de Parry** - Ventana precisa de deflect
3. **Sistema de Resonance** - Recurso para habilidades especiales
4. **Enemigos básicos** - FSM enemigo, comportamientos
5. **Boss** - FSM complejo con fases

¡El movimiento es la base de todo! Tómate tiempo twekeando hasta que se sienta PERFECTO.
