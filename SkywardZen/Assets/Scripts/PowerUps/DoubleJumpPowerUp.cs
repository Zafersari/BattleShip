using UnityEngine;

namespace SkywardZen.PowerUps
{
    /// <summary>
    /// Power-up that grants the player a double jump ability
    /// </summary>
    public class DoubleJumpPowerUp : PowerUp
    {
        [Header("Double Jump Settings")]
        [SerializeField] private int extraJumps = 1;

        protected override void Start()
        {
            base.Start();
            powerUpType = PowerUpType.DoubleJump;
        }

        protected override void ApplyEffect(GameObject player)
        {
            // Notify PowerUpManager to apply effect
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivatePowerUp(PowerUpType.DoubleJump, duration, extraJumps);
            }

            Debug.Log($"[DoubleJump] Granted {extraJumps} extra jump(s) for {duration} seconds!");
        }
    }
}
