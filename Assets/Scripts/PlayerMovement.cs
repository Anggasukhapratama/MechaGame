using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;
    public float slideSpeed = 10f;
    public float slideDuration = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private PlayerController playerController;

    private Vector2 moveInput;
    private bool isJumping = false;
    private bool isSliding = false;
    private float slideTime;

    private enum MovementState { idle, run, jump, slide }

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();

        playerController = new PlayerController();
    }

    private void OnEnable()
    {
        playerController.Enable();

        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();
        playerController.Movement.Slide.performed += ctx => Slide();
    }

    private void OnDisable()
    {
        playerController.Disable();
    }

    private void Update()
    {
        moveInput = playerController.Movement.Move.ReadValue<Vector2>();

        if (isSliding)
        {
            slideTime -= Time.deltaTime;
            if (slideTime <= 0)
            {
                isSliding = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isSliding)
        {
            float speed = moveInput.magnitude > 0.7f ? runSpeed : moveSpeed;
            Vector2 targetVelocity = new Vector2(moveInput.x * speed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }
        
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        MovementState state;

        if (isSliding)
        {
            state = MovementState.slide;
        }
        else if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jump;
        }
        else if (moveInput.x != 0f)
        {
            state = MovementState.run;
            sprite.flipX = moveInput.x < 0f;
        }
        else
        {
            state = MovementState.idle;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private void Jump()
    {
        if (isGrounded() && !isSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void Slide()
    {
        if (isGrounded() && !isSliding && moveInput.x != 0)
        {
            isSliding = true;
            slideTime = slideDuration;
            rb.velocity = new Vector2(moveInput.x * slideSpeed, rb.velocity.y);
        }
    }
}