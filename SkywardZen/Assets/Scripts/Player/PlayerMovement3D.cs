using UnityEngine;

namespace SkywardZen.Player
{
    /// <summary>
    /// Core player movement controller with physics-based jumping,
    /// touch/mouse drag controls, and procedural squash & stretch animation.
    /// Designed for a modern 3D vertical platformer.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement3D : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float horizontalSpeed = 10f;
        [SerializeField] private float maxHorizontalSpeed = 8f;
        [SerializeField] private float horizontalDrag = 5f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float autoJumpOnLanding = true;

        [Header("Squash & Stretch")]
        [SerializeField] private float stretchAmount = 0.3f;
        [SerializeField] private float squashAmount = 0.4f;
        [SerializeField] private float squashStretchSpeed = 8f;
        [SerializeField] private AnimationCurve squashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Control Settings")]
        [SerializeField] private bool useTiltControls = false;
        [SerializeField] private float tiltSensitivity = 3f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask platformLayer;
        [SerializeField] private float groundCheckDistance = 0.6f;

        // Components
        private Rigidbody rb;
        private Vector3 baseScale;

        // State
        private bool isGrounded;
        private bool wasGrounded;
        private float currentSquashStretch = 0f;

        // Input
        private Vector2 touchStartPos;
        private bool isDragging;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            baseScale = transform.localScale;

            // Configure rigidbody for optimal physics
            rb.useGravity = false; // We'll handle gravity manually for more control
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void Update()
        {
            HandleInput();
            ApplySquashAndStretch();
        }

        private void FixedUpdate()
        {
            ApplyGravity();
            ApplyHorizontalMovement();
            CheckGroundState();

            // Auto-jump when landing on platform
            if (isGrounded && !wasGrounded && autoJumpOnLanding)
            {
                Jump();
            }

            wasGrounded = isGrounded;
        }

        #region Input Handling

        private void HandleInput()
        {
            if (useTiltControls)
            {
                HandleTiltControls();
            }
            else
            {
                HandleTouchDragControls();
            }
        }

        private void HandleTouchDragControls()
        {
            // Mouse/Touch drag controls
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                isDragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }

        private void HandleTiltControls()
        {
            // Accelerometer-based tilt controls
            Vector3 tilt = Input.acceleration;
            float targetVelocityX = tilt.x * tiltSensitivity;

            Vector3 velocity = rb.velocity;
            velocity.x = Mathf.Lerp(velocity.x, targetVelocityX * maxHorizontalSpeed, Time.deltaTime * horizontalSpeed);
            rb.velocity = velocity;
        }

        #endregion

        #region Physics & Movement

        private void ApplyGravity()
        {
            // Apply custom gravity for more control over feel
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }

        private void ApplyHorizontalMovement()
        {
            if (useTiltControls) return; // Tilt controls handle movement in HandleTiltControls()

            float horizontalInput = 0f;

            if (isDragging)
            {
                // Calculate horizontal movement based on drag distance
                Vector2 currentTouchPos = Input.mousePosition;
                float dragDelta = (currentTouchPos.x - touchStartPos.x) / Screen.width;
                horizontalInput = dragDelta * horizontalSpeed;
            }

            // Apply horizontal force
            Vector3 horizontalForce = new Vector3(horizontalInput, 0f, 0f);
            rb.AddForce(horizontalForce, ForceMode.Acceleration);

            // Clamp horizontal velocity
            Vector3 velocity = rb.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
            rb.velocity = velocity;

            // Apply horizontal drag for natural deceleration
            if (!isDragging)
            {
                velocity.x = Mathf.Lerp(velocity.x, 0f, horizontalDrag * Time.fixedDeltaTime);
                rb.velocity = velocity;
            }
        }

        private void CheckGroundState()
        {
            // Raycast downward to check if we're on a platform
            RaycastHit hit;
            isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                out hit,
                groundCheckDistance,
                platformLayer
            );

            // Only consider grounded if moving downward or stationary
            if (isGrounded && rb.velocity.y > 0.1f)
            {
                isGrounded = false;
            }
        }

        public void Jump()
        {
            // Reset vertical velocity and apply jump force
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.velocity = velocity;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        #endregion

        #region Squash & Stretch Animation

        private void ApplySquashAndStretch()
        {
            float targetSquashStretch = 0f;

            // Calculate target based on velocity
            float verticalVelocity = rb.velocity.y;

            if (verticalVelocity > 1f)
            {
                // Stretching while going up
                float normalizedVelocity = Mathf.Clamp01(verticalVelocity / jumpForce);
                targetSquashStretch = squashCurve.Evaluate(normalizedVelocity) * stretchAmount;
            }
            else if (verticalVelocity < -1f)
            {
                // Squashing while falling
                float normalizedVelocity = Mathf.Clamp01(-verticalVelocity / Mathf.Abs(jumpForce));
                targetSquashStretch = -squashCurve.Evaluate(normalizedVelocity) * squashAmount;
            }
            else if (!isGrounded)
            {
                // Neutral when at apex of jump
                targetSquashStretch = 0f;
            }
            else
            {
                // Slight squash when grounded
                targetSquashStretch = -squashAmount * 0.2f;
            }

            // Smoothly interpolate current squash/stretch
            currentSquashStretch = Mathf.Lerp(
                currentSquashStretch,
                targetSquashStretch,
                squashStretchSpeed * Time.deltaTime
            );

            // Apply to transform scale
            // Y axis stretches, X and Z squash (volume preservation)
            float yScale = 1f + currentSquashStretch;
            float xzScale = 1f - (currentSquashStretch * 0.5f); // Compensate to preserve volume

            transform.localScale = new Vector3(
                baseScale.x * xzScale,
                baseScale.y * yScale,
                baseScale.z * xzScale
            );
        }

        #endregion

        #region Public API

        public bool IsGrounded => isGrounded;
        public float VerticalVelocity => rb.velocity.y;
        public Vector3 Velocity => rb.velocity;

        public void SetHorizontalSpeed(float speed)
        {
            horizontalSpeed = speed;
        }

        public void SetJumpForce(float force)
        {
            jumpForce = force;
        }

        public void ToggleTiltControls(bool enabled)
        {
            useTiltControls = enabled;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            // Visualize ground check
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        }

        #endregion
    }
}
