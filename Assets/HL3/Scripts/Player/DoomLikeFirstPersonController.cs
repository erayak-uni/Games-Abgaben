using UnityEngine;
using UnityEngine.InputSystem;

namespace HL3.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class DoomLikeFirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private LayerMask wallMask = ~0;
        [SerializeField] private LayerMask grappleMask = ~0;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8.5f;
        [SerializeField] private float sprintSpeed = 12f;
        [SerializeField] private float airControl = 0.55f;
        [SerializeField] private float gravity = -28f;
        [SerializeField] private float groundedStickForce = -3f;

        [Header("Jump / Dash")]
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float jumpHeight = 2.2f;
        [SerializeField] private float dashSpeed = 24f;
        [SerializeField] private float dashDuration = 0.16f;
        [SerializeField] private float dashCooldown = 0.55f;

        [Header("Grapple")]
        [SerializeField] private float grappleRange = 35f;
        [SerializeField] private float grapplePullSpeed = 24f;
        [SerializeField] private float grappleReleaseDistance = 2.2f;

        [Header("Wall Movement")]
        [SerializeField] private float wallCheckDistance = 0.85f;
        [SerializeField] private float wallSlideGravity = -2.5f;
        [SerializeField] private float wallClimbSpeed = 4f;
        [SerializeField] private float wallJumpUpVelocity = 10f;
        [SerializeField] private float wallJumpAwayVelocity = 10f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private Vector3 externalVelocity;
        private Vector3 dashDirection;
        private Vector3 wallNormal;
        private Vector3 grapplePoint;
        private float verticalVelocity;
        private float pitch;
        private float dashTimer;
        private float nextDashTime;
        private int jumpsUsed;
        private bool isWallHolding;
        private bool isGrappling;

        public bool IsGrappling => isGrappling;
        public bool IsWallHolding => isWallHolding;
        public Vector3 GrapplePoint => grapplePoint;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            ResolveCameraRoot();
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null)
            {
                return;
            }

            Look();
            UpdateGroundedState();
            UpdateWallHold();
            UpdateGrapple();
            UpdateDash();
            UpdateJump();
            MoveCharacter();
        }

        public void Launch(Vector3 velocity)
        {
            externalVelocity = velocity;
            verticalVelocity = Mathf.Max(verticalVelocity, velocity.y);
            jumpsUsed = 0;
            isWallHolding = false;
            isGrappling = false;
        }

        private void Look()
        {
            if (cameraRoot == null)
            {
                ResolveCameraRoot();
            }

            Vector2 look = Mouse.current.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(Vector3.up, look.x);
            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
            if (cameraRoot != null)
            {
                cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void ResolveCameraRoot()
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                cameraRoot = childCamera.transform;
                childCamera.tag = "MainCamera";
                return;
            }

            if (Camera.main != null)
            {
                cameraRoot = Camera.main.transform;
            }
        }

        private void UpdateGroundedState()
        {
            if (controller.isGrounded)
            {
                jumpsUsed = 0;
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = groundedStickForce;
                }
            }
        }

        private void UpdateJump()
        {
            if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                return;
            }

            if (isWallHolding)
            {
                isWallHolding = false;
                verticalVelocity = wallJumpUpVelocity;
                horizontalVelocity = wallNormal * wallJumpAwayVelocity;
                jumpsUsed = 1;
                return;
            }

            if (jumpsUsed >= maxJumps)
            {
                return;
            }

            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsUsed++;
            isGrappling = false;
        }

        private void UpdateDash()
        {
            if (dashTimer > 0f)
            {
                dashTimer -= Time.deltaTime;
                return;
            }

            if (Keyboard.current.leftShiftKey.wasPressedThisFrame && Time.time >= nextDashTime)
            {
                dashDirection = GetWishDirection();
                if (dashDirection.sqrMagnitude < 0.05f)
                {
                    dashDirection = transform.forward;
                }

                dashTimer = dashDuration;
                nextDashTime = Time.time + dashCooldown;
                isWallHolding = false;
            }
        }

        private void UpdateGrapple()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame && cameraRoot != null)
            {
                Ray ray = new Ray(cameraRoot.position, cameraRoot.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, grappleRange, grappleMask, QueryTriggerInteraction.Ignore))
                {
                    grapplePoint = hit.point;
                    isGrappling = true;
                    isWallHolding = false;
                }
            }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                isGrappling = false;
            }

            if (isGrappling && Vector3.Distance(transform.position, grapplePoint) <= grappleReleaseDistance)
            {
                isGrappling = false;
            }
        }

        private void UpdateWallHold()
        {
            bool wantsWallHold = Keyboard.current.eKey.isPressed;
            isWallHolding = false;

            if (controller.isGrounded || !wantsWallHold)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * controller.height * 0.45f;
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, wallCheckDistance, wallMask, QueryTriggerInteraction.Ignore))
            {
                isWallHolding = true;
                isGrappling = false;
                wallNormal = hit.normal;
                verticalVelocity = Mathf.Max(verticalVelocity, wallSlideGravity);
            }
        }

        private void MoveCharacter()
        {
            Vector3 wishDirection = GetWishDirection();
            float targetSpeed = Keyboard.current.leftCtrlKey.isPressed ? sprintSpeed : moveSpeed;
            float control = controller.isGrounded ? 1f : airControl;
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, wishDirection * targetSpeed, control * 12f * Time.deltaTime);

            if (isWallHolding)
            {
                float climb = 0f;
                if (Keyboard.current.wKey.isPressed) climb += 1f;
                if (Keyboard.current.sKey.isPressed) climb -= 1f;
                verticalVelocity = climb * wallClimbSpeed;
            }
            else if (isGrappling)
            {
                Vector3 pull = (grapplePoint - transform.position).normalized * grapplePullSpeed;
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, new Vector3(pull.x, 0f, pull.z), 14f * Time.deltaTime);
                verticalVelocity = Mathf.Lerp(verticalVelocity, pull.y, 14f * Time.deltaTime);
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity + externalVelocity;
            if (dashTimer > 0f)
            {
                velocity += dashDirection * dashSpeed;
            }

            controller.Move(velocity * Time.deltaTime);
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, 4f * Time.deltaTime);
        }

        private Vector3 GetWishDirection()
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;

            Vector3 direction = transform.forward * input.y + transform.right * input.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.TryGetComponent(out World.Trampoline trampoline))
            {
                Launch(trampoline.GetLaunchVelocity(transform.position));
            }
        }
    }
}
