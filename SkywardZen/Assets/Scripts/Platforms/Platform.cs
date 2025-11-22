using UnityEngine;

namespace SkywardZen.Platforms
{
    /// <summary>
    /// Base class for all platform types. Handles common platform behavior
    /// and provides virtual methods for specific platform implementations.
    /// </summary>
    public class Platform : MonoBehaviour
    {
        [Header("Platform Properties")]
        [SerializeField] private Level.PlatformType platformType;
        [SerializeField] private bool canRespawn = true;

        [Header("Visual Feedback")]
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material activatedMaterial;
        [SerializeField] private ParticleSystem landingEffect;
        [SerializeField] private AudioClip landingSound;

        // State
        protected bool isActivated;
        protected AudioSource audioSource;
        protected MeshRenderer meshRenderer;

        public Level.PlatformType Type => platformType;

        protected virtual void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();

            // Setup audio
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && landingSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.5f; // Partial 3D sound
            }
        }

        public virtual void Initialize()
        {
            isActivated = false;

            if (meshRenderer != null && normalMaterial != null)
            {
                meshRenderer.material = normalMaterial;
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Only activate if player is coming from above
                if (collision.relativeVelocity.y > 0)
                {
                    OnPlayerLanded(collision);
                }
            }
        }

        protected virtual void OnPlayerLanded(Collision collision)
        {
            if (isActivated) return;

            isActivated = true;

            // Visual feedback
            if (meshRenderer != null && activatedMaterial != null)
            {
                meshRenderer.material = activatedMaterial;
            }

            // Particle effect
            if (landingEffect != null)
            {
                landingEffect.Play();
            }

            // Sound effect
            if (audioSource != null && landingSound != null)
            {
                audioSource.PlayOneShot(landingSound);
            }
        }

        public virtual void Reset()
        {
            Initialize();
        }
    }
}
