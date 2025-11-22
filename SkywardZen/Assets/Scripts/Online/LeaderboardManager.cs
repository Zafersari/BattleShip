using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace SkywardZen.Online
{
    /// <summary>
    /// Manages online leaderboards for competitive scoring
    /// Can integrate with PlayFab, Unity Gaming Services, or custom backend
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("Leaderboard Settings")]
        [SerializeField] private string leaderboardId = "global_high_scores";
        [SerializeField] private int maxEntries = 100;
        [SerializeField] private bool useLocalFallback = true;

        [Header("Player Info")]
        [SerializeField] private string playerName = "Player";
        [SerializeField] private string playerId = "";

        [Header("Cached Leaderboard")]
        [SerializeField] private List<LeaderboardEntry> globalLeaderboard = new List<LeaderboardEntry>();
        [SerializeField] private List<LeaderboardEntry> friendsLeaderboard = new List<LeaderboardEntry>();

        [Header("Events")]
        public UnityEvent<List<LeaderboardEntry>> OnLeaderboardLoaded;
        public UnityEvent<int> OnScoreSubmitted; // Rank
        public UnityEvent<string> OnLeaderboardError;

        [Header("Connection Status")]
        [SerializeField] private bool isConnected = false;
        [SerializeField] private bool isSubmitting = false;
        [SerializeField] private bool isLoading = false;

        private const string LOCAL_LEADERBOARD_KEY = "LocalLeaderboard_";
        private const string PLAYER_NAME_KEY = "PlayerName";

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

            LoadPlayerInfo();
            LoadLocalLeaderboard();
        }

        private void Start()
        {
            // Try to connect to online service
            ConnectToOnlineService();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Submit score to online leaderboard
        /// </summary>
        public void SubmitScore(int score)
        {
            if (isSubmitting)
            {
                Debug.LogWarning("[LeaderboardManager] Already submitting score!");
                return;
            }

            Debug.Log($"[LeaderboardManager] Submitting score: {score} for player: {playerName}");

            isSubmitting = true;

            if (isConnected)
            {
                // Submit to online service
                SubmitScoreOnline(score);
            }
            else if (useLocalFallback)
            {
                // Use local leaderboard as fallback
                SubmitScoreLocal(score);
            }
            else
            {
                Debug.LogError("[LeaderboardManager] Not connected and local fallback disabled!");
                OnLeaderboardError?.Invoke("Not connected to online service");
                isSubmitting = false;
            }
        }

        /// <summary>
        /// Load global leaderboard
        /// </summary>
        public void LoadGlobalLeaderboard()
        {
            if (isLoading)
            {
                Debug.LogWarning("[LeaderboardManager] Already loading leaderboard!");
                return;
            }

            Debug.Log("[LeaderboardManager] Loading global leaderboard...");

            isLoading = true;

            if (isConnected)
            {
                LoadLeaderboardOnline();
            }
            else if (useLocalFallback)
            {
                LoadLocalLeaderboard();
                OnLeaderboardLoaded?.Invoke(globalLeaderboard);
                isLoading = false;
            }
            else
            {
                Debug.LogError("[LeaderboardManager] Not connected and local fallback disabled!");
                OnLeaderboardError?.Invoke("Not connected to online service");
                isLoading = false;
            }
        }

        /// <summary>
        /// Load friends leaderboard
        /// </summary>
        public void LoadFriendsLeaderboard()
        {
            Debug.Log("[LeaderboardManager] Loading friends leaderboard...");

            if (isConnected)
            {
                LoadFriendsOnline();
            }
            else
            {
                Debug.LogWarning("[LeaderboardManager] Friends leaderboard requires online connection");
                OnLeaderboardError?.Invoke("Friends leaderboard requires online connection");
            }
        }

        /// <summary>
        /// Get player's rank
        /// </summary>
        public int GetPlayerRank()
        {
            for (int i = 0; i < globalLeaderboard.Count; i++)
            {
                if (globalLeaderboard[i].playerId == playerId)
                {
                    return i + 1; // Rank is 1-indexed
                }
            }

            return -1; // Not found
        }

        /// <summary>
        /// Set player name
        /// </summary>
        public void SetPlayerName(string name)
        {
            playerName = name;
            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
            PlayerPrefs.Save();

            Debug.Log($"[LeaderboardManager] Player name set to: {playerName}");
        }

        /// <summary>
        /// Get global leaderboard entries
        /// </summary>
        public List<LeaderboardEntry> GetGlobalLeaderboard()
        {
            return new List<LeaderboardEntry>(globalLeaderboard);
        }

        #endregion

        #region Private Methods - Online Service

        /// <summary>
        /// Connect to online leaderboard service
        /// PLACEHOLDER: Integrate with PlayFab, Unity Gaming Services, etc.
        /// </summary>
        private void ConnectToOnlineService()
        {
            Debug.Log("[LeaderboardManager] Attempting to connect to online service...");

            // TODO: Integrate with actual backend service
            // Example: PlayFab, Unity Gaming Services, Firebase, etc.

            // For now, simulate connection failure and use local fallback
            isConnected = false;

            if (!isConnected && useLocalFallback)
            {
                Debug.LogWarning("[LeaderboardManager] Online service unavailable, using local fallback");
            }
        }

        /// <summary>
        /// Submit score to online service
        /// PLACEHOLDER: Implement actual API call
        /// </summary>
        private void SubmitScoreOnline(int score)
        {
            Debug.Log($"[LeaderboardManager] Submitting score {score} to online service...");

            // TODO: Implement actual online submission
            // Example using PlayFab:
            // PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
            // {
            //     Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = leaderboardId, Value = score } }
            // }, OnScoreSubmitSuccess, OnScoreSubmitFailure);

            // For now, fallback to local
            SubmitScoreLocal(score);
        }

        /// <summary>
        /// Load leaderboard from online service
        /// PLACEHOLDER: Implement actual API call
        /// </summary>
        private void LoadLeaderboardOnline()
        {
            Debug.Log("[LeaderboardManager] Loading leaderboard from online service...");

            // TODO: Implement actual online loading
            // Example using PlayFab:
            // PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
            // {
            //     StatisticName = leaderboardId,
            //     StartPosition = 0,
            //     MaxResultsCount = maxEntries
            // }, OnLeaderboardLoadSuccess, OnLeaderboardLoadFailure);

            // For now, fallback to local
            LoadLocalLeaderboard();
            OnLeaderboardLoaded?.Invoke(globalLeaderboard);
            isLoading = false;
        }

        /// <summary>
        /// Load friends leaderboard from online service
        /// PLACEHOLDER: Implement actual API call
        /// </summary>
        private void LoadFriendsOnline()
        {
            Debug.Log("[LeaderboardManager] Loading friends leaderboard...");

            // TODO: Implement friends leaderboard
            // This requires friend system integration
        }

        #endregion

        #region Private Methods - Local Fallback

        /// <summary>
        /// Submit score to local leaderboard
        /// </summary>
        private void SubmitScoreLocal(int score)
        {
            Debug.Log($"[LeaderboardManager] Submitting score {score} to local leaderboard");

            // Create new entry
            LeaderboardEntry newEntry = new LeaderboardEntry
            {
                playerId = playerId,
                playerName = playerName,
                score = score,
                rank = 0,
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Remove old entry for this player
            globalLeaderboard.RemoveAll(e => e.playerId == playerId);

            // Add new entry
            globalLeaderboard.Add(newEntry);

            // Sort by score descending
            globalLeaderboard = globalLeaderboard.OrderByDescending(e => e.score).ToList();

            // Update ranks
            for (int i = 0; i < globalLeaderboard.Count; i++)
            {
                globalLeaderboard[i].rank = i + 1;
            }

            // Trim to max entries
            if (globalLeaderboard.Count > maxEntries)
            {
                globalLeaderboard = globalLeaderboard.Take(maxEntries).ToList();
            }

            // Save to PlayerPrefs
            SaveLocalLeaderboard();

            int rank = GetPlayerRank();
            OnScoreSubmitted?.Invoke(rank);

            isSubmitting = false;

            Debug.Log($"[LeaderboardManager] Score submitted. Player rank: {rank}");
        }

        /// <summary>
        /// Load local leaderboard from PlayerPrefs
        /// </summary>
        private void LoadLocalLeaderboard()
        {
            globalLeaderboard.Clear();

            for (int i = 0; i < maxEntries; i++)
            {
                string key = LOCAL_LEADERBOARD_KEY + i;
                if (!PlayerPrefs.HasKey(key)) break;

                string json = PlayerPrefs.GetString(key);
                LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(json);

                if (entry != null)
                {
                    globalLeaderboard.Add(entry);
                }
            }

            Debug.Log($"[LeaderboardManager] Loaded {globalLeaderboard.Count} local leaderboard entries");
        }

        /// <summary>
        /// Save local leaderboard to PlayerPrefs
        /// </summary>
        private void SaveLocalLeaderboard()
        {
            for (int i = 0; i < globalLeaderboard.Count; i++)
            {
                string key = LOCAL_LEADERBOARD_KEY + i;
                string json = JsonUtility.ToJson(globalLeaderboard[i]);
                PlayerPrefs.SetString(key, json);
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load player info from PlayerPrefs
        /// </summary>
        private void LoadPlayerInfo()
        {
            playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "Player");

            // Generate unique player ID if not exists
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = SystemInfo.deviceUniqueIdentifier;
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            // Debug display
            GUILayout.BeginArea(new Rect(Screen.width - 260, 330, 250, 150));
            GUILayout.Label($"<b>Leaderboard:</b> {(isConnected ? "Online" : "Local")}");
            GUILayout.Label($"<b>Entries:</b> {globalLeaderboard.Count}");

            int rank = GetPlayerRank();
            if (rank > 0)
            {
                GUILayout.Label($"<b>Your Rank:</b> #{rank}");
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Leaderboard entry data
    /// </summary>
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerId;
        public string playerName;
        public int score;
        public int rank;
        public string timestamp;
    }
}
