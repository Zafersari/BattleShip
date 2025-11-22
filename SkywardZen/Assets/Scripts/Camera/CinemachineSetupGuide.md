# Cinemachine Setup Guide for Skyward Zen

## Why Cinemachine?
Cinemachine provides professional-grade camera control with minimal code. It's perfect for creating smooth, cinematic camera movements in Unity.

## Installation

1. Open Unity Package Manager (Window > Package Manager)
2. Search for "Cinemachine"
3. Click Install

## Setup Steps

### 1. Create Virtual Camera

1. Right-click in Hierarchy > Cinemachine > Virtual Camera
2. Rename it to "VCam_Player"
3. Set the Follow target to your Player GameObject
4. Set Look At target to your Player GameObject (optional)

### 2. Configure Virtual Camera Settings

```
Body: Framing Transposer
- Lookahead Time: 0.2
- Lookahead Smoothing: 10
- Lookahead Ignore Y: False
- Dead Zone Width: 0.1
- Dead Zone Height: 0.3 (THIS IS KEY - prevents jitter)
- Screen Y: 0.4 (keeps player in lower portion of screen)
- Damping:
  - X: 0.5
  - Y: 1.5 (slower Y for smooth vertical tracking)
  - Z: 0.5

Aim: Do Nothing (or Composer if you want dynamic aiming)

Noise: None (add Basic Multi Channel Perlin for camera shake effects)
```

### 3. Upward-Only Movement Extension

Create a custom Cinemachine Extension to prevent downward movement:

```csharp
using UnityEngine;
using Cinemachine;

namespace SkywardZen.Camera
{
    public class CinemachineUpwardOnly : CinemachineExtension
    {
        private float highestY;
        private bool initialized;

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            // Only modify in the Body stage
            if (stage == CinemachineCore.Stage.Body)
            {
                if (!initialized)
                {
                    highestY = state.CorrectedPosition.y;
                    initialized = true;
                }

                // Track highest Y position
                if (state.CorrectedPosition.y > highestY)
                {
                    highestY = state.CorrectedPosition.y;
                }

                // Prevent camera from going down
                Vector3 pos = state.CorrectedPosition;
                if (pos.y < highestY)
                {
                    pos.y = highestY;
                    state.PositionCorrection = pos - state.CorrectedPosition;
                }
            }
        }

        public void ResetHighestY()
        {
            initialized = false;
        }
    }
}
```

### 4. Add Extension to Virtual Camera

1. Select your VCam_Player
2. Click "Add Extension" > CinemachineUpwardOnly
3. Done!

## Advanced: Impulse for Jump Impact

Add camera shake when landing:

```csharp
// In PlayerMovement3D.cs
using Cinemachine;

[Header("Camera Shake")]
[SerializeField] private CinemachineImpulseSource impulseSource;

private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Platform"))
    {
        // Generate impulse on landing
        impulseSource?.GenerateImpulse(rb.velocity.magnitude * 0.1f);
    }
}
```

## Performance Notes

- Cinemachine is optimized and won't impact 60 FPS on mobile
- Use Virtual Camera's Update Method: "Smart Update" for best performance
- Disable "Aim" component if not needed (saves CPU)

## Recommended Settings for Mobile

```
Quality Settings:
- vSync: Don't Sync (handle frame rate manually)
- Target Frame Rate: 60 FPS

Cinemachine Brain (on Main Camera):
- Update Method: Smart Update
- Blend Update Method: Late Update
- Default Blend: EaseInOut, 0.5 seconds
```

## Comparing to Manual CameraController.cs

| Feature | CameraController.cs | Cinemachine |
|---------|---------------------|-------------|
| Setup Time | Immediate | 5-10 minutes |
| Customization | Full control | Extension-based |
| Dead Zone | Manual implementation | Built-in Framing Transposer |
| Camera Shake | Manual | Impulse System |
| Performance | Slightly lighter | Highly optimized |
| Professional Features | Limited | Extensive |

**Recommendation:** Use Cinemachine for production. Use CameraController.cs for quick prototyping or if you want full control.
