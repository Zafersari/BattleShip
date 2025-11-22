using UnityEngine;

namespace SkywardZen.Camera
{
    /// <summary>
    /// Camera controller that follows the player vertically with smooth damping.
    /// Only moves upward - never moves down. Implements a dead zone to prevent jitter.
    /// Use this OR Cinemachine (see documentation for Cinemachine alternative).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool useFixedUpdate = false;

        [Header("Dead Zone (Prevents Jitter)")]
        [SerializeField] private float deadZoneHeight = 2f;
        [Tooltip("Camera only moves when player is this far above camera's target")]
        [SerializeField] private float activationThreshold = 1f;

        [Header("Vertical Lock")]
        [SerializeField] private bool lockToPlayerX = false;
        [SerializeField] private bool lockToPlayerZ = false;

        // State
        private float highestY;
        private Vector3 targetPosition;
        private Vector3 velocity = Vector3.zero;

        private void Start()
        {
            if (player == null)
            {
                Debug.LogError("CameraController: Player reference is missing!");
                enabled = false;
                return;
            }

            // Initialize camera position
            highestY = player.position.y;
            targetPosition = CalculateTargetPosition();
            transform.position = targetPosition;
        }

        private void Update()
        {
            if (!useFixedUpdate)
            {
                UpdateCamera();
            }
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate)
            {
                UpdateCamera();
            }
        }

        private void LateUpdate()
        {
            // Always execute in LateUpdate for smoothest tracking
            if (!useFixedUpdate)
            {
                ApplyCameraPosition();
            }
        }

        private void UpdateCamera()
        {
            TrackHighestPoint();
            CalculateTargetWithDeadZone();
        }

        private void TrackHighestPoint()
        {
            // Only update highest point if player goes higher
            if (player.position.y > highestY)
            {
                highestY = player.position.y;
            }
        }

        private void CalculateTargetWithDeadZone()
        {
            // Calculate ideal target position
            Vector3 idealTarget = CalculateTargetPosition();

            // Apply dead zone logic
            float distanceAboveCamera = player.position.y - (transform.position.y - offset.y);

            if (distanceAboveCamera > deadZoneHeight + activationThreshold)
            {
                // Player is above dead zone - move camera up
                targetPosition = idealTarget;
            }
            else
            {
                // Player is in dead zone - keep current Y but may update X/Z
                targetPosition = new Vector3(
                    lockToPlayerX ? idealTarget.x : targetPosition.x,
                    targetPosition.y,
                    lockToPlayerZ ? idealTarget.z : targetPosition.z
                );
            }

            // Ensure camera never moves down
            if (targetPosition.y < transform.position.y)
            {
                targetPosition.y = transform.position.y;
            }
        }

        private Vector3 CalculateTargetPosition()
        {
            return new Vector3(
                lockToPlayerX ? player.position.x + offset.x : offset.x,
                highestY + offset.y,
                lockToPlayerZ ? player.position.z + offset.z : offset.z
            );
        }

        private void ApplyCameraPosition()
        {
            // Smooth damp for silky movement
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                1f / smoothSpeed
            );
        }

        #region Public API

        public void ResetCamera()
        {
            highestY = player.position.y;
            targetPosition = CalculateTargetPosition();
            transform.position = targetPosition;
            velocity = Vector3.zero;
        }

        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        public float GetHighestY() => highestY;

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            if (player == null) return;

            // Draw dead zone
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Vector3 deadZoneCenter = transform.position - new Vector3(0f, offset.y, 0f);
            Gizmos.DrawCube(deadZoneCenter, new Vector3(5f, deadZoneHeight * 2f, 5f));

            // Draw activation threshold
            Gizmos.color = Color.yellow;
            Vector3 thresholdPos = deadZoneCenter + Vector3.up * (deadZoneHeight + activationThreshold);
            Gizmos.DrawWireCube(thresholdPos, new Vector3(5f, 0.2f, 5f));

            // Draw target position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }

        #endregion
    }
}
