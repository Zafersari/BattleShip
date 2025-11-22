# Skyward Zen - Project Setup Guide

## Overview
Skyward Zen is a modern 3D reimagining of the classic vertical platformer, built with Unity 2023+ and URP (Universal Render Pipeline). This guide will help you set up the project from scratch.

---

## 🎯 Project Requirements

### Unity Version
- **Unity 2023.1+** (LTS recommended)
- Universal Render Pipeline (URP) 14.0+

### Target Platform
- **Mobile**: iOS & Android
- **Target Performance**: 60 FPS
- **Resolution**: 1080p portrait orientation

### Required Packages
1. Universal RP (com.unity.render-pipelines.universal)
2. Cinemachine (com.unity.cinemachine) - Optional but recommended
3. Input System (com.unity.inputsystem) - Optional for enhanced controls

---

## 📦 Initial Setup

### Step 1: Create New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select **3D (URP)** template
4. Name it "SkywardZen"
5. Click "Create Project"

### Step 2: Import Scripts

1. Copy all scripts from `/SkywardZen/Assets/Scripts/` to your Unity project
2. Wait for compilation to complete
3. Verify no errors in Console

**Script Structure:**
```
Assets/
└── Scripts/
    ├── Player/
    │   ├── PlayerMovement3D.cs
    │   └── WorldWrapper.cs
    ├── Level/
    │   └── LevelManager.cs
    ├── Camera/
    │   └── CameraController.cs
    ├── Platforms/
    │   ├── Platform.cs
    │   ├── MovingPlatform.cs
    │   └── CrumblingPlatform.cs
    └── Utilities/
        └── ObjectPool.cs
```

### Step 3: Configure Project Settings

#### Player Settings
- **Company Name**: Your name/studio
- **Product Name**: Skyward Zen
- **Default Orientation**: Portrait
- **Multithreaded Rendering**: Enabled (for better mobile performance)

#### Quality Settings
- **V Sync Count**: Don't Sync (we'll manage frame rate manually)
- **Anti Aliasing**: 2x Multi Sampling (URP handles this)
- **Texture Quality**: Full Res

#### Graphics Settings
- **Render Pipeline Asset**: URP-HighFidelity (or create custom - see URP guide)

---

## 🎮 Scene Setup

### Step 1: Create Player

1. Create GameObject > 3D Object > Sphere
2. Rename to "Player"
3. Scale: (1, 1, 1)
4. Add **Rigidbody** component
5. Add **PlayerMovement3D** script
6. Add **WorldWrapper** script (optional)
7. **Create Tag**: "Player" and assign it

#### PlayerMovement3D Settings:
```
Horizontal Speed: 10
Max Horizontal Speed: 8
Jump Force: 15
Gravity: -25
Stretch Amount: 0.3
Squash Amount: 0.4
Platform Layer: Platforms (create this layer)
```

### Step 2: Create Camera

1. Select Main Camera
2. Position: (0, 5, -10)
3. Rotation: (15, 0, 0) - slight downward angle
4. Add **CameraController** script
5. Assign Player reference

#### CameraController Settings:
```
Offset: (0, 5, -10)
Smooth Speed: 5
Dead Zone Height: 2
Activation Threshold: 1
```

### Step 3: Create Platform Prefabs

#### Static Platform:
1. Create GameObject > 3D Object > Cylinder
2. Rename to "Platform_Static"
3. Scale: (2, 0.3, 2)
4. Add **Platform** script
5. Set Layer to "Platforms"
6. Add Collider (should be auto-added)
7. Create Material > Assign pastel color (e.g., soft pink #FFB6C1)
8. Drag to Prefabs folder

#### Moving Platform:
1. Duplicate Static Platform prefab
2. Rename to "Platform_Moving"
3. Add **MovingPlatform** script
4. Different material color (e.g., cyan #87CEEB)

#### Crumbling Platform:
1. Duplicate Static Platform prefab
2. Rename to "Platform_Crumbling"
3. Add **CrumblingPlatform** script
4. Glass-like material (e.g., white with transparency)

### Step 4: Create Level Manager

1. Create Empty GameObject
2. Rename to "LevelManager"
3. Add **LevelManager** script
4. Assign references:
   - Player: Player GameObject
   - Static Platform Prefab
   - Moving Platform Prefab
   - Crumbling Platform Prefab

#### LevelManager Settings:
```
Platform Spacing: 3
Horizontal Spawn Range: 8
Vertical Spawn Ahead: 30
Initial Platform Count: 15
Pool Size Per Type: 30

Probabilities:
- Static: 0.7
- Moving: 0.2
- Crumbling: 0.1
```

---

## 🎨 Visual Style Setup

### Lighting

1. Delete default Directional Light
2. Create new Directional Light
   - Intensity: 0.8
   - Color: Warm white (#FFF8DC)
   - Rotation: (50, -30, 0) - for soft shadows
3. Enable Soft Shadows in Light component

### Skybox

Create a gradient skybox for that "soft modern" look:

1. Create Material > Shader: Skybox/Gradient
2. Top Color: Soft Pink (#FFB6D9)
3. Middle Color: Pale Yellow (#FFF9E3)
4. Bottom Color: Light Cyan (#E0F4FF)
5. Assign in Window > Rendering > Lighting > Skybox Material

### Ambient Lighting

In Window > Rendering > Lighting:
- **Source**: Gradient
- **Sky Color**: #FFC8DD
- **Equator Color**: #FFEAA7
- **Ground Color**: #C8E6F5

---

## 📱 Mobile Optimization

### Physics Settings

Edit > Project Settings > Physics:
- **Default Solver Iterations**: 4 (down from 6 for mobile)
- **Default Solver Velocity Iterations**: 1
- **Bounce Threshold**: 2
- **Sleep Threshold**: 0.005

### Frame Rate Management

Add this script to a GameManager:

```csharp
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
}
```

### Build Settings

- **Compression Format**: LZ4 (faster loading)
- **Managed Stripping Level**: Medium
- **IL2CPP** for final builds (better performance than Mono)

---

## 🧪 Testing Your Setup

### Quick Test Checklist

1. ✅ Player spawns and falls
2. ✅ Player lands on platform and auto-jumps
3. ✅ Drag to move left/right works
4. ✅ Squash and stretch animation is visible
5. ✅ Camera follows player upward
6. ✅ New platforms generate as you rise
7. ✅ Old platforms despawn behind you
8. ✅ Screen wrapping works (if enabled)

### Performance Test

- Open Stats window (Game view > Stats)
- Target: **60 FPS** with **less than 100 draw calls**
- If performance is poor, reduce pool sizes or platform counts

---

## 🎛️ Input Configuration

### Touch Controls (Default)

The game uses classic touch drag by default:
- **Drag Left/Right**: Move player horizontally
- **Automatic Jumping**: Player jumps on platform contact

### Tilt Controls (Optional)

Enable in PlayerMovement3D:
```csharp
useTiltControls = true
tiltSensitivity = 3
```

---

## 🔧 Troubleshooting

### Player Not Jumping

- Check Rigidbody is **not kinematic**
- Verify Platform Layer is set correctly
- Check Ground Check Distance in PlayerMovement3D

### Platforms Not Spawning

- Verify LevelManager has all prefab references
- Check player reference is assigned
- Look for errors in Console

### Camera Not Following

- Assign Player reference in CameraController
- Check camera offset values
- Verify camera is not parented to anything

### Poor Performance

- Reduce `poolSizePerType` in LevelManager
- Disable shadows on some platforms
- Use simpler materials without transparency

---

## 📚 Next Steps

1. **Review URP_POSTPROCESSING_GUIDE.md** for visual polish
2. **Review CinemachineSetupGuide.md** for advanced camera
3. Customize platform materials and colors
4. Add UI (score, restart button)
5. Implement game state management
6. Add audio and music

---

## 🤝 Support

For Unity-specific questions:
- Unity Documentation: https://docs.unity3d.com/
- URP Documentation: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest

For script questions:
- Review inline code comments
- Check Debug visualizations (Gizmos in Scene view)
