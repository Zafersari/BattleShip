using UnityEngine;

namespace SkywardZen.Platforms
{
    /// <summary>
    /// Moving platform that translates horizontally or in a circular pattern.
    /// Player can land on it and will move with the platform.
    /// </summary>
    public class MovingPlatform : Platform
    {
        [Header("Movement Settings")]
        [SerializeField] private MovementPattern movementPattern = MovementPattern.Horizontal;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 5f;

        [Header("Circular Movement (if pattern is Circular)")]
        [SerializeField] private float circleRadius = 3f;
        [SerializeField] private float rotationSpeed = 1f;

        // State
        private Vector3 startPosition;
        private float timeOffset;
        private Transform playerTransform;

        public enum MovementPattern
        {
            Horizontal,
            Circular,
            VerticalOscillation
        }

        protected override void Awake()
        {
            base.Awake();
            startPosition = transform.position;
            timeOffset = Random.Range(0f, 100f); // Random phase for variety
        }

        public override void Initialize()
        {
            base.Initialize();
            startPosition = transform.position;
            timeOffset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            float time = Time.time + timeOffset;

            switch (movementPattern)
            {
                case MovementPattern.Horizontal:
                    MoveHorizontal(time);
                    break;

                case MovementPattern.Circular:
                    MoveCircular(time);
                    break;

                case MovementPattern.VerticalOscillation:
                    MoveVertical(time);
                    break;
            }
        }

        private void MoveHorizontal(float time)
        {
            float offsetX = Mathf.Sin(time * moveSpeed) * moveRange;
            transform.position = startPosition + new Vector3(offsetX, 0f, 0f);
        }

        private void MoveCircular(float time)
        {
            float angle = time * rotationSpeed;
            float offsetX = Mathf.Cos(angle) * circleRadius;
            float offsetZ = Mathf.Sin(angle) * circleRadius;
            transform.position = startPosition + new Vector3(offsetX, 0f, offsetZ);
        }

        private void MoveVertical(float time)
        {
            float offsetY = Mathf.Sin(time * moveSpeed) * (moveRange * 0.5f);
            transform.position = startPosition + new Vector3(0f, offsetY, 0f);
        }

        private void OnCollisionStay(Collision collision)
        {
            // Make player move with platform
            if (collision.gameObject.CompareTag("Player"))
            {
                Vector3 platformVelocity = (transform.position - startPosition) / Time.deltaTime;

                Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    // Add platform's velocity to player (simulates being "carried")
                    Vector3 playerVel = playerRb.velocity;
                    playerVel.x += platformVelocity.x * 0.1f;
                    playerVel.z += platformVelocity.z * 0.1f;
                    playerRb.velocity = playerVel;
                }
            }

            startPosition = transform.position; // Update for next frame
        }

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            Vector3 center = Application.isPlaying ? startPosition : transform.position;

            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);

            switch (movementPattern)
            {
                case MovementPattern.Horizontal:
                    Gizmos.DrawLine(center - Vector3.right * moveRange, center + Vector3.right * moveRange);
                    break;

                case MovementPattern.Circular:
                    DrawCircle(center, circleRadius, 32);
                    break;

                case MovementPattern.VerticalOscillation:
                    Gizmos.DrawLine(center - Vector3.up * moveRange * 0.5f, center + Vector3.up * moveRange * 0.5f);
                    break;
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * angleStep * i;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        #endregion
    }
}
