using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace SkywardZen.PowerUps
{
    /// <summary>
    /// Manages active power-ups and their effects on the player
    /// </summary>
    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Active Power-Ups")]
        [SerializeField] private Dictionary<PowerUpType, PowerUpData> activePowerUps = new Dictionary<PowerUpType, PowerUpData>();

        [Header("Power-Up Prefabs")]
        [SerializeField] private GameObject doubleJumpPrefab;
        [SerializeField] private GameObject slowMotionPrefab;
        [SerializeField] private GameObject magnetPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnChance = 0.1f; // 10% chance per platform
        [SerializeField] private float spawnHeightOffset = 1.5f;

        [Header("Events")]
        public UnityEvent<PowerUpType> OnPowerUpActivated;
        public UnityEvent<PowerUpType> OnPowerUpExpired;

        private float savedTimeScale = 1f;

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

            savedTimeScale = Time.timeScale;
        }

        private void Update()
        {
            UpdateActivePowerUps();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Activate a power-up with given duration and intensity
        /// </summary>
        public void ActivatePowerUp(PowerUpType type, float duration, float intensity = 1f)
        {
            Debug.Log($"[PowerUpManager] Activating {type} for {duration}s (intensity: {intensity})");

            // Create or update power-up data
            PowerUpData data = new PowerUpData
            {
                type = type,
                startTime = Time.time,
                duration = duration,
                intensity = intensity
            };

            // If already active, extend duration
            if (activePowerUps.ContainsKey(type))
            {
                activePowerUps[type] = data; // Refresh timer
            }
            else
            {
                activePowerUps.Add(type, data);
            }

            // Apply power-up effect
            ApplyPowerUpEffect(type, intensity);

            OnPowerUpActivated?.Invoke(type);
        }

        /// <summary>
        /// Deactivate a specific power-up
        /// </summary>
        public void DeactivatePowerUp(PowerUpType type)
        {
            if (!activePowerUps.ContainsKey(type)) return;

            Debug.Log($"[PowerUpManager] Deactivating {type}");

            // Remove power-up effect
            RemovePowerUpEffect(type);

            activePowerUps.Remove(type);

            OnPowerUpExpired?.Invoke(type);
        }

        /// <summary>
        /// Check if a specific power-up is active
        /// </summary>
        public bool IsPowerUpActive(PowerUpType type)
        {
            return activePowerUps.ContainsKey(type);
        }

        /// <summary>
        /// Get remaining time for a power-up
        /// </summary>
        public float GetRemainingTime(PowerUpType type)
        {
            if (!activePowerUps.ContainsKey(type)) return 0f;

            PowerUpData data = activePowerUps[type];
            float elapsed = Time.time - data.startTime;
            return Mathf.Max(0f, data.duration - elapsed);
        }

        /// <summary>
        /// Spawn a power-up at given position
        /// </summary>
        public void SpawnPowerUp(Vector3 position)
        {
            // Random chance to spawn
            if (Random.value > spawnChance) return;

            // Choose random power-up type
            PowerUpType randomType = (PowerUpType)Random.Range(0, 3); // DoubleJump, SlowMotion, Magnet

            GameObject prefab = GetPowerUpPrefab(randomType);
            if (prefab == null) return;

            // Spawn above platform
            Vector3 spawnPos = position + Vector3.up * spawnHeightOffset;
            Instantiate(prefab, spawnPos, Quaternion.identity);

            Debug.Log($"[PowerUpManager] Spawned {randomType} power-up at {spawnPos}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update all active power-ups and expire old ones
        /// </summary>
        private void UpdateActivePowerUps()
        {
            List<PowerUpType> toRemove = new List<PowerUpType>();

            foreach (var kvp in activePowerUps)
            {
                PowerUpData data = kvp.Value;
                float elapsed = Time.time - data.startTime;

                // Check if expired
                if (elapsed >= data.duration)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            // Remove expired power-ups
            foreach (PowerUpType type in toRemove)
            {
                DeactivatePowerUp(type);
            }
        }

        /// <summary>
        /// Apply the effect of a power-up
        /// </summary>
        private void ApplyPowerUpEffect(PowerUpType type, float intensity)
        {
            switch (type)
            {
                case PowerUpType.DoubleJump:
                    // Effect handled by PlayerMovement3D
                    break;

                case PowerUpType.SlowMotion:
                    savedTimeScale = Time.timeScale;
                    Time.timeScale = intensity; // intensity is the time scale (e.g., 0.5)
                    break;

                case PowerUpType.Magnet:
                    // Effect handled by separate magnet system
                    break;
            }
        }

        /// <summary>
        /// Remove the effect of a power-up
        /// </summary>
        private void RemovePowerUpEffect(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.DoubleJump:
                    // Effect handled by PlayerMovement3D
                    break;

                case PowerUpType.SlowMotion:
                    Time.timeScale = savedTimeScale;
                    break;

                case PowerUpType.Magnet:
                    // Effect handled by separate magnet system
                    break;
            }
        }

        /// <summary>
        /// Get prefab for given power-up type
        /// </summary>
        private GameObject GetPowerUpPrefab(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.DoubleJump:
                    return doubleJumpPrefab;
                case PowerUpType.SlowMotion:
                    return slowMotionPrefab;
                case PowerUpType.Magnet:
                    return magnetPrefab;
                default:
                    return null;
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            // Debug display of active power-ups
            GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 200));
            GUILayout.Label("<b>Active Power-Ups:</b>");

            foreach (var kvp in activePowerUps)
            {
                float remaining = GetRemainingTime(kvp.Key);
                GUILayout.Label($"{kvp.Key}: {remaining:F1}s");
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Data for tracking active power-ups
    /// </summary>
    [System.Serializable]
    public class PowerUpData
    {
        public PowerUpType type;
        public float startTime;
        public float duration;
        public float intensity;
    }
}
