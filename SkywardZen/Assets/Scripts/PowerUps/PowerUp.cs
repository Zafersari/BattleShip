using UnityEngine;
using UnityEngine.Events;

namespace SkywardZen.PowerUps
{
    /// <summary>
    /// Base class for all power-ups in the game
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class PowerUp : MonoBehaviour
    {
        [Header("Power-Up Settings")]
        [SerializeField] protected PowerUpType powerUpType;
        [SerializeField] protected float duration = 5f;
        [SerializeField] protected bool isCollected = false;

        [Header("Visual Settings")]
        [SerializeField] protected float rotationSpeed = 90f;
        [SerializeField] protected float bobSpeed = 2f;
        [SerializeField] protected float bobHeight = 0.3f;
        [SerializeField] protected GameObject visualModel;
        [SerializeField] protected ParticleSystem collectEffect;

        [Header("Audio")]
        [SerializeField] protected AudioClip collectSound;
        [SerializeField] protected AudioSource audioSource;

        [Header("Events")]
        public UnityEvent<PowerUpType> OnCollected;

        protected Vector3 startPosition;
        protected float bobTimer;

        #region Unity Lifecycle

        protected virtual void Start()
        {
            startPosition = transform.position;
            bobTimer = Random.Range(0f, Mathf.PI * 2); // Random start phase

            // Ensure trigger collider
            GetComponent<Collider>().isTrigger = true;

            // Setup audio source if needed
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        protected virtual void Update()
        {
            if (isCollected) return;

            // Rotate power-up
            if (visualModel != null)
            {
                visualModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            // Bob up and down
            bobTimer += Time.deltaTime * bobSpeed;
            Vector3 bobOffset = Vector3.up * Mathf.Sin(bobTimer) * bobHeight;
            transform.position = startPosition + bobOffset;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            // Check if player collected this
            if (other.CompareTag("Player"))
            {
                Collect(other.gameObject);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Collect the power-up
        /// </summary>
        public virtual void Collect(GameObject player)
        {
            if (isCollected) return;

            isCollected = true;

            Debug.Log($"[PowerUp] {powerUpType} collected!");

            // Visual feedback
            if (visualModel != null)
            {
                visualModel.SetActive(false);
            }

            if (collectEffect != null)
            {
                collectEffect.Play();
            }

            // Audio feedback
            if (audioSource != null && collectSound != null)
            {
                audioSource.PlayOneShot(collectSound);
            }

            // Notify PowerUpManager
            OnCollected?.Invoke(powerUpType);

            // Apply power-up effect
            ApplyEffect(player);

            // Add score
            if (Core.ScoreManager.Instance != null)
            {
                Core.ScoreManager.Instance.AddPowerUpScore();
            }

            // Destroy or return to pool after particle effect
            Destroy(gameObject, collectEffect != null ? collectEffect.main.duration : 0.5f);
        }

        /// <summary>
        /// Reset power-up for pooling
        /// </summary>
        public virtual void Reset()
        {
            isCollected = false;
            if (visualModel != null)
            {
                visualModel.SetActive(true);
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Apply the power-up effect to the player
        /// </summary>
        protected abstract void ApplyEffect(GameObject player);

        #endregion

        #region Getters

        public PowerUpType Type => powerUpType;
        public float Duration => duration;

        #endregion
    }

    /// <summary>
    /// Types of power-ups available
    /// </summary>
    public enum PowerUpType
    {
        DoubleJump,
        SlowMotion,
        Magnet,
        Shield,
        ScoreMultiplier
    }
}
