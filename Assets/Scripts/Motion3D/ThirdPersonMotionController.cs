using UnityEngine;
using UnityEngine.Events;

namespace Jambudweep.Ramayana.Motion3D
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonMotionController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 2.6f;
        [SerializeField] private float runSpeed = 5.4f;
        [SerializeField] private float acceleration = 13.5f;
        [SerializeField] private float deceleration = 18f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float jumpHeight = 1.15f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.14f;
        [SerializeField] private float landingRecoveryDuration = 0.18f;
        [SerializeField] private float fallResetHeight = -4f;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private UnityEvent onEmbassyReached = new UnityEvent();

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private Vector3 verticalVelocity;
        private Vector2 mobileInput;
        private bool jumpQueued;
        private bool controlsLocked;
        private bool embassyReached;
        private bool wasGrounded = true;
        private float strideTimer;
        private float landingRecoveryTimer;
        private float groundedGraceTimer;
        private float jumpBufferTimer;
        private float currentSpeed;
        private Vector3 respawnPosition;

        public float MoveAmount { get; private set; }
        public float Speed01 { get; private set; }
        public string MovementState { get; private set; } = "Idle";
        public bool IsGrounded => controller == null || controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            respawnPosition = transform.position;
        }

        private void Update()
        {
            Vector2 keyboardInput = controlsLocked ? Vector2.zero : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 input = Vector2.ClampMagnitude(keyboardInput + mobileInput, 1f);
            bool runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool running = runHeld || input.magnitude > 0.72f;
            float speedBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.18f, 1f, input.magnitude));
            float speed = Mathf.Lerp(walkSpeed, runSpeed, running ? speedBlend : Mathf.Min(speedBlend, 0.52f));

            Vector3 cameraForward = cameraRoot == null ? Vector3.forward : cameraRoot.forward;
            Vector3 cameraRight = cameraRoot == null ? Vector3.right : cameraRoot.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 move = cameraForward * input.y + cameraRight * input.x;
            MoveAmount = Mathf.Clamp01(move.magnitude);
            Vector3 desiredVelocity = move.sqrMagnitude > 0.0025f ? move.normalized * speed * MoveAmount : Vector3.zero;
            float response = desiredVelocity.sqrMagnitude > horizontalVelocity.sqrMagnitude ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredVelocity, response * Time.deltaTime);
            currentSpeed = horizontalVelocity.magnitude;
            Speed01 = Mathf.InverseLerp(0f, runSpeed, currentSpeed);

            if (horizontalVelocity.sqrMagnitude > 0.0025f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            }

            bool grounded = controller.isGrounded;
            groundedGraceTimer = grounded ? coyoteTime : Mathf.Max(0f, groundedGraceTimer - Time.deltaTime);
            if (jumpQueued || Input.GetKeyDown(KeyCode.Space)) jumpBufferTimer = jumpBufferTime;
            else jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);

            if (grounded && !wasGrounded) landingRecoveryTimer = landingRecoveryDuration;
            if (grounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
            if (!controlsLocked && jumpBufferTimer > 0f && groundedGraceTimer > 0f)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                grounded = false;
                groundedGraceTimer = 0f;
                jumpBufferTimer = 0f;
            }
            jumpQueued = false;
            verticalVelocity.y += gravity * Time.deltaTime;
            landingRecoveryTimer = Mathf.Max(0f, landingRecoveryTimer - Time.deltaTime);

            controller.Move((horizontalVelocity + verticalVelocity) * Time.deltaTime);
            if (transform.position.y < fallResetHeight) ResetToSafePosition();
            UpdateMovementState(grounded, running);
            AnimateVisual(running);
            wasGrounded = controller.isGrounded;
        }

        public void SetMobileInput(Vector2 input)
        {
            if (controlsLocked) return;
            mobileInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void QueueJump()
        {
            if (!controlsLocked) jumpQueued = true;
        }

        public void LockControls()
        {
            controlsLocked = true;
            mobileInput = Vector2.zero;
            MoveAmount = 0f;
            horizontalVelocity = Vector3.zero;
            currentSpeed = 0f;
            Speed01 = 0f;
            MovementState = "Locked";
        }

        /// <summary>
        /// Unlock controls. Public counterpart to <see cref="LockControls"/>.
        /// Called by MotionTriggeredDialogue after a non-motion-allowing line completes.
        /// </summary>
        public void UnlockControlsPublic()
        {
            controlsLocked = false;
            MovementState = "Idle";
        }

        /// <summary>Wire the camera used for camera-relative WASD (GTA feel).</summary>
        public void SetCameraRoot(Transform cam)
        {
            cameraRoot = cam;
        }

        private void ResetToSafePosition()
        {
            controller.enabled = false;
            transform.position = respawnPosition;
            controller.enabled = true;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = Vector3.zero;
            mobileInput = Vector2.zero;
            MoveAmount = 0f;
            Speed01 = 0f;
            MovementState = "Reset";
        }

        private void UpdateMovementState(bool grounded, bool running)
        {
            if (controlsLocked)
            {
                MovementState = "Locked";
                return;
            }

            if (!grounded)
            {
                MovementState = verticalVelocity.y > 0f ? "Jump" : "Fall";
            }
            else if (landingRecoveryTimer > 0f)
            {
                MovementState = "Land";
            }
            else if (currentSpeed < 0.08f)
            {
                MovementState = "Idle";
            }
            else
            {
                MovementState = running ? "Run" : "Walk";
            }
        }

        private void AnimateVisual(bool running)
        {
            if (visualRoot == null) return;
            strideTimer += Time.deltaTime * Mathf.Lerp(5f, 10.5f, Speed01) * (running ? 1.2f : 1f);
            float bob = Speed01 <= 0.01f ? 0f : Mathf.Sin(strideTimer) * Mathf.Lerp(0.018f, 0.045f, Speed01);
            float lean = Speed01 <= 0.01f ? 0f : Mathf.Sin(strideTimer * 0.5f) * Mathf.Lerp(1.2f, 2.6f, Speed01);
            float landingDuration = Mathf.Max(0.01f, landingRecoveryDuration);
            float landingDip = landingRecoveryTimer <= 0f ? 0f : -Mathf.Sin((landingRecoveryTimer / landingDuration) * Mathf.PI) * 0.035f;
            visualRoot.localPosition = new Vector3(0f, bob + landingDip, 0f);
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, -lean);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Finish") || embassyReached) return;
            embassyReached = true;
            LockControls();
            onEmbassyReached.Invoke();
        }
    }
}
