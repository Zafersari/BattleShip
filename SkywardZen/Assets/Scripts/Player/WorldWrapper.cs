using UnityEngine;

namespace SkywardZen.Player
{
    /// <summary>
    /// Implements screen wrapping in 3D space - when the player moves too far left,
    /// they appear on the right, and vice versa. Creates an "infinite" horizontal space.
    /// Classic arcade game mechanic adapted for modern 3D.
    /// </summary>
    public class WorldWrapper : MonoBehaviour
    {
        [Header("Wrap Boundaries")]
        [SerializeField] private float wrapBoundaryX = 10f;
        [SerializeField] private float wrapBoundaryZ = 5f;
        [Tooltip("Add extra distance to prevent visual popping")]
        [SerializeField] private float wrapBuffer = 0.5f;

        [Header("Options")]
        [SerializeField] private bool enableXWrapping = true;
        [SerializeField] private bool enableZWrapping = false;
        [SerializeField] private bool smoothTransition = true;
        [SerializeField] private float transitionDuration = 0.1f;

        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem wrapEffectPrefab;
        [SerializeField] private AudioClip wrapSound;

        // State
        private Vector3 lastPosition;
        private bool isWrapping;
        private float wrapTimer;

        // Components
        private Rigidbody rb;
        private AudioSource audioSource;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            lastPosition = transform.position;

            // Setup audio source for wrap sound
            if (wrapSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.clip = wrapSound;
            }
        }

        private void Update()
        {
            CheckAndWrap();

            if (isWrapping && smoothTransition)
            {
                wrapTimer += Time.deltaTime;
                if (wrapTimer >= transitionDuration)
                {
                    isWrapping = false;
                }
            }
        }

        private void CheckAndWrap()
        {
            if (isWrapping) return;

            Vector3 currentPos = transform.position;
            bool wrapped = false;

            // Check X-axis wrapping
            if (enableXWrapping)
            {
                if (currentPos.x > wrapBoundaryX + wrapBuffer)
                {
                    WrapToPosition(new Vector3(-wrapBoundaryX, currentPos.y, currentPos.z));
                    wrapped = true;
                }
                else if (currentPos.x < -wrapBoundaryX - wrapBuffer)
                {
                    WrapToPosition(new Vector3(wrapBoundaryX, currentPos.y, currentPos.z));
                    wrapped = true;
                }
            }

            // Check Z-axis wrapping
            if (enableZWrapping && !wrapped)
            {
                if (currentPos.z > wrapBoundaryZ + wrapBuffer)
                {
                    WrapToPosition(new Vector3(currentPos.x, currentPos.y, -wrapBoundaryZ));
                    wrapped = true;
                }
                else if (currentPos.z < -wrapBoundaryZ - wrapBuffer)
                {
                    WrapToPosition(new Vector3(currentPos.x, currentPos.y, wrapBoundaryZ));
                    wrapped = true;
                }
            }

            lastPosition = currentPos;
        }

        private void WrapToPosition(Vector3 newPosition)
        {
            // Preserve velocity when wrapping
            Vector3 velocity = rb != null ? rb.velocity : Vector3.zero;

            // Teleport to new position
            transform.position = newPosition;

            // Restore velocity
            if (rb != null)
            {
                rb.velocity = velocity;
            }

            // Visual/Audio feedback
            OnWrap(newPosition);

            // Set wrapping state
            if (smoothTransition)
            {
                isWrapping = true;
                wrapTimer = 0f;
            }
        }

        private void OnWrap(Vector3 position)
        {
            // Spawn wrap effect
            if (wrapEffectPrefab != null)
            {
                Instantiate(wrapEffectPrefab, position, Quaternion.identity);
            }

            // Play wrap sound
            if (audioSource != null && wrapSound != null)
            {
                audioSource.Play();
            }
        }

        #region Public API

        public void SetWrapBoundaries(float x, float z)
        {
            wrapBoundaryX = x;
            wrapBoundaryZ = z;
        }

        public void EnableWrapping(bool enableX, bool enableZ)
        {
            enableXWrapping = enableX;
            enableZWrapping = enableZ;
        }

        public Vector2 GetBoundaries() => new Vector2(wrapBoundaryX, wrapBoundaryZ);

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            // Draw wrap boundaries
            if (enableXWrapping)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                DrawBoundaryLine(Vector3.right * wrapBoundaryX, 20f);
                DrawBoundaryLine(Vector3.right * -wrapBoundaryX, 20f);
            }

            if (enableZWrapping)
            {
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                DrawBoundaryLine(Vector3.forward * wrapBoundaryZ, 20f);
                DrawBoundaryLine(Vector3.forward * -wrapBoundaryZ, 20f);
            }
        }

        private void DrawBoundaryLine(Vector3 position, float height)
        {
            Vector3 center = new Vector3(position.x, transform.position.y + height * 0.5f, position.z);
            Gizmos.DrawCube(center, new Vector3(0.1f, height, 0.1f));
        }

        #endregion
    }
}
