using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace SkywardZen.Environment
{
    /// <summary>
    /// Manages biome transitions with different color palettes and visual styles
    /// </summary>
    public class BiomeManager : MonoBehaviour
    {
        public static BiomeManager Instance { get; private set; }

        [Header("Current Biome")]
        [SerializeField] private BiomeType currentBiome = BiomeType.PastelDream;
        [SerializeField] private int currentBiomeIndex = 0;

        [Header("Biome Settings")]
        [SerializeField] private List<BiomeData> biomes = new List<BiomeData>();
        [SerializeField] private float heightPerBiome = 100f; // Change biome every 100 meters

        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 2f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Events")]
        public UnityEvent<BiomeType> OnBiomeChanged;

        private bool isTransitioning = false;
        private float transitionTimer = 0f;
        private BiomeData previousBiome;
        private BiomeData targetBiome;

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeBiomes();
        }

        private void Start()
        {
            // Apply initial biome
            if (biomes.Count > 0)
            {
                ApplyBiome(biomes[0], true);
            }
        }

        private void Update()
        {
            // Update biome transition
            if (isTransitioning)
            {
                UpdateTransition();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Change biome based on player height
        /// </summary>
        public void UpdateBiomeByHeight(float playerHeight)
        {
            int newBiomeIndex = Mathf.FloorToInt(playerHeight / heightPerBiome);
            newBiomeIndex = Mathf.Clamp(newBiomeIndex, 0, biomes.Count - 1);

            if (newBiomeIndex != currentBiomeIndex && !isTransitioning)
            {
                TransitionToBiome(newBiomeIndex);
            }
        }

        /// <summary>
        /// Force transition to a specific biome
        /// </summary>
        public void TransitionToBiome(int biomeIndex)
        {
            if (biomeIndex < 0 || biomeIndex >= biomes.Count) return;
            if (biomeIndex == currentBiomeIndex) return;

            Debug.Log($"[BiomeManager] Transitioning from {currentBiome} to {biomes[biomeIndex].type}");

            currentBiomeIndex = biomeIndex;
            previousBiome = biomes[currentBiomeIndex > 0 ? currentBiomeIndex - 1 : 0];
            targetBiome = biomes[biomeIndex];
            currentBiome = targetBiome.type;

            isTransitioning = true;
            transitionTimer = 0f;

            OnBiomeChanged?.Invoke(currentBiome);
        }

        /// <summary>
        /// Get material for platform based on current biome
        /// </summary>
        public Material GetPlatformMaterial(PlatformVariant variant)
        {
            if (biomes.Count == 0 || currentBiomeIndex >= biomes.Count) return null;

            BiomeData biome = biomes[currentBiomeIndex];

            switch (variant)
            {
                case PlatformVariant.Static:
                    return biome.staticPlatformMaterial;
                case PlatformVariant.Moving:
                    return biome.movingPlatformMaterial;
                case PlatformVariant.Crumbling:
                    return biome.crumblingPlatformMaterial;
                default:
                    return biome.staticPlatformMaterial;
            }
        }

        /// <summary>
        /// Get current biome data
        /// </summary>
        public BiomeData GetCurrentBiome()
        {
            if (biomes.Count == 0 || currentBiomeIndex >= biomes.Count) return null;
            return biomes[currentBiomeIndex];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize default biomes with color palettes
        /// </summary>
        private void InitializeBiomes()
        {
            if (biomes.Count > 0) return; // Already initialized in inspector

            // Create default biomes programmatically
            biomes.Add(new BiomeData
            {
                type = BiomeType.PastelDream,
                name = "Pastel Dream",
                skyColor = new Color(0.94f, 0.97f, 1f), // Alice Blue
                fogColor = new Color(0.9f, 0.9f, 0.95f),
                ambientColor = new Color(1f, 0.71f, 0.76f) // Pastel Pink
            });

            biomes.Add(new BiomeData
            {
                type = BiomeType.SunsetOasis,
                name = "Sunset Oasis",
                skyColor = new Color(1f, 0.85f, 0.73f), // Sunset Orange
                fogColor = new Color(1f, 0.6f, 0.4f),
                ambientColor = new Color(1f, 0.5f, 0.3f)
            });

            biomes.Add(new BiomeData
            {
                type = BiomeType.CyberNeon,
                name = "Cyber Neon",
                skyColor = new Color(0.1f, 0.05f, 0.2f), // Dark purple
                fogColor = new Color(0.3f, 0f, 0.5f),
                ambientColor = new Color(0f, 0.8f, 1f) // Neon cyan
            });

            biomes.Add(new BiomeData
            {
                type = BiomeType.ForestMist,
                name = "Forest Mist",
                skyColor = new Color(0.7f, 0.9f, 0.8f), // Mint green
                fogColor = new Color(0.8f, 0.95f, 0.9f),
                ambientColor = new Color(0.4f, 0.8f, 0.5f)
            });

            biomes.Add(new BiomeData
            {
                type = BiomeType.CosmicVoid,
                name = "Cosmic Void",
                skyColor = new Color(0.05f, 0.05f, 0.15f), // Deep space blue
                fogColor = new Color(0.1f, 0f, 0.2f),
                ambientColor = new Color(0.6f, 0.4f, 0.9f) // Purple
            });

            Debug.Log($"[BiomeManager] Initialized {biomes.Count} default biomes");
        }

        /// <summary>
        /// Apply biome settings to the scene
        /// </summary>
        private void ApplyBiome(BiomeData biome, bool instant = false)
        {
            if (biome == null) return;

            Debug.Log($"[BiomeManager] Applying biome: {biome.name}");

            // Apply camera background color
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = biome.skyColor;
            }

            // Apply fog settings
            RenderSettings.fog = true;
            RenderSettings.fogColor = biome.fogColor;
            RenderSettings.fogDensity = 0.01f;

            // Apply ambient lighting
            RenderSettings.ambientLight = biome.ambientColor;
            RenderSettings.ambientIntensity = 1f;

            currentBiome = biome.type;
        }

        /// <summary>
        /// Update smooth transition between biomes
        /// </summary>
        private void UpdateTransition()
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            float smoothT = transitionCurve.Evaluate(t);

            // Lerp colors
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.Lerp(
                    previousBiome.skyColor,
                    targetBiome.skyColor,
                    smoothT
                );
            }

            RenderSettings.fogColor = Color.Lerp(
                previousBiome.fogColor,
                targetBiome.fogColor,
                smoothT
            );

            RenderSettings.ambientLight = Color.Lerp(
                previousBiome.ambientColor,
                targetBiome.ambientColor,
                smoothT
            );

            // Finish transition
            if (t >= 1f)
            {
                isTransitioning = false;
                ApplyBiome(targetBiome, true);
                Debug.Log($"[BiomeManager] Transition complete to {targetBiome.name}");
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            // Debug display
            GUILayout.BeginArea(new Rect(Screen.width - 260, 220, 250, 100));
            GUILayout.Label($"<b>Biome:</b> {currentBiome}");
            if (isTransitioning)
            {
                float progress = (transitionTimer / transitionDuration) * 100f;
                GUILayout.Label($"<b>Transition:</b> {progress:F0}%");
            }
            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Biome data with color palette and materials
    /// </summary>
    [System.Serializable]
    public class BiomeData
    {
        public BiomeType type;
        public string name;

        [Header("Colors")]
        public Color skyColor = Color.white;
        public Color fogColor = Color.gray;
        public Color ambientColor = Color.white;

        [Header("Materials")]
        public Material staticPlatformMaterial;
        public Material movingPlatformMaterial;
        public Material crumblingPlatformMaterial;

        [Header("Lighting")]
        public float lightIntensity = 1f;
        public Color lightColor = Color.white;
    }

    /// <summary>
    /// Types of biomes
    /// </summary>
    public enum BiomeType
    {
        PastelDream,    // Default soft colors
        SunsetOasis,    // Warm oranges and reds
        CyberNeon,      // Dark with neon accents
        ForestMist,     // Green and natural tones
        CosmicVoid      // Deep space purples and blues
    }

    /// <summary>
    /// Platform variants for material selection
    /// </summary>
    public enum PlatformVariant
    {
        Static,
        Moving,
        Crumbling
    }
}
