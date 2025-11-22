using UnityEngine;

namespace SkywardZen.PowerUps
{
    /// <summary>
    /// Power-up that attracts nearby platforms or collectibles
    /// </summary>
    public class MagnetPowerUp : PowerUp
    {
        [Header("Magnet Settings")]
        [SerializeField] private float magnetRange = 5f;
        [SerializeField] private float attractionForce = 10f;

        protected override void Start()
        {
            base.Start();
            powerUpType = PowerUpType.Magnet;
        }

        protected override void ApplyEffect(GameObject player)
        {
            // Notify PowerUpManager to apply effect
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivatePowerUp(PowerUpType.Magnet, duration, magnetRange);
            }

            Debug.Log($"[Magnet] Activated with {magnetRange}m range for {duration} seconds!");
        }
    }
}
