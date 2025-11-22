using UnityEngine;

namespace SkywardZen.PowerUps
{
    /// <summary>
    /// Power-up that slows down time for easier platforming
    /// </summary>
    public class SlowMotionPowerUp : PowerUp
    {
        [Header("Slow Motion Settings")]
        [SerializeField] private float timeScale = 0.5f;

        protected override void Start()
        {
            base.Start();
            powerUpType = PowerUpType.SlowMotion;
        }

        protected override void ApplyEffect(GameObject player)
        {
            // Notify PowerUpManager to apply effect
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivatePowerUp(PowerUpType.SlowMotion, duration, timeScale);
            }

            Debug.Log($"[SlowMotion] Time slowed to {timeScale}x for {duration} seconds!");
        }
    }
}
