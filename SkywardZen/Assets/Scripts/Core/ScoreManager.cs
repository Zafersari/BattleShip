using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace SkywardZen.Core
{
    /// <summary>
    /// Manages scoring, high scores, and persistent score tracking
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Current Score")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int currentHeight = 0;
        [SerializeField] private int platformsLanded = 0;

        [Header("Score Multipliers")]
        [SerializeField] private int heightPointsPerMeter = 10;
        [SerializeField] private int platformLandingPoints = 5;
        [SerializeField] private int movingPlatformBonus = 10;
        [SerializeField] private int crumblingPlatformBonus = 15;
        [SerializeField] private int powerUpCollectionPoints = 50;

        [Header("High Scores")]
        [SerializeField] private int highScore = 0;
        [SerializeField] private int topHighScoreCount = 10; // Store top 10 scores
        private List<int> highScores = new List<int>();

        [Header("Events")]
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int> OnHeightChanged;
        public UnityEvent<int> OnNewHighScore;

        [Header("Persistence")]
        private const string HIGH_SCORE_KEY = "HighScore";
        private const string HIGH_SCORES_KEY = "HighScores_";
        private const string TOTAL_SCORE_KEY = "TotalLifetimeScore";

        private int totalLifetimeScore = 0;

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

            LoadHighScores();
        }

        private void OnEnable()
        {
            // Subscribe to game state events
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStart.AddListener(ResetScore);
                GameStateManager.Instance.OnGameOver.AddListener(SaveScore);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStart.RemoveListener(ResetScore);
                GameStateManager.Instance.OnGameOver.RemoveListener(SaveScore);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add score based on height achieved
        /// </summary>
        public void AddHeightScore(int heightInMeters)
        {
            if (heightInMeters <= currentHeight) return; // Only count upward progress

            int heightGained = heightInMeters - currentHeight;
            int points = heightGained * heightPointsPerMeter;

            currentHeight = heightInMeters;
            AddScore(points);

            OnHeightChanged?.Invoke(currentHeight);
        }

        /// <summary>
        /// Add score for landing on a platform
        /// </summary>
        public void AddPlatformLandingScore(PlatformType platformType = PlatformType.Static)
        {
            platformsLanded++;
            int points = platformLandingPoints;

            // Bonus points for special platform types
            switch (platformType)
            {
                case PlatformType.Moving:
                    points += movingPlatformBonus;
                    break;
                case PlatformType.Crumbling:
                    points += crumblingPlatformBonus;
                    break;
            }

            AddScore(points);
        }

        /// <summary>
        /// Add score for collecting a power-up
        /// </summary>
        public void AddPowerUpScore()
        {
            AddScore(powerUpCollectionPoints);
        }

        /// <summary>
        /// Add custom score amount
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);

            Debug.Log($"[ScoreManager] Score: {currentScore} (+{points})");
        }

        /// <summary>
        /// Reset score to zero for new game
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            currentHeight = 0;
            platformsLanded = 0;

            OnScoreChanged?.Invoke(currentScore);
            OnHeightChanged?.Invoke(currentHeight);

            Debug.Log("[ScoreManager] Score reset for new game");
        }

        /// <summary>
        /// Save current score and check for high score
        /// </summary>
        public void SaveScore()
        {
            // Update lifetime score
            totalLifetimeScore += currentScore;
            PlayerPrefs.SetInt(TOTAL_SCORE_KEY, totalLifetimeScore);

            // Check for high score
            bool isNewHighScore = currentScore > highScore;

            if (isNewHighScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
                OnNewHighScore?.Invoke(highScore);

                Debug.Log($"[ScoreManager] NEW HIGH SCORE: {highScore}!");
            }

            // Add to high scores list
            AddToHighScoresList(currentScore);

            PlayerPrefs.Save();

            Debug.Log($"[ScoreManager] Score saved: {currentScore} (High Score: {highScore})");
        }

        /// <summary>
        /// Get the all-time high score
        /// </summary>
        public int GetHighScore()
        {
            return highScore;
        }

        /// <summary>
        /// Get list of top high scores
        /// </summary>
        public List<int> GetHighScores()
        {
            return new List<int>(highScores);
        }

        /// <summary>
        /// Get current score
        /// </summary>
        public int GetCurrentScore()
        {
            return currentScore;
        }

        /// <summary>
        /// Get current height
        /// </summary>
        public int GetCurrentHeight()
        {
            return currentHeight;
        }

        /// <summary>
        /// Get total lifetime score
        /// </summary>
        public int GetTotalLifetimeScore()
        {
            return totalLifetimeScore;
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Load high scores from PlayerPrefs
        /// </summary>
        private void LoadHighScores()
        {
            // Load single high score
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

            // Load high scores list
            highScores.Clear();
            for (int i = 0; i < topHighScoreCount; i++)
            {
                int score = PlayerPrefs.GetInt(HIGH_SCORES_KEY + i, 0);
                if (score > 0)
                {
                    highScores.Add(score);
                }
            }

            // Load lifetime score
            totalLifetimeScore = PlayerPrefs.GetInt(TOTAL_SCORE_KEY, 0);

            Debug.Log($"[ScoreManager] High scores loaded. Best: {highScore}, Total: {totalLifetimeScore}");
        }

        /// <summary>
        /// Add score to high scores list and save
        /// </summary>
        private void AddToHighScoresList(int score)
        {
            // Add new score
            highScores.Add(score);

            // Sort descending
            highScores = highScores.OrderByDescending(s => s).ToList();

            // Keep only top N scores
            if (highScores.Count > topHighScoreCount)
            {
                highScores = highScores.Take(topHighScoreCount).ToList();
            }

            // Save to PlayerPrefs
            for (int i = 0; i < highScores.Count; i++)
            {
                PlayerPrefs.SetInt(HIGH_SCORES_KEY + i, highScores[i]);
            }
        }

        /// <summary>
        /// Clear all saved high scores (for testing)
        /// </summary>
        public void ClearHighScores()
        {
            PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
            PlayerPrefs.DeleteKey(TOTAL_SCORE_KEY);

            for (int i = 0; i < topHighScoreCount; i++)
            {
                PlayerPrefs.DeleteKey(HIGH_SCORES_KEY + i);
            }

            highScore = 0;
            highScores.Clear();
            totalLifetimeScore = 0;

            PlayerPrefs.Save();

            Debug.Log("[ScoreManager] All high scores cleared!");
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            // Debug display
            GUILayout.BeginArea(new Rect(10, 60, 250, 200));
            GUILayout.Label($"<b>Score:</b> {currentScore}");
            GUILayout.Label($"<b>Height:</b> {currentHeight}m");
            GUILayout.Label($"<b>Platforms:</b> {platformsLanded}");
            GUILayout.Label($"<b>High Score:</b> {highScore}");
            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Platform types for score calculation
    /// </summary>
    public enum PlatformType
    {
        Static,
        Moving,
        Crumbling
    }
}
