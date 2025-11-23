using UnityEngine;
using System.Collections.Generic;

namespace SkywardZen.VFX
{
    /// <summary>
    /// Manages particle effects for platform interactions, jumps, and visual feedback
    /// Uses object pooling for performance
    /// </summary>
    public class ParticleEffectsManager : MonoBehaviour
    {
        public static ParticleEffectsManager Instance { get; private set; }

        [Header("Particle Prefabs")]
        [SerializeField] private GameObject landingParticlePrefab;
        [SerializeField] private GameObject jumpParticlePrefab;
        [SerializeField] private GameObject platformBreakParticlePrefab;
        [SerializeField] private GameObject powerUpCollectParticlePrefab;
        [SerializeField] private GameObject trailParticlePrefab;

        [Header("Pool Settings")]
        [SerializeField] private int poolSizePerEffect = 10;

        [Header("Effect Colors")]
        [SerializeField] private Color defaultLandingColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Color movingPlatformColor = new Color(0f, 0.8f, 1f, 0.8f);
        [SerializeField] private Color crumblingPlatformColor = new Color(1f, 0.5f, 0f, 0.8f);
        [SerializeField] private Color powerUpColor = new Color(1f, 0.8f, 0f, 1f);

        private Dictionary<ParticleEffectType, Queue<GameObject>> particlePools = new Dictionary<ParticleEffectType, Queue<GameObject>>();
        private Dictionary<ParticleEffectType, GameObject> particlePrefabs = new Dictionary<ParticleEffectType, GameObject>();

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

            InitializeParticlePools();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Play landing particle effect
        /// </summary>
        public void PlayLandingEffect(Vector3 position, Core.PlatformType platformType = Core.PlatformType.Static)
        {
            Color effectColor = GetColorForPlatform(platformType);
            PlayEffect(ParticleEffectType.Landing, position, effectColor);
        }

        /// <summary>
        /// Play jump particle effect
        /// </summary>
        public void PlayJumpEffect(Vector3 position)
        {
            PlayEffect(ParticleEffectType.Jump, position, defaultLandingColor);
        }

        /// <summary>
        /// Play platform break effect
        /// </summary>
        public void PlayPlatformBreakEffect(Vector3 position)
        {
            PlayEffect(ParticleEffectType.PlatformBreak, position, crumblingPlatformColor);
        }

        /// <summary>
        /// Play power-up collection effect
        /// </summary>
        public void PlayPowerUpCollectEffect(Vector3 position, PowerUps.PowerUpType powerUpType)
        {
            Color color = GetColorForPowerUp(powerUpType);
            PlayEffect(ParticleEffectType.PowerUpCollect, position, color);
        }

        /// <summary>
        /// Create trail effect for player
        /// </summary>
        public GameObject CreateTrailEffect(Transform parent)
        {
            if (trailParticlePrefab == null) return null;

            GameObject trail = Instantiate(trailParticlePrefab, parent);
            trail.transform.localPosition = Vector3.zero;

            Debug.Log("[ParticleEffectsManager] Trail effect created");

            return trail;
        }

        /// <summary>
        /// Play custom particle effect at position
        /// </summary>
        public void PlayEffect(ParticleEffectType effectType, Vector3 position, Color color)
        {
            GameObject effectObj = GetParticleFromPool(effectType);

            if (effectObj != null)
            {
                effectObj.transform.position = position;
                effectObj.SetActive(true);

                ParticleSystem ps = effectObj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    // Set color
                    var main = ps.main;
                    main.startColor = color;

                    // Play effect
                    ps.Play();

                    // Return to pool after duration
                    StartCoroutine(ReturnToPoolAfterDelay(effectObj, effectType, ps.main.duration + ps.main.startLifetime.constantMax));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize particle effect pools
        /// </summary>
        private void InitializeParticlePools()
        {
            // Map effect types to prefabs
            particlePrefabs[ParticleEffectType.Landing] = landingParticlePrefab;
            particlePrefabs[ParticleEffectType.Jump] = jumpParticlePrefab;
            particlePrefabs[ParticleEffectType.PlatformBreak] = platformBreakParticlePrefab;
            particlePrefabs[ParticleEffectType.PowerUpCollect] = powerUpCollectParticlePrefab;

            // Create pools for each effect type
            foreach (ParticleEffectType effectType in System.Enum.GetValues(typeof(ParticleEffectType)))
            {
                if (!particlePrefabs.ContainsKey(effectType) || particlePrefabs[effectType] == null)
                    continue;

                Queue<GameObject> pool = new Queue<GameObject>();

                for (int i = 0; i < poolSizePerEffect; i++)
                {
                    GameObject obj = CreateParticleObject(effectType);
                    pool.Enqueue(obj);
                }

                particlePools[effectType] = pool;

                Debug.Log($"[ParticleEffectsManager] Created pool for {effectType} with {poolSizePerEffect} objects");
            }
        }

        /// <summary>
        /// Create a new particle object for the pool
        /// </summary>
        private GameObject CreateParticleObject(ParticleEffectType effectType)
        {
            GameObject prefab = particlePrefabs[effectType];
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            obj.name = $"{effectType}_Particle";

            return obj;
        }

        /// <summary>
        /// Get particle from pool
        /// </summary>
        private GameObject GetParticleFromPool(ParticleEffectType effectType)
        {
            if (!particlePools.ContainsKey(effectType))
            {
                Debug.LogWarning($"[ParticleEffectsManager] No pool for effect type: {effectType}");
                return null;
            }

            Queue<GameObject> pool = particlePools[effectType];

            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                // Pool exhausted, create new object
                Debug.LogWarning($"[ParticleEffectsManager] Pool exhausted for {effectType}, creating new object");
                return CreateParticleObject(effectType);
            }
        }

        /// <summary>
        /// Return particle to pool
        /// </summary>
        private void ReturnParticleToPool(GameObject obj, ParticleEffectType effectType)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.position = transform.position;

            if (particlePools.ContainsKey(effectType))
            {
                particlePools[effectType].Enqueue(obj);
            }
        }

        /// <summary>
        /// Coroutine to return particle to pool after delay
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject obj, ParticleEffectType effectType, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnParticleToPool(obj, effectType);
        }

        /// <summary>
        /// Get color based on platform type
        /// </summary>
        private Color GetColorForPlatform(Core.PlatformType platformType)
        {
            switch (platformType)
            {
                case Core.PlatformType.Moving:
                    return movingPlatformColor;
                case Core.PlatformType.Crumbling:
                    return crumblingPlatformColor;
                default:
                    return defaultLandingColor;
            }
        }

        /// <summary>
        /// Get color based on power-up type
        /// </summary>
        private Color GetColorForPowerUp(PowerUps.PowerUpType powerUpType)
        {
            switch (powerUpType)
            {
                case PowerUps.PowerUpType.DoubleJump:
                    return new Color(0.3f, 0.8f, 1f, 1f); // Blue
                case PowerUps.PowerUpType.SlowMotion:
                    return new Color(0.8f, 0.3f, 1f, 1f); // Purple
                case PowerUps.PowerUpType.Magnet:
                    return new Color(1f, 0.8f, 0.2f, 1f); // Gold
                default:
                    return powerUpColor;
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            // Debug display
            GUILayout.BeginArea(new Rect(Screen.width - 260, 490, 250, 150));
            GUILayout.Label("<b>Particle Pools:</b>");

            foreach (var kvp in particlePools)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value.Count} available");
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Types of particle effects
    /// </summary>
    public enum ParticleEffectType
    {
        Landing,
        Jump,
        PlatformBreak,
        PowerUpCollect,
        Trail
    }
}
