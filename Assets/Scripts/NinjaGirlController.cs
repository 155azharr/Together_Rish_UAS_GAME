using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class NinjaGirlController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float longJumpForce = 16f;
    [SerializeField] private float longJumpForcePerUpgrade = 1.5f;
    [SerializeField] private float longJumpHorizontalSpeed = 6.5f;
    [SerializeField] private int longJumpsPerCheckpoint = 3;
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private Vector2 cameraOffset = new Vector2(0f, 0.25f);
    [SerializeField] private float cameraSmoothTime = 0.18f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Camera followCamera;
    private float horizontalInput;
    private bool jumpRequested;
    private bool longJumpRequested;
    private bool longJumpInProgress;
    private float longJumpDirection = 1f;
    private int jumpUpgradeLevel;
    private int longJumpCharges;
    private Vector3 cameraVelocity;

    public int JumpUpgradeLevel => jumpUpgradeLevel;
    public int LongJumpCharges => longJumpCharges;
    public int LongJumpsPerCheckpoint => longJumpsPerCheckpoint;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        followCamera = Camera.main;
    }

    public void ResetJumpUpgrades()
    {
        jumpUpgradeLevel = 0;
        longJumpCharges = 0;
        longJumpInProgress = false;
    }

    public bool UnlockLongJump(int upgradeLevel)
    {
        if (upgradeLevel <= jumpUpgradeLevel)
            return false;

        jumpUpgradeLevel = upgradeLevel;
        longJumpCharges = longJumpsPerCheckpoint;
        return true;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            horizontalInput = 0f;
        }
        else
        {
            bool moveLeft = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool moveRight = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            horizontalInput = (moveRight ? 1f : 0f) - (moveLeft ? 1f : 0f);

            bool jumpPressed = keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.wKey.wasPressedThisFrame ||
                keyboard.upArrowKey.wasPressedThisFrame;
            bool longJumpModifier = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            if (jumpPressed)
            {
                jumpRequested = true;
                longJumpRequested = longJumpModifier;
            }
        }

        if (Mathf.Abs(horizontalInput) > 0.01f)
            spriteRenderer.flipX = horizontalInput < 0f;

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetBool("Grounded", body.IsTouchingLayers(groundLayer));
        }
    }

    private void FixedUpdate()
    {
        bool grounded = body.IsTouchingLayers(groundLayer);
        if (!longJumpInProgress)
            body.linearVelocity = new Vector2(horizontalInput * moveSpeed, body.linearVelocity.y);

        if (jumpRequested && grounded)
        {
            longJumpInProgress = longJumpRequested && jumpUpgradeLevel > 0 && longJumpCharges > 0;
            if (longJumpInProgress)
            {
                longJumpCharges--;
                longJumpDirection = Mathf.Abs(horizontalInput) > 0.01f
                    ? Mathf.Sign(horizontalInput)
                    : spriteRenderer.flipX ? -1f : 1f;
                float upgradedForce = longJumpForce +
                    longJumpForcePerUpgrade * (jumpUpgradeLevel - 1);
                body.linearVelocity = new Vector2(longJumpDirection * longJumpHorizontalSpeed, upgradedForce);
            }
            else
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            }
        }

        if (longJumpInProgress)
        {
            if (Mathf.Abs(horizontalInput) > 0.01f)
                longJumpDirection = Mathf.Sign(horizontalInput);

            body.linearVelocity = new Vector2(longJumpDirection * longJumpHorizontalSpeed, body.linearVelocity.y);
            if (grounded && body.linearVelocity.y <= 0f)
                longJumpInProgress = false;
        }

        jumpRequested = false;
        longJumpRequested = false;
    }

    private void LateUpdate()
    {
        if (followCamera == null)
            followCamera = Camera.main;
        if (followCamera == null)
            return;

        Vector3 cameraPosition = followCamera.transform.position;
        Vector3 targetPosition = new Vector3(
            transform.position.x + cameraOffset.x,
            transform.position.y + cameraOffset.y,
            cameraPosition.z);
        followCamera.transform.position = Vector3.SmoothDamp(
            cameraPosition, targetPosition, ref cameraVelocity, Mathf.Max(0.01f, cameraSmoothTime));
    }
}
