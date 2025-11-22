# URP Post-Processing Guide - "High-End Mobile" Look

## Overview
This guide will help you configure Unity's Universal Render Pipeline (URP) post-processing to achieve the **modern, soft 3D aesthetic** for Skyward Zen. Think Instagram-worthy visuals optimized for mobile devices.

---

## 🎨 Target Aesthetic

**Visual Goals:**
- Soft, dreamy appearance with gentle bloom
- Pastel color grading (pinks, cyans, sunset oranges)
- Subtle vignette for focus
- Clean, minimal UI-friendly look
- **60 FPS on mobile** - performance is critical

---

## 📦 Setup Post-Processing

### Step 1: Install URP (If Not Already)

1. Window > Package Manager
2. Search "Universal RP"
3. Install latest version (14.0+)

### Step 2: Create URP Asset

If you started with a 3D template, you should already have URP assets. If not:

1. Right-click in Assets folder
2. Create > Rendering > URP Asset (with Universal Renderer)
3. Name it "SkywardZen_URP"
4. Go to Edit > Project Settings > Graphics
5. Assign your URP Asset to "Scriptable Render Pipeline Settings"

### Step 3: Enable Post-Processing in URP

1. Select your **URP Asset** in Assets
2. In Inspector, under **Quality**:
   - HDR: ✅ Enabled
   - Anti Aliasing (MSAA): **2x** (balance between quality and performance)
3. In Inspector, under **Lighting**:
   - Main Light: Per Pixel
   - Cast Shadows: ✅ Enabled
   - Soft Shadows: ✅ Enabled
4. In Inspector, under **Post-processing**:
   - ✅ Post Processing Feature enabled

### Step 4: Enable Post-Processing on Camera

1. Select **Main Camera**
2. In Camera component:
   - Rendering > Post Processing: ✅ Enable

---

## 🎬 Create Volume Profile

### Step 1: Create Global Volume

1. Right-click in Hierarchy > Volume > Global Volume
2. Rename to "Post-Processing Volume"
3. In Inspector, click "New" next to Profile to create new Volume Profile
4. Name it "SkywardZen_VolumeProfile"

### Step 2: Add Post-Processing Effects

Click **"Add Override"** and add the following effects:

---

## 🌟 Recommended Effect Settings

### 1. Bloom (Essential for Modern Look)

**Purpose:** Makes bright areas glow softly, crucial for that "premium" feel.

```
✅ Enable
Intensity: 0.15 - 0.25 (subtle glow)
Threshold: 0.9 (only bright areas bloom)
Scatter: 0.7 (smooth spread)
Tint: #FFFFFF or very light pink #FFF0F5
Clamp: 65472 (prevent over-bloom)
High Quality Filtering: ✅ (if performance allows)
```

**Mobile Optimization:**
- Intensity: Keep below 0.2
- Disable "High Quality Filtering" for +5-10 FPS

---

### 2. Color Adjustments

**Purpose:** Create the pastel, dreamy color palette.

```
✅ Enable

Post-exposure: 0.2 to 0.5 (brighten overall scene)
Contrast: -5 to -15 (softer, less harsh contrast)
Color Filter:
  - Default: #FFFFFF
  - Warm Sunset: #FFDAB9
  - Cool Cyan: #E0FFFF
  - Soft Pink: #FFE4E1
Hue Shift: 0 (or +5 for slight warmth)
Saturation: +10 to +20 (boost pastel vibrancy)
```

**Pro Tip:** Animate Color Filter for time-of-day effects!

---

### 3. Tonemapping (Critical for HDR)

**Purpose:** Controls how bright colors are mapped to screen.

```
✅ Enable
Mode: ACES (most cinematic) or Neutral (cleaner)
```

**Recommendations:**
- **ACES**: For dramatic, cinematic look
- **Neutral**: For cleaner, more accurate colors (better for pastels)

---

### 4. Vignette

**Purpose:** Subtle darkening at screen edges to focus attention on center.

```
✅ Enable
Color: Black (#000000) or dark purple (#1A0033) for creative touch
Center: (0.5, 0.5) - centered
Intensity: 0.2 - 0.3 (very subtle)
Smoothness: 0.4
Rounded: ✅ Enable (for softer edges)
```

**Mobile Note:** Vignette has minimal performance impact - safe to use!

---

### 5. Ambient Occlusion (Optional - Performance Cost!)

**Purpose:** Adds subtle shadows in crevices for depth.

```
⚠️ Only enable if targeting high-end mobile devices

✅ Enable
Mode: Scalable Ambient Obscurance (better performance)
Intensity: 0.5 - 0.7
Radius: 0.5
Quality: Medium
```

**Performance Impact:**
- Medium quality: -5 to -10 FPS
- Disable for 60 FPS on mid-range devices

---

### 6. Depth of Field (Optional)

**Purpose:** Blur background for cinematic depth. Use sparingly!

```
❌ NOT RECOMMENDED for vertical platformer
(Player needs to see upcoming platforms clearly)

Only use for:
- Main menu background
- Game over screen
- Cutscenes
```

---

### 7. Film Grain (Artistic Choice)

**Purpose:** Adds subtle texture for a "film-like" quality.

```
Optional - Test with your audience

✅ Enable
Type: Thin1 or Thin2
Intensity: 0.1 - 0.15 (barely noticeable)
Response: 0.8
```

**Note:** Some players dislike grain - make it toggleable in settings!

---

## 🎨 Example Preset Profiles

### Preset 1: "Soft Pastel Dream"

**Best for:** Cute, relaxing aesthetic

```yaml
Bloom:
  Intensity: 0.2
  Threshold: 0.9
  Tint: #FFF0F5 (light pink)

Color Adjustments:
  Post-exposure: 0.3
  Contrast: -10
  Color Filter: #FFE4E1 (pastel pink)
  Saturation: +15

Tonemapping:
  Mode: Neutral

Vignette:
  Intensity: 0.25
  Color: #1A0033 (dark purple)
```

---

### Preset 2: "Sunset Vibes"

**Best for:** Warm, Instagram aesthetic

```yaml
Bloom:
  Intensity: 0.25
  Threshold: 0.85
  Tint: #FFDAB9 (peach)

Color Adjustments:
  Post-exposure: 0.4
  Contrast: -8
  Color Filter: #FFA07A (light salmon)
  Saturation: +20
  Hue Shift: +5

Tonemapping:
  Mode: ACES

Vignette:
  Intensity: 0.3
  Color: #2B1700 (dark brown)
```

---

### Preset 3: "Cyberpunk Neon"

**Best for:** Futuristic, high-energy look

```yaml
Bloom:
  Intensity: 0.35
  Threshold: 0.7
  Tint: #00FFFF (cyan)

Color Adjustments:
  Post-exposure: 0.2
  Contrast: +10 (more dramatic!)
  Color Filter: #E0FFFF (light cyan)
  Saturation: +30

Tonemapping:
  Mode: ACES

Vignette:
  Intensity: 0.4
  Color: #000033 (dark blue)
```

---

## 📱 Mobile Optimization Strategies

### Performance Tiers

**Tier 1: High-End Devices** (iPhone 12+, Samsung S21+)
- All effects enabled
- MSAA 4x
- Ambient Occlusion ON
- Bloom High Quality ON

**Tier 2: Mid-Range** (iPhone 11, Samsung A52)
- Bloom, Color Adjustments, Tonemapping, Vignette
- MSAA 2x
- Ambient Occlusion OFF
- Bloom High Quality OFF

**Tier 3: Low-End** (Older devices)
- Only Color Adjustments and Tonemapping
- MSAA OFF
- All other effects OFF

### Auto-Detect Device Quality

Add this to your GameManager:

```csharp
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] private Volume postProcessVolume;

    void Start()
    {
        ConfigureForDevice();
    }

    void ConfigureForDevice()
    {
        int tier = GetDeviceTier();

        if (postProcessVolume.profile.TryGet<Bloom>(out var bloom))
        {
            bloom.active = tier >= 2;
        }

        if (postProcessVolume.profile.TryGet<AmbientOcclusion>(out var ao))
        {
            ao.active = tier >= 3;
        }

        // Set MSAA based on tier
        UniversalRenderPipelineAsset urpAsset =
            GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (urpAsset != null)
        {
            urpAsset.msaaSampleCount = tier >= 2 ? 4 : (tier == 1 ? 2 : 1);
        }
    }

    int GetDeviceTier()
    {
        // Simple heuristic based on memory
        int systemMemoryMB = SystemInfo.systemMemorySize;

        if (systemMemoryMB >= 6000) return 3; // High-end
        if (systemMemoryMB >= 3000) return 2; // Mid-range
        return 1; // Low-end
    }
}
```

---

## 🎯 Material Recommendations

### Platform Materials

To work well with post-processing:

**Static Platform:**
```
Shader: URP/Lit
Base Color: #FFB6C1 (pastel pink)
Metallic: 0
Smoothness: 0.3 - 0.5 (slight shine)
Emission: OFF (unless you want glow)
```

**Moving Platform:**
```
Shader: URP/Lit
Base Color: #87CEEB (cyan)
Metallic: 0.1
Smoothness: 0.6
Emission: Very light cyan (#87CEEB) at 0.1 intensity
```

**Crumbling Platform:**
```
Shader: URP/Lit
Base Color: #F0F8FF (alice blue)
Metallic: 0.3
Smoothness: 0.8 (glass-like)
Rendering Mode: Transparent (Alpha: 0.7)
```

---

## 🧪 Testing Your Post-Processing

### Visual Checklist

1. ✅ Bright areas have soft glow (not harsh)
2. ✅ Colors are vibrant but not oversaturated
3. ✅ Vignette is noticeable but not distracting
4. ✅ Overall scene feels "soft" and dreamy
5. ✅ Performance stays at 60 FPS

### Common Issues

**Problem:** Everything looks washed out
- **Fix:** Reduce Post-exposure, increase Contrast

**Problem:** Colors look dull
- **Fix:** Increase Saturation in Color Adjustments

**Problem:** Too much bloom
- **Fix:** Increase Threshold, reduce Intensity

**Problem:** FPS drops below 60
- **Fix:** Disable Ambient Occlusion, reduce MSAA

---

## 🎬 Animation Ideas

### Dynamic Color Transitions

Animate the Color Filter for time-based effects:

```csharp
using UnityEngine.Rendering;

public class DynamicColorGrading : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private float cycleDuration = 60f;

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        volume.profile.TryGet(out colorAdjustments);
    }

    void Update()
    {
        float t = (Time.time % cycleDuration) / cycleDuration;
        Color currentColor = colorGradient.Evaluate(t);
        colorAdjustments.colorFilter.value = currentColor;
    }
}
```

---

## 📚 Further Reading

- [URP Post-Processing Official Docs](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/post-processing-ssao.html)
- [Volume Profiles Best Practices](https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@latest/index.html?subfolder=/manual/Volumes.html)

---

**Remember:** The goal is to enhance the game, not distract from it. Start subtle and iterate based on playtesting!
