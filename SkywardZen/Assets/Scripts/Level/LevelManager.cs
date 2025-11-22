using UnityEngine;
using System.Collections.Generic;

namespace SkywardZen.Level
{
    /// <summary>
    /// Manages procedural platform generation with object pooling for optimal mobile performance.
    /// Generates platforms relative to player position and cleans up off-screen platforms.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform platformParent;

        [Header("Platform Prefabs")]
        [SerializeField] private GameObject staticPlatformPrefab;
        [SerializeField] private GameObject movingPlatformPrefab;
        [SerializeField] private GameObject crumblingPlatformPrefab;

        [Header("Generation Settings")]
        [SerializeField] private float platformSpacing = 3f;
        [SerializeField] private float horizontalSpawnRange = 8f;
        [SerializeField] private float verticalSpawnAhead = 30f;
        [SerializeField] private int initialPlatformCount = 15;

        [Header("Platform Type Probabilities (0-1)")]
        [SerializeField] private float staticPlatformChance = 0.7f;
        [SerializeField] private float movingPlatformChance = 0.2f;
        [SerializeField] private float crumblingPlatformChance = 0.1f;

        [Header("Object Pooling")]
        [SerializeField] private int poolSizePerType = 30;

        [Header("Cleanup")]
        [SerializeField] private float cleanupDistanceBelowPlayer = 20f;

        // Pools
        private ObjectPool staticPlatformPool;
        private ObjectPool movingPlatformPool;
        private ObjectPool crumblingPlatformPool;

        // Generation state
        private float lastPlatformY;
        private float highestPlayerY;
        private List<GameObject> activePlatforms = new List<GameObject>();

        private void Awake()
        {
            if (platformParent == null)
            {
                platformParent = new GameObject("Platforms").transform;
            }

            InitializePools();
        }

        private void Start()
        {
            GenerateInitialPlatforms();
        }

        private void Update()
        {
            TrackPlayerHeight();
            GeneratePlatformsAhead();
            CleanupPlatformsBehind();
        }

        #region Object Pooling

        private void InitializePools()
        {
            staticPlatformPool = new ObjectPool(
                staticPlatformPrefab,
                poolSizePerType,
                platformParent
            );

            movingPlatformPool = new ObjectPool(
                movingPlatformPrefab,
                poolSizePerType,
                platformParent
            );

            crumblingPlatformPool = new ObjectPool(
                crumblingPlatformPrefab,
                poolSizePerType,
                platformParent
            );
        }

        #endregion

        #region Platform Generation

        private void GenerateInitialPlatforms()
        {
            // First platform directly under player
            Vector3 startPos = player.position - Vector3.up * 2f;
            SpawnPlatform(PlatformType.Static, startPos);

            lastPlatformY = startPos.y;

            // Generate initial set of platforms
            for (int i = 0; i < initialPlatformCount; i++)
            {
                GenerateNextPlatform();
            }
        }

        private void TrackPlayerHeight()
        {
            if (player.position.y > highestPlayerY)
            {
                highestPlayerY = player.position.y;
            }
        }

        private void GeneratePlatformsAhead()
        {
            // Generate new platforms when player gets close to the highest platform
            while (lastPlatformY < player.position.y + verticalSpawnAhead)
            {
                GenerateNextPlatform();
            }
        }

        private void GenerateNextPlatform()
        {
            // Calculate next platform position
            float nextY = lastPlatformY + platformSpacing;
            float randomX = Random.Range(-horizontalSpawnRange, horizontalSpawnRange);
            float randomZ = Random.Range(-horizontalSpawnRange * 0.3f, horizontalSpawnRange * 0.3f);

            Vector3 spawnPos = new Vector3(randomX, nextY, randomZ);

            // Determine platform type based on probabilities
            PlatformType type = DeterminePlatformType();

            // Spawn the platform
            SpawnPlatform(type, spawnPos);

            lastPlatformY = nextY;
        }

        private PlatformType DeterminePlatformType()
        {
            float random = Random.value;

            if (random < staticPlatformChance)
            {
                return PlatformType.Static;
            }
            else if (random < staticPlatformChance + movingPlatformChance)
            {
                return PlatformType.Moving;
            }
            else
            {
                return PlatformType.Crumbling;
            }
        }

        private void SpawnPlatform(PlatformType type, Vector3 position)
        {
            GameObject platform = null;

            switch (type)
            {
                case PlatformType.Static:
                    platform = staticPlatformPool.GetObject();
                    break;
                case PlatformType.Moving:
                    platform = movingPlatformPool.GetObject();
                    break;
                case PlatformType.Crumbling:
                    platform = crumblingPlatformPool.GetObject();
                    break;
            }

            if (platform != null)
            {
                platform.transform.position = position;
                platform.transform.rotation = Quaternion.identity;
                platform.SetActive(true);

                // Initialize platform component if it has one
                var platformComponent = platform.GetComponent<Platforms.Platform>();
                if (platformComponent != null)
                {
                    platformComponent.Initialize();
                }

                activePlatforms.Add(platform);
            }
        }

        #endregion

        #region Cleanup

        private void CleanupPlatformsBehind()
        {
            // Remove platforms that are far below the player
            float cleanupThreshold = player.position.y - cleanupDistanceBelowPlayer;

            for (int i = activePlatforms.Count - 1; i >= 0; i--)
            {
                GameObject platform = activePlatforms[i];

                if (platform != null && platform.transform.position.y < cleanupThreshold)
                {
                    // Return to pool
                    ReturnPlatformToPool(platform);
                    activePlatforms.RemoveAt(i);
                }
            }
        }

        private void ReturnPlatformToPool(GameObject platform)
        {
            var platformComponent = platform.GetComponent<Platforms.Platform>();
            if (platformComponent != null)
            {
                switch (platformComponent.Type)
                {
                    case PlatformType.Static:
                        staticPlatformPool.ReturnObject(platform);
                        break;
                    case PlatformType.Moving:
                        movingPlatformPool.ReturnObject(platform);
                        break;
                    case PlatformType.Crumbling:
                        crumblingPlatformPool.ReturnObject(platform);
                        break;
                }
            }
            else
            {
                // Fallback: just deactivate
                platform.SetActive(false);
            }
        }

        #endregion

        #region Public API

        public void ResetLevel()
        {
            // Clear all active platforms
            foreach (var platform in activePlatforms)
            {
                if (platform != null)
                {
                    ReturnPlatformToPool(platform);
                }
            }

            activePlatforms.Clear();
            lastPlatformY = 0f;
            highestPlayerY = 0f;

            // Regenerate initial platforms
            GenerateInitialPlatforms();
        }

        public float GetHighestPoint() => lastPlatformY;

        #endregion
    }

    #region Platform Type Enum

    public enum PlatformType
    {
        Static,
        Moving,
        Crumbling
    }

    #endregion
}
