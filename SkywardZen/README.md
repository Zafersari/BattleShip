# 🎮 Skyward Zen - Modern 3D Vertical Platformer

A hyper-polished, modern 3D reimagining of the classic "Doodle Jump" mechanics, built with Unity 2023+ and Universal Render Pipeline (URP).

![Unity Version](https://img.shields.io/badge/Unity-2023.1%2B-blue)
![URP](https://img.shields.io/badge/URP-14.0%2B-green)
![Platform](https://img.shields.io/badge/Platform-Mobile-orange)
![Target FPS](https://img.shields.io/badge/Target%20FPS-60-red)

---

## 🎯 Project Vision

**Skyward Zen** transforms the classic vertical platformer into a **premium 3D experience** featuring:

- 🎨 **Modern Aesthetic**: Soft 3D minimalism with pastel colors and matte materials
- 🎪 **Satisfying Physics**: Squash & stretch animations with weighty, responsive controls
- 🌟 **Visual Polish**: URP post-processing with bloom, ambient occlusion, and color grading
- 📱 **Mobile-First**: Optimized for 60 FPS on mobile devices with object pooling
- 🎬 **Cinematic Camera**: Smooth tracking with Cinemachine integration

---

## 🏗️ Architecture Overview

### Core Systems

```
SkywardZen/
├── Player/
│   ├── PlayerMovement3D.cs      ← Physics-based movement + squash/stretch
│   └── WorldWrapper.cs           ← Screen wrapping in 3D space
├── Level/
│   └── LevelManager.cs           ← Procedural generation + object pooling
├── Camera/
│   ├── CameraController.cs       ← Upward-only tracking with dead zone
│   └── CinemachineSetupGuide.md  ← Professional camera setup
├── Platforms/
│   ├── Platform.cs               ← Base platform class
│   ├── MovingPlatform.cs         ← Horizontal/circular movement
│   └── CrumblingPlatform.cs      ← Glass-like breakable platforms
└── Utilities/
    └── ObjectPool.cs             ← Generic pooling system
```

---

## ✨ Key Features

### 🎮 Player Movement
- **Touch/Drag Controls**: Slide to move horizontally
- **Tilt Controls**: Accelerometer-based alternative (toggleable)
- **Auto-Jump**: Automatically jump when landing on platforms
- **Squash & Stretch**: Procedural animation based on velocity
  - Stretches upward during jump
  - Squashes on landing
  - Smooth interpolation for natural feel

### 🏔️ Procedural Level Generation
- **Infinite Vertical Progression**: Platforms generate ahead, despawn behind
- **Object Pooling**: Reuse platforms for 60 FPS performance
- **Platform Variety**:
  - **Static**: Standard platforms (70% spawn rate)
  - **Moving**: Horizontal/circular motion (20% spawn rate)
  - **Crumbling**: Glass-like breakable platforms (10% spawn rate)

### 📷 Camera System
- **Upward-Only Tracking**: Never moves down
- **Dead Zone**: Prevents jitter from small movements
- **Smooth Damping**: Silky camera follow
- **Cinemachine Ready**: Professional-grade camera control option

### 🎨 Visual Style
- **Soft 3D Minimalism**: Geometric shapes with matte materials
- **Pastel Palette**: Soft pinks, cyans, sunset oranges
- **Real-time Lighting**: Soft shadows with ambient occlusion
- **Post-Processing**: Bloom, vignette, color grading for "Instagram-worthy" look

---

## 🚀 Quick Start

### 1. Prerequisites
- Unity 2023.1+ (LTS recommended)
- Basic Unity knowledge
- Mobile device for testing (or simulator)

### 2. Setup Steps

1. **Create New Project**:
   ```
   Unity Hub > New Project > 3D (URP) template
   ```

2. **Import Scripts**:
   - Copy all scripts from `/Assets/Scripts/` to your project
   - Wait for compilation

3. **Follow Setup Guide**:
   - Read `Documentation/PROJECT_SETUP.md` for detailed scene setup
   - Configure URP post-processing using `Documentation/URP_POSTPROCESSING_GUIDE.md`

4. **Build & Test**:
   - Create player, camera, and platform prefabs
   - Assign references in LevelManager
   - Hit Play!

---

## 📖 Documentation

| Document | Description |
|----------|-------------|
| **[PROJECT_SETUP.md](Documentation/PROJECT_SETUP.md)** | Complete scene setup and configuration guide |
| **[URP_POSTPROCESSING_GUIDE.md](Documentation/URP_POSTPROCESSING_GUIDE.md)** | Post-processing settings for "high-end mobile" look |
| **[CinemachineSetupGuide.md](Assets/Scripts/Camera/CinemachineSetupGuide.md)** | Professional camera setup with Cinemachine |

---

## 🎓 Code Highlights

### Squash & Stretch Animation

The heart of the modern feel - procedural animation based on physics:

```csharp
// From PlayerMovement3D.cs:107-142
private void ApplySquashAndStretch()
{
    float verticalVelocity = rb.velocity.y;

    if (verticalVelocity > 1f)
    {
        // Stretch while going up
        targetSquashStretch = stretchAmount;
    }
    else if (verticalVelocity < -1f)
    {
        // Squash while falling
        targetSquashStretch = -squashAmount;
    }

    // Smooth interpolation
    currentSquashStretch = Mathf.Lerp(
        currentSquashStretch,
        targetSquashStretch,
        squashStretchSpeed * Time.deltaTime
    );

    // Apply to scale (Y stretches, X/Z squash to preserve volume)
    transform.localScale = new Vector3(
        baseScale.x * xzScale,
        baseScale.y * yScale,
        baseScale.z * xzScale
    );
}
```

### Object Pooling for Performance

Efficient platform reuse for mobile 60 FPS:

```csharp
// From ObjectPool.cs:22-35
public GameObject GetObject()
{
    if (availableObjects.Count > 0)
    {
        return availableObjects.Dequeue();
    }
    else
    {
        // Pool exhausted - expand dynamically
        return CreateNewObject();
    }
}
```

---

## 🎨 Visual Style Reference

### Color Palette

```
Pastel Pink:    #FFB6C1
Soft Cyan:      #87CEEB
Sunset Orange:  #FFDAB9
Alice Blue:     #F0F8FF
Lavender:       #E6E6FA
```

### Material Settings

**Static Platform**:
- Shader: URP/Lit
- Metallic: 0
- Smoothness: 0.3-0.5

**Moving Platform**:
- Shader: URP/Lit
- Emission: Enabled (subtle glow)
- Smoothness: 0.6

**Crumbling Platform**:
- Shader: URP/Lit
- Rendering Mode: Transparent
- Smoothness: 0.8 (glass-like)

---

## 📱 Performance Targets

| Device Tier | Target FPS | MSAA | Post-Processing |
|-------------|-----------|------|-----------------|
| High-End (iPhone 12+) | 60 | 4x | All effects |
| Mid-Range (iPhone 11) | 60 | 2x | Bloom + Color |
| Low-End (Budget) | 60 | Off | Color only |

**Optimization Techniques:**
- ✅ Object pooling (no runtime instantiation)
- ✅ Efficient collision detection (layers)
- ✅ Minimal draw calls (<100)
- ✅ URP mobile-optimized shaders
- ✅ Texture atlasing for platforms

---

## 🛠️ Customization Guide

### Adjusting Difficulty

In `LevelManager.cs`:
```csharp
platformSpacing = 3f;        // Decrease for harder (2.5f)
horizontalSpawnRange = 8f;   // Increase for harder (10f)
```

### Changing Platform Distribution

```csharp
staticPlatformChance = 0.7f;     // More static = easier
movingPlatformChance = 0.2f;     // More moving = harder
crumblingPlatformChance = 0.1f;  // More crumbling = harder
```

### Tuning Jump Feel

In `PlayerMovement3D.cs`:
```csharp
jumpForce = 15f;    // Higher = jump higher
gravity = -25f;     // More negative = fall faster
```

---

## 🎯 Roadmap

- [ ] Game state management (start, game over, restart)
- [ ] Score system with persistent high scores
- [ ] Power-ups (double jump, slow motion, magnet)
- [ ] Multiple biomes with different color palettes
- [ ] Leaderboards (online multiplayer)
- [ ] Sound design and music integration
- [ ] Particle effects for platform interaction
- [ ] Tutorial system for first-time players

---

## 🤝 Contributing

This is a prototype/reference project. Feel free to:
- Use the code in your own projects
- Modify and extend the systems
- Share improvements and optimizations

---

## 📜 License

This project is provided as-is for educational and reference purposes.

---

## 🙏 Acknowledgments

**Inspiration:**
- Doodle Jump (original gameplay)
- Monument Valley (visual aesthetic)
- Alto's Adventure (polish and feel)

**Unity Technologies:**
- Universal Render Pipeline
- Cinemachine
- Physics system

---

## 📧 Support

For questions about the architecture or implementation:
- Review inline code comments (all scripts are heavily documented)
- Check `Documentation/` folder for guides
- Examine Debug visualizations (Gizmos in Scene view)

---

**Built with ❤️ for modern mobile gaming**

*"Reimagining classics with contemporary polish"*
