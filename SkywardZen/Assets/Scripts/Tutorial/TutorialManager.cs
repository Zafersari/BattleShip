using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace SkywardZen.Tutorial
{
    /// <summary>
    /// Manages tutorial system for first-time players
    /// Shows contextual hints and guides player through core mechanics
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Tutorial Settings")]
        [SerializeField] private bool enableTutorial = true;
        [SerializeField] private bool skipForReturningPlayers = true;
        [SerializeField] private float hintDisplayDuration = 3f;

        [Header("Tutorial Steps")]
        [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        [SerializeField] private int currentStepIndex = 0;

        [Header("UI References")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private Text tutorialText;
        [SerializeField] private GameObject tutorialArrow;

        [Header("Events")]
        public UnityEvent OnTutorialStarted;
        public UnityEvent OnTutorialCompleted;
        public UnityEvent<string> OnStepCompleted;

        [Header("Progress Tracking")]
        [SerializeField] private bool hasCompletedTutorial = false;
        [SerializeField] private int jumpsPerformed = 0;
        [SerializeField] private int platformsLanded = 0;
        [SerializeField] private int powerUpsCollected = 0;

        private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";
        private bool isTutorialActive = false;
        private float stepStartTime = 0f;

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

            LoadTutorialProgress();
            InitializeTutorialSteps();
        }

        private void Start()
        {
            // Check if tutorial should be shown
            if (enableTutorial && !hasCompletedTutorial)
            {
                StartTutorial();
            }
            else
            {
                if (tutorialPanel != null)
                    tutorialPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isTutorialActive) return;

            UpdateCurrentStep();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the tutorial
        /// </summary>
        public void StartTutorial()
        {
            if (!enableTutorial) return;

            Debug.Log("[TutorialManager] Starting tutorial...");

            isTutorialActive = true;
            currentStepIndex = 0;

            // Reset progress
            jumpsPerformed = 0;
            platformsLanded = 0;
            powerUpsCollected = 0;

            // Show tutorial UI
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);

            OnTutorialStarted?.Invoke();

            ShowCurrentStep();
        }

        /// <summary>
        /// Complete the tutorial
        /// </summary>
        public void CompleteTutorial()
        {
            Debug.Log("[TutorialManager] Tutorial completed!");

            isTutorialActive = false;
            hasCompletedTutorial = true;

            // Save completion
            PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();

            // Hide tutorial UI
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            OnTutorialCompleted?.Invoke();
        }

        /// <summary>
        /// Skip tutorial
        /// </summary>
        public void SkipTutorial()
        {
            Debug.Log("[TutorialManager] Tutorial skipped");
            CompleteTutorial();
        }

        /// <summary>
        /// Reset tutorial progress (for testing)
        /// </summary>
        public void ResetTutorial()
        {
            hasCompletedTutorial = false;
            currentStepIndex = 0;
            PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();

            Debug.Log("[TutorialManager] Tutorial progress reset");
        }

        /// <summary>
        /// Notify tutorial of player jump
        /// </summary>
        public void OnPlayerJump()
        {
            jumpsPerformed++;
            CheckStepCompletion();
        }

        /// <summary>
        /// Notify tutorial of platform landing
        /// </summary>
        public void OnPlayerLandedOnPlatform()
        {
            platformsLanded++;
            CheckStepCompletion();
        }

        /// <summary>
        /// Notify tutorial of power-up collection
        /// </summary>
        public void OnPlayerCollectedPowerUp()
        {
            powerUpsCollected++;
            CheckStepCompletion();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize tutorial steps
        /// </summary>
        private void InitializeTutorialSteps()
        {
            if (tutorialSteps.Count > 0) return; // Already set in inspector

            // Create default tutorial steps
            tutorialSteps.Add(new TutorialStep
            {
                stepName = "Welcome",
                message = "Welcome to Skyward Zen! Swipe left or right to move.",
                triggerType = TutorialTriggerType.Time,
                requiredTime = 2f
            });

            tutorialSteps.Add(new TutorialStep
            {
                stepName = "FirstJump",
                message = "Jump on platforms to climb higher!",
                triggerType = TutorialTriggerType.LandOnPlatforms,
                requiredCount = 3
            });

            tutorialSteps.Add(new TutorialStep
            {
                stepName = "KeepClimbing",
                message = "Keep climbing! Don't fall off the screen.",
                triggerType = TutorialTriggerType.LandOnPlatforms,
                requiredCount = 10
            });

            tutorialSteps.Add(new TutorialStep
            {
                stepName = "MovingPlatforms",
                message = "Watch out for moving platforms! They're worth more points.",
                triggerType = TutorialTriggerType.Time,
                requiredTime = 3f
            });

            tutorialSteps.Add(new TutorialStep
            {
                stepName = "PowerUps",
                message = "Collect power-ups for special abilities!",
                triggerType = TutorialTriggerType.CollectPowerUp,
                requiredCount = 1
            });

            tutorialSteps.Add(new TutorialStep
            {
                stepName = "Complete",
                message = "Great job! You're ready to play. Good luck!",
                triggerType = TutorialTriggerType.Time,
                requiredTime = 2f
            });

            Debug.Log($"[TutorialManager] Initialized {tutorialSteps.Count} tutorial steps");
        }

        /// <summary>
        /// Show current tutorial step
        /// </summary>
        private void ShowCurrentStep()
        {
            if (currentStepIndex >= tutorialSteps.Count)
            {
                CompleteTutorial();
                return;
            }

            TutorialStep step = tutorialSteps[currentStepIndex];
            stepStartTime = Time.time;

            // Show message
            if (tutorialText != null)
            {
                tutorialText.text = step.message;
            }

            // Position arrow if needed
            if (tutorialArrow != null && step.arrowPosition != Vector3.zero)
            {
                tutorialArrow.SetActive(true);
                tutorialArrow.transform.position = step.arrowPosition;
            }
            else if (tutorialArrow != null)
            {
                tutorialArrow.SetActive(false);
            }

            Debug.Log($"[TutorialManager] Showing step {currentStepIndex + 1}/{tutorialSteps.Count}: {step.stepName}");
        }

        /// <summary>
        /// Update current tutorial step
        /// </summary>
        private void UpdateCurrentStep()
        {
            if (currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep step = tutorialSteps[currentStepIndex];

            // Check if step should complete based on time
            if (step.triggerType == TutorialTriggerType.Time)
            {
                float elapsed = Time.time - stepStartTime;
                if (elapsed >= step.requiredTime)
                {
                    CompleteCurrentStep();
                }
            }
        }

        /// <summary>
        /// Check if current step is completed based on player actions
        /// </summary>
        private void CheckStepCompletion()
        {
            if (!isTutorialActive) return;
            if (currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep step = tutorialSteps[currentStepIndex];
            bool shouldComplete = false;

            switch (step.triggerType)
            {
                case TutorialTriggerType.Jump:
                    shouldComplete = jumpsPerformed >= step.requiredCount;
                    break;

                case TutorialTriggerType.LandOnPlatforms:
                    shouldComplete = platformsLanded >= step.requiredCount;
                    break;

                case TutorialTriggerType.CollectPowerUp:
                    shouldComplete = powerUpsCollected >= step.requiredCount;
                    break;
            }

            if (shouldComplete)
            {
                CompleteCurrentStep();
            }
        }

        /// <summary>
        /// Complete current step and move to next
        /// </summary>
        private void CompleteCurrentStep()
        {
            if (currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep step = tutorialSteps[currentStepIndex];

            Debug.Log($"[TutorialManager] Completed step: {step.stepName}");

            OnStepCompleted?.Invoke(step.stepName);

            currentStepIndex++;

            if (currentStepIndex < tutorialSteps.Count)
            {
                ShowCurrentStep();
            }
            else
            {
                CompleteTutorial();
            }
        }

        /// <summary>
        /// Load tutorial progress from PlayerPrefs
        /// </summary>
        private void LoadTutorialProgress()
        {
            hasCompletedTutorial = PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;

            Debug.Log($"[TutorialManager] Tutorial completed: {hasCompletedTutorial}");
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!isTutorialActive) return;

            // Debug display
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, 10, 300, 100));
            GUILayout.Label($"<b>Tutorial Step:</b> {currentStepIndex + 1}/{tutorialSteps.Count}");

            if (currentStepIndex < tutorialSteps.Count)
            {
                TutorialStep step = tutorialSteps[currentStepIndex];
                GUILayout.Label($"<b>Goal:</b> {step.stepName}");
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Individual tutorial step
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public string stepName;
        [TextArea(2, 4)]
        public string message;
        public TutorialTriggerType triggerType;
        public int requiredCount = 1;
        public float requiredTime = 3f;
        public Vector3 arrowPosition = Vector3.zero;
    }

    /// <summary>
    /// Types of tutorial triggers
    /// </summary>
    public enum TutorialTriggerType
    {
        Time,               // Complete after X seconds
        Jump,               // Complete after X jumps
        LandOnPlatforms,    // Complete after landing on X platforms
        CollectPowerUp,     // Complete after collecting X power-ups
        ReachHeight,        // Complete after reaching X height
        Manual              // Requires manual completion
    }
}
