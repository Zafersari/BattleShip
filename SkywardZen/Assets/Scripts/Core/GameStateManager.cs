using UnityEngine;
using UnityEngine.Events;

namespace SkywardZen.Core
{
    /// <summary>
    /// Manages the overall game state (Menu, Playing, Paused, GameOver)
    /// Singleton pattern for global access
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Menu;

        [Header("Events")]
        public UnityEvent OnGameStart;
        public UnityEvent OnGamePause;
        public UnityEvent OnGameResume;
        public UnityEvent OnGameOver;
        public UnityEvent OnRestart;

        [Header("Settings")]
        [SerializeField] private bool startInMenuState = true;
        [SerializeField] private float timeScale = 1f;

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

            // Initialize state
            if (startInMenuState)
            {
                currentState = GameState.Menu;
                Time.timeScale = 0f; // Pause game in menu
            }
        }

        private void Start()
        {
            // If starting in menu, wait for player to start
            if (startInMenuState)
            {
                Debug.Log("[GameStateManager] Game ready. Waiting in Menu state.");
            }
            else
            {
                // Auto-start if not in menu
                StartGame();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the game from menu or restart
        /// </summary>
        public void StartGame()
        {
            if (currentState == GameState.Playing)
            {
                Debug.LogWarning("[GameStateManager] Game is already running!");
                return;
            }

            Debug.Log("[GameStateManager] Starting game...");

            currentState = GameState.Playing;
            Time.timeScale = timeScale;

            OnGameStart?.Invoke();
        }

        /// <summary>
        /// Pause the active game
        /// </summary>
        public void PauseGame()
        {
            if (currentState != GameState.Playing)
            {
                Debug.LogWarning("[GameStateManager] Cannot pause - game is not playing!");
                return;
            }

            Debug.Log("[GameStateManager] Pausing game...");

            savedTimeScale = Time.timeScale;
            currentState = GameState.Paused;
            Time.timeScale = 0f;

            OnGamePause?.Invoke();
        }

        /// <summary>
        /// Resume from pause
        /// </summary>
        public void ResumeGame()
        {
            if (currentState != GameState.Paused)
            {
                Debug.LogWarning("[GameStateManager] Cannot resume - game is not paused!");
                return;
            }

            Debug.Log("[GameStateManager] Resuming game...");

            currentState = GameState.Playing;
            Time.timeScale = savedTimeScale;

            OnGameResume?.Invoke();
        }

        /// <summary>
        /// Trigger game over state
        /// </summary>
        public void GameOver()
        {
            if (currentState == GameState.GameOver)
            {
                Debug.LogWarning("[GameStateManager] Game is already over!");
                return;
            }

            Debug.Log("[GameStateManager] Game Over!");

            currentState = GameState.GameOver;
            Time.timeScale = 0f; // Stop game

            OnGameOver?.Invoke();
        }

        /// <summary>
        /// Restart the game from beginning
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("[GameStateManager] Restarting game...");

            OnRestart?.Invoke();

            // Reset time scale
            Time.timeScale = timeScale;

            // Transition to playing state
            currentState = GameState.Playing;

            // Invoke start event for other systems to reset
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            Debug.Log("[GameStateManager] Returning to menu...");

            currentState = GameState.Menu;
            Time.timeScale = 0f;

            OnRestart?.Invoke(); // Let other systems reset
        }

        /// <summary>
        /// Quit the application
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameStateManager] Quitting game...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        #endregion

        #region Getters

        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsGameOver => currentState == GameState.GameOver;
        public bool IsInMenu => currentState == GameState.Menu;

        #endregion

        #region Debug

        private void OnGUI()
        {
            // Debug display in top-left corner
            GUILayout.BeginArea(new Rect(10, 10, 250, 150));
            GUILayout.Label($"<b>Game State:</b> {currentState}");
            GUILayout.Label($"<b>Time Scale:</b> {Time.timeScale:F2}");
            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Possible game states
    /// </summary>
    public enum GameState
    {
        Menu,       // In main menu
        Playing,    // Active gameplay
        Paused,     // Game paused
        GameOver    // Game ended
    }
}
