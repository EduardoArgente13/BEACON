# BEACON - Tweak Values Reference

Referencia rápida de todos los valores tweakables y su impacto en el game feel.

---

## Movimiento Horizontal

| Parámetro | Rango | Default | Efecto |
|-----------|-------|---------|--------|
| `moveSpeed` | 1-20 | 8 | Velocidad máxima. Más alto = más rápido |
| `groundAcceleration` | 10-100 | 50 | Qué tan rápido llega a max speed en suelo |
| `groundDeceleration` | 10-100 | 50 | Qué tan rápido se detiene en suelo |
| `airAcceleration` | 5-100 | 35 | Control en aire (menos = más realista) |
| `airDeceleration` | 5-50 | 20 | Frenado en aire |

### Presets de referencia:

**Hollow Knight (Tight)**
- Speed: 8.7
- Ground Accel: 80
- Ground Decel: 80
- Air Accel: 45
- Air Decel: 30

**Celeste (Ultra-responsive)**
- Speed: 9
- Ground Accel: 100
- Ground Decel: 100
- Air Accel: 65
- Air Decel: 40

**Ori (Floaty)**
- Speed: 10
- Ground Accel: 40
- Ground Decel: 35
- Air Accel: 25
- Air Decel: 15

---

## Sistema de Salto

| Parámetro | Rango | Default | Efecto |
|-----------|-------|---------|--------|
| `jumpForce` | 10-30 | 18 | Altura del salto |
| `jumpCutMultiplier` | 0.1-1 | 0.5 | Variable height (menor = más control) |
| `fallGravityMultiplier` | 1-3 | 1.8 | Qué tan rápido cae |
| `maxFallSpeed` | 10-50 | 25 | Terminal velocity |
| `apexThreshold` | 0.5-5 | 2.5 | Rango de velocidad Y para apex |
| `apexSpeedBonus` | 0-1 | 0.5 | Boost de control en apex |
| `apexGravityReduction` | 0-1 | 0.4 | Hang time en apex |

### Tips:
- **jumpCutMultiplier bajo (0.3-0.4)** = Gran diferencia entre tap y hold
- **fallGravityMultiplier alto (2.0+)** = Salto snappy, caída rápida
- **apexThreshold alto** = Más tiempo en "zona de hang"

---

## Jump Assists

| Parámetro | Rango | Default | Descripción |
|-----------|-------|---------|-------------|
| `coyoteTime` | 0.05-0.3 | 0.12 | Tiempo para saltar después de salir de plataforma |
| `jumpBufferTime` | 0.05-0.3 | 0.1 | Tiempo que se recuerda input de salto |

### Nota importante:
- Valores **muy altos** (>0.2) pueden sentirse "sloppy"
- Valores **muy bajos** (<0.08) pueden frustrar al jugador
- Rango óptimo: **0.1-0.15** para ambos

---

## Sistema de Dash

| Parámetro | Rango | Default | Efecto |
|-----------|-------|---------|--------|
| `dashSpeed` | 15-40 | 25 | Velocidad durante dash |
| `dashDuration` | 0.1-0.4 | 0.15 | Duración del dash |
| `dashCooldown` | 0.1-2 | 0.4 | Tiempo entre dashes |
| `dashIFramesDuration` | 0.05-0.3 | 0.12 | Invincibilidad durante dash |
| `airDashesAllowed` | 0-3 | 1 | Dashes aéreos |
| `dashEndSpeedMultiplier` | 0-1 | 0.5 | Velocidad residual post-dash |

### Trade-offs:
- **Dash corto + rápido** = Combat-focused (Hollow Knight)
- **Dash largo + i-frames extendidos** = Traversal-focused (Dead Cells)
- **dashEndSpeedMultiplier bajo** = Más control post-dash
- **dashEndSpeedMultiplier alto** = Momentum preservation

---

## Wall Mechanics

| Parámetro | Rango | Default | Efecto |
|-----------|-------|---------|--------|
| `wallSlideSpeed` | 1-10 | 3 | Velocidad de caída en pared |
| `wallJumpForceX` | 5-20 | 12 | Impulso horizontal del wall jump |
| `wallJumpForceY` | 10-25 | 16 | Impulso vertical del wall jump |
| `wallJumpLockTime` | 0.1-0.5 | 0.2 | Tiempo de control reducido post-wall-jump |

### Consideraciones:
- **wallSlideSpeed muy bajo** = Puede sentirse "pegajoso"
- **wallJumpForceX alto** = Aleja mucho de la pared (1 wall, muchas opciones)
- **wallJumpForceX bajo** = Permite escalar paredes estrechas

---

## Ground Detection

| Parámetro | Rango | Default | Uso |
|-----------|-------|---------|-----|
| `groundCheckDistance` | 0.05-0.5 | 0.1 | Distancia de raycast |
| `groundCheckWidth` | 0.1-1 | 0.4 | Separación de raycasts laterales |
| `maxSlopeAngle` | 20-60 | 45 | Ángulo máximo de slopes caminables |

---

## Gravity Settings

| Parámetro | Rango | Default | Descripción |
|-----------|-------|---------|-------------|
| `defaultGravityScale` | 1-5 | 3 | Rigidbody2D gravity scale base |
| `holdJumpGravityScale` | 0.5-3 | 2 | Gravedad mientras mantiene salto |

### Formula típica:
```
Fall = defaultGravityScale * fallGravityMultiplier
Rise (holding) = holdJumpGravityScale  
Rise (released) = defaultGravityScale * fallGravityMultiplier (jump cut)
```

---

## Quick Test Workflow

1. **Ajustar moveSpeed** hasta que la velocidad base se sienta bien
2. **Ajustar jumpForce** para altura correcta
3. **Ajustar fallGravityMultiplier** para snappiness de caída
4. **Fine-tune aceleración** para responsiveness
5. **Ajustar coyote/buffer** hasta que se sienta "fair"
6. **Ajustar dash values** para combat flow
7. **Repeat** hasta la perfección

---

## Valores de Hollow Knight (Reference)

Estos son valores estimados basados en análisis de gameplay:

```
moveSpeed: 8.7
groundAcceleration: 85
groundDeceleration: 85
airAcceleration: 45
jumpForce: 19
jumpCutMultiplier: 0.5
fallGravityMultiplier: 2.0
coyoteTime: 0.1
jumpBufferTime: 0.1
dashSpeed: 28
dashDuration: 0.12
dashCooldown: 0.5
```

---

## Debug Commands

En el PlayerController hay debug GUI que muestra:
- Estado actual de la FSM
- Ground status
- Velocidad actual
- Air dashes restantes
- Coyote timer
- Jump buffer timer
- Invincibility status

Para ver los Gizmos de GroundChecker y WallChecker:
1. Seleccionar el Player en Hierarchy
2. En Scene view, asegurarse de que Gizmos está activado
3. Los raycasts se dibujan en verde (grounded) o rojo (airborne)
