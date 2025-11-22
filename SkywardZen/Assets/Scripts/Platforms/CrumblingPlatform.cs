using UnityEngine;
using System.Collections;

namespace SkywardZen.Platforms
{
    /// <summary>
    /// Platform that crumbles and disappears after the player lands on it.
    /// Features visual/audio feedback and can optionally respawn after a delay.
    /// </summary>
    public class CrumblingPlatform : Platform
    {
        [Header("Crumble Settings")]
        [SerializeField] private float crumbleDelay = 0.3f;
        [SerializeField] private float crumbleDuration = 0.5f;
        [SerializeField] private bool respawnAfterCrumble = false;
        [SerializeField] private float respawnDelay = 3f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem crumbleEffect;
        [SerializeField] private AudioClip crumbleSound;
        [SerializeField] private bool shakeBeforeCrumble = true;
        [SerializeField] private float shakeIntensity = 0.1f;

        [Header("Glass Shader Settings (Optional)")]
        [SerializeField] private bool useDissolveMaterial = true;
        [SerializeField] private string dissolvePropertyName = "_DissolveAmount";

        // State
        private bool isCrumbling;
        private Vector3 originalPosition;
        private Collider platformCollider;
        private MaterialPropertyBlock propertyBlock;

        protected override void Awake()
        {
            base.Awake();
            originalPosition = transform.localPosition;
            platformCollider = GetComponent<Collider>();
            propertyBlock = new MaterialPropertyBlock();
        }

        public override void Initialize()
        {
            base.Initialize();

            isCrumbling = false;
            transform.localPosition = originalPosition;

            // Reset collider
            if (platformCollider != null)
            {
                platformCollider.enabled = true;
            }

            // Reset material
            if (meshRenderer != null && useDissolveMaterial)
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(dissolvePropertyName, 0f);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }

            gameObject.SetActive(true);
        }

        protected override void OnPlayerLanded(Collision collision)
        {
            base.OnPlayerLanded(collision);

            if (!isCrumbling)
            {
                StartCoroutine(CrumbleSequence());
            }
        }

        private IEnumerator CrumbleSequence()
        {
            isCrumbling = true;

            // Shake warning
            if (shakeBeforeCrumble)
            {
                yield return StartCoroutine(ShakePlatform());
            }

            // Wait before crumbling
            yield return new WaitForSeconds(crumbleDelay);

            // Play crumble sound
            if (audioSource != null && crumbleSound != null)
            {
                audioSource.PlayOneShot(crumbleSound);
            }

            // Spawn crumble particles
            if (crumbleEffect != null)
            {
                crumbleEffect.Play();
            }

            // Disable collision immediately
            if (platformCollider != null)
            {
                platformCollider.enabled = false;
            }

            // Visual crumble effect
            yield return StartCoroutine(VisualCrumble());

            // Deactivate platform
            gameObject.SetActive(false);

            // Optional respawn
            if (respawnAfterCrumble)
            {
                yield return new WaitForSeconds(respawnDelay);
                Initialize();
            }
        }

        private IEnumerator ShakePlatform()
        {
            float elapsed = 0f;
            float shakeDuration = crumbleDelay * 0.5f;

            while (elapsed < shakeDuration)
            {
                float intensity = Mathf.Lerp(0f, shakeIntensity, elapsed / shakeDuration);
                Vector3 randomOffset = new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity) * 0.5f,
                    Random.Range(-intensity, intensity)
                );

                transform.localPosition = originalPosition + randomOffset;

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
        }

        private IEnumerator VisualCrumble()
        {
            float elapsed = 0f;

            while (elapsed < crumbleDuration)
            {
                float t = elapsed / crumbleDuration;

                if (useDissolveMaterial && meshRenderer != null)
                {
                    // Dissolve effect
                    meshRenderer.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetFloat(dissolvePropertyName, t);
                    meshRenderer.SetPropertyBlock(propertyBlock);
                }
                else
                {
                    // Fallback: Scale down effect
                    transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Reset scale for next spawn
            transform.localScale = Vector3.one;
        }

        public override void Reset()
        {
            StopAllCoroutines();
            Initialize();
        }
    }
}
