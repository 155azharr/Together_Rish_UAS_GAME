using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Minimal network-owned 2D movement sample.
/// Add NetworkObject and NetworkTransform to the player prefab too.
/// NetworkTransform synchronizes the owner's position to other clients.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class NetworkCatPlayer : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;

    private Rigidbody2D body;
    private bool jumpRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetButtonDown("Jump"))
            jumpRequested = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || body == null) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        body.linearVelocity = new Vector2(horizontal * moveSpeed, body.linearVelocity.y);

        bool grounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpRequested && grounded)
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);

        jumpRequested = false;
    }
}
