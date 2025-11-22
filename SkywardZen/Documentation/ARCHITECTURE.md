# Skyward Zen - Technical Architecture

## 🏛️ System Design Philosophy

Skyward Zen follows these core principles:

1. **Separation of Concerns**: Each system has a single, well-defined responsibility
2. **Mobile-First Performance**: Object pooling, efficient physics, and URP optimization
3. **Designer-Friendly**: Expose parameters in Inspector for easy tuning
4. **Extensibility**: Base classes and interfaces allow easy addition of new platform types
5. **Clean Code**: Self-documenting with clear naming and inline comments

---

## 📊 System Architecture Diagram

```
┌─────────────────────────────────────────────────┐
│                  Game Scene                      │
├─────────────────────────────────────────────────┤
│                                                  │
│  ┌──────────────┐        ┌──────────────┐      │
│  │   Player     │◄───────│  Camera      │      │
│  │  Movement    │        │ Controller   │      │
│  └──────┬───────┘        └──────────────┘      │
│         │                                        │
│         │ (collision)                            │
│         │                                        │
│  ┌──────▼───────────────────────────────┐      │
│  │        Level Manager                 │      │
│  │  - Procedural Generation             │      │
│  │  - Object Pooling                    │      │
│  │  - Platform Lifecycle                │      │
│  └──────┬───────────────────────────────┘      │
│         │                                        │
│         │ (spawns/pools)                         │
│         │                                        │
│  ┌──────▼───────────────────────────────┐      │
│  │         Platform Hierarchy           │      │
│  │                                       │      │
│  │  ┌────────────┐  ┌────────────┐     │      │
│  │  │  Moving    │  │ Crumbling  │     │      │
│  │  │  Platform  │  │  Platform  │     │      │
│  │  └──────┬─────┘  └──────┬─────┘     │      │
│  │         │                │            │      │
│  │         └────────┬───────┘            │      │
│  │                  │                     │      │
│  │         ┌────────▼─────────┐          │      │
│  │         │  Base Platform   │          │      │
│  │         └──────────────────┘          │      │
│  └───────────────────────────────────────┘      │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## 🎯 Core Systems

### 1. Player System (`Player/`)

**PlayerMovement3D.cs**
- **Responsibility**: Handle player physics, input, and animation
- **Key Features**:
  - Physics-based jumping with custom gravity
  - Touch/tilt input handling
  - Procedural squash & stretch animation
  - Ground detection with raycasting

**WorldWrapper.cs**
- **Responsibility**: Screen wrapping in 3D space
- **Key Features**:
  - Boundary detection
  - Position teleportation with velocity preservation
  - Visual/audio feedback

**Dependencies**:
```
PlayerMovement3D
├── Rigidbody (Unity)
├── AudioSource (Unity)
└── Platforms (via collision)

WorldWrapper
├── Rigidbody (optional)
└── ParticleSystem (optional)
```

---

### 2. Level System (`Level/`)

**LevelManager.cs**
- **Responsibility**: Manage platform lifecycle and procedural generation
- **Key Features**:
  - Spawn platforms based on player Y position
  - Object pooling for performance
  - Platform type distribution
  - Cleanup of off-screen platforms

**ObjectPool.cs** (`Utilities/`)
- **Responsibility**: Generic object pooling implementation
- **Key Features**:
  - Pre-instantiation of objects
  - Dynamic expansion when pool exhausted
  - Return objects to pool instead of destroying

**Data Flow**:
```
1. LevelManager.Update()
   ↓
2. TrackPlayerHeight()
   ↓
3. GeneratePlatformsAhead()
   ↓
4. DeterminePlatformType() (probabilistic)
   ↓
5. ObjectPool.GetObject()
   ↓
6. Platform.Initialize()
   ↓
7. Add to activePlatforms list

Cleanup:
1. CleanupPlatformsBehind()
   ↓
2. Find platforms below threshold
   ↓
3. ObjectPool.ReturnObject()
   ↓
4. Remove from activePlatforms list
```

---

### 3. Camera System (`Camera/`)

**CameraController.cs**
- **Responsibility**: Follow player with smooth, upward-only movement
- **Key Features**:
  - Dead zone to prevent jitter
  - Smooth damping with SmoothDamp
  - Upward-only constraint
  - Configurable offset

**Alternative: Cinemachine**
- See `CinemachineSetupGuide.md` for professional-grade camera
- Custom extension `CinemachineUpwardOnly` for upward lock

**Camera Logic**:
```
1. Track highest player Y position
   ↓
2. Calculate ideal target position
   ↓
3. Check if player is above dead zone
   ↓
   YES → Update target Y
   NO  → Keep current Y
   ↓
4. Ensure target Y >= current Y (never go down)
   ↓
5. SmoothDamp to target position
```

---

### 4. Platform System (`Platforms/`)

**Inheritance Hierarchy**:
```
Platform (base class)
├── MovingPlatform
│   ├── Horizontal movement
│   ├── Circular movement
│   └── Vertical oscillation
└── CrumblingPlatform
    ├── Shake warning
    ├── Dissolve effect
    └── Optional respawn
```

**Platform.cs (Base Class)**
- **Responsibility**: Common platform behavior
- **Key Features**:
  - Collision detection
  - Visual/audio feedback
  - Activation state
  - Reset functionality

**MovingPlatform.cs**
- **Responsibility**: Platforms that move in patterns
- **Movement Patterns**:
  - Horizontal: Sine wave on X axis
  - Circular: Circular motion on X/Z plane
  - Vertical Oscillation: Sine wave on Y axis
- **Player Interaction**: Applies platform velocity to player

**CrumblingPlatform.cs**
- **Responsibility**: Breakable platforms
- **Lifecycle**:
  1. Player lands → Shake warning
  2. Delay → Disable collision
  3. Visual crumble (dissolve or scale)
  4. Deactivate
  5. Optional: Respawn after delay

---

## 🔄 Game Loop

```
┌─────────────────────────────────────────┐
│         Unity Update Cycle              │
└─────────────────────────────────────────┘

Update() [60 times per second]
├── PlayerMovement3D.Update()
│   ├── HandleInput() (touch/tilt)
│   └── ApplySquashAndStretch()
├── LevelManager.Update()
│   ├── TrackPlayerHeight()
│   ├── GeneratePlatformsAhead()
│   └── CleanupPlatformsBehind()
├── CameraController.Update()
│   ├── TrackHighestPoint()
│   └── CalculateTargetWithDeadZone()
└── MovingPlatform.Update() [for each active]
    └── UpdateMovement()

FixedUpdate() [50 times per second - physics]
├── PlayerMovement3D.FixedUpdate()
│   ├── ApplyGravity()
│   ├── ApplyHorizontalMovement()
│   ├── CheckGroundState()
│   └── Auto-jump if landed
└── Physics engine updates

LateUpdate() [After Update]
└── CameraController.LateUpdate()
    └── ApplyCameraPosition() (smooth follow)

OnCollisionEnter() [Event-driven]
└── Platform.OnCollisionEnter()
    └── OnPlayerLanded()
        ├── Visual feedback
        ├── Audio feedback
        └── Platform-specific behavior
```

---

## 🎨 Rendering Pipeline

```
┌─────────────────────────────────────────┐
│    Universal Render Pipeline (URP)     │
└─────────────────────────────────────────┘

1. Culling (Frustum + Occlusion)
   ↓
2. Depth Prepass (URP optimization)
   ↓
3. Main Light Shadow Map
   ↓
4. Forward Rendering
   ├── Opaque objects (platforms)
   ├── Skybox
   └── Transparent objects (crumbling platforms)
   ↓
5. Post-Processing Stack
   ├── Tonemapping
   ├── Bloom
   ├── Color Adjustments
   ├── Vignette
   └── Ambient Occlusion (optional)
   ↓
6. Output to screen
```

---

## 💾 Memory Management

### Object Pooling Strategy

**Why Pool?**
- Avoid garbage collection spikes (causes frame drops)
- Instantiate/Destroy is expensive on mobile
- Predictable memory usage

**Pool Configuration**:
```csharp
// From LevelManager.cs
poolSizePerType = 30; // 30 of each platform type

Total pool size:
- 30 Static Platforms
- 30 Moving Platforms
- 30 Crumbling Platforms
= 90 GameObjects pre-instantiated
```

**Memory Footprint** (approximate):
- Player: ~1 MB
- Camera: ~0.5 MB
- Platform pool: ~10 MB (with materials/meshes)
- Total: **~12 MB** (excluding textures/audio)

### Performance Budget

| System | Target CPU Time | Notes |
|--------|----------------|-------|
| Player Movement | < 1 ms | Physics-heavy |
| Level Manager | < 0.5 ms | Only active when spawning |
| Camera | < 0.1 ms | Very light |
| Platforms | < 0.5 ms | Update only active moving platforms |
| **Total Gameplay** | **< 2.5 ms** | Leaves 14 ms for rendering at 60 FPS |

---

## 🔌 Extension Points

### Adding New Platform Types

1. Create new class inheriting from `Platform.cs`:
```csharp
public class BouncyPlatform : Platform
{
    [SerializeField] private float bounceMultiplier = 2f;

    protected override void OnPlayerLanded(Collision collision)
    {
        base.OnPlayerLanded(collision);

        // Add extra upward force
        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(Vector3.up * bounceMultiplier, ForceMode.Impulse);
        }
    }
}
```

2. Add to `PlatformType` enum in `LevelManager.cs`:
```csharp
public enum PlatformType
{
    Static,
    Moving,
    Crumbling,
    Bouncy  // New!
}
```

3. Add to spawn probabilities:
```csharp
[SerializeField] private float bouncyPlatformChance = 0.1f;
```

4. Add pool and spawn logic in `LevelManager`

### Adding Power-Ups

Create new system:
```csharp
public class PowerUpManager : MonoBehaviour
{
    // Similar structure to LevelManager
    // Spawn power-ups on platforms
    // Player collects and activates effects
}
```

Modify `PlayerMovement3D.cs`:
```csharp
private Dictionary<PowerUpType, float> activePowerUps;

public void ActivatePowerUp(PowerUpType type, float duration)
{
    activePowerUps[type] = Time.time + duration;
}

// In Update(), check and apply active power-ups
```

---

## 🧪 Testing Strategies

### Unit Testing Platform Logic

```csharp
[Test]
public void Platform_WhenPlayerLands_ActivatesOnce()
{
    // Arrange
    var platform = new GameObject().AddComponent<Platform>();
    var player = CreateTestPlayer();

    // Act
    platform.OnCollisionEnter(CreateMockCollision(player));
    platform.OnCollisionEnter(CreateMockCollision(player)); // Second call

    // Assert
    Assert.IsTrue(platform.isActivated);
    // Should only activate once
}
```

### Performance Testing

```csharp
void OnGUI()
{
    GUILayout.Label($"FPS: {1f / Time.deltaTime:F1}");
    GUILayout.Label($"Active Platforms: {activePlatforms.Count}");
    GUILayout.Label($"Pool Available: {platformPool.AvailableCount}");
}
```

### Visual Debugging

All scripts include `OnDrawGizmos()` for visual debugging:
- **PlayerMovement3D**: Ground check raycast (green when grounded)
- **CameraController**: Dead zone visualization
- **WorldWrapper**: Boundary lines
- **MovingPlatform**: Movement path

---

## 📈 Scalability Considerations

### Vertical Scalability (Going Higher)

Current implementation is **infinitely scalable** due to:
1. Cleanup of platforms behind player
2. Object pooling (reuse, not instantiate)
3. Floating origin support (Unity handles large Y values)

**Limitation**: Unity's float precision at Y > 1,000,000
- **Solution**: Shift world down periodically (not implemented)

### Feature Scalability

Easy to add:
- ✅ New platform types (inheritance)
- ✅ Power-ups (new system)
- ✅ Biomes (swap materials/prefabs)
- ✅ Enemies (similar to platforms)

Moderate complexity:
- ⚠️ Multiplayer (needs networking)
- ⚠️ Procedural mesh generation (for unique platforms)

---

## 🎓 Design Patterns Used

1. **Object Pool Pattern**: `ObjectPool.cs`
2. **Template Method**: `Platform.cs` base class with virtual methods
3. **Component Pattern**: Unity's component-based architecture
4. **Observer Pattern**: Unity's event system (collision callbacks)
5. **Singleton** (not used): Intentionally avoided for better testability

---

## 📚 Further Reading

- [Unity Component Best Practices](https://docs.unity3d.com/Manual/class-MonoBehaviour.html)
- [URP Architecture](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Mobile Optimization Guide](https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html)

---

**This architecture balances simplicity, performance, and extensibility for a modern mobile game.**
