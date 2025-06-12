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

    [SerializeField] private PlayerHealth playerHealth;

    private float mobileInputX = 0f;
    private Vector2 moveInput;
    private bool isJumping = false;
    public bool isSliding = false;
    private float slideTime;

    private enum MovementState { idle = 0, run = 1, jump = 2, slide = 3, dead = 4, hurt = 5 }

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    private bool isPlayerDead = false;
    private bool isHurt = false;

    [SerializeField] private string hurtAnimationStateName = "Hurt";

    private MovementState currentState = MovementState.idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();

        playerController = new PlayerController();

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        playerController.Enable();

        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();
        playerController.Movement.Slide.performed += ctx => Slide();

        PlayerHealth.OnPlayerDied += HandlePlayerDeath;
        PlayerHealth.OnPlayerRespawned += HandlePlayerRespawn;
        PlayerHealth.OnPlayerHurt += HandlePlayerHurt;
    }

    private void OnDisable()
    {
        playerController.Disable();

        PlayerHealth.OnPlayerDied -= HandlePlayerDeath;
        PlayerHealth.OnPlayerRespawned -= HandlePlayerRespawn;
        PlayerHealth.OnPlayerHurt -= HandlePlayerHurt;
    }

    private void Update()
    {
        if (isPlayerDead || isHurt) return;

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
        if (isPlayerDead) return;

        if (isHurt)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else if (!isSliding)
        {
            float combinedHorizontalInput = moveInput.x + mobileInputX;
            float speed = Mathf.Abs(combinedHorizontalInput) > 0.7f ? runSpeed : moveSpeed;
            Vector2 targetVelocity = new Vector2(combinedHorizontalInput * speed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }

        UpdateAnimation();

        if (isGrounded() && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            isJumping = false;
        }
    }

    private void UpdateAnimation()
    {
        MovementState newState;

        float horizontal = moveInput.x + mobileInputX;

        if (!isPlayerDead && horizontal != 0f)
        {
            sprite.flipX = horizontal < 0f;
        }

        if (isPlayerDead)
        {
            newState = MovementState.dead;
        }
        else if (isHurt)
        {
            newState = MovementState.hurt;
        }
        else if (isSliding)
        {
            newState = MovementState.slide;
        }
        else if (!isGrounded())
        {
            newState = MovementState.jump;
        }
        else if (horizontal != 0f)
        {
            newState = MovementState.run;
        }
        else
        {
            newState = MovementState.idle;
        }

        if (newState != currentState)
        {
            anim.SetInteger("state", (int)newState);
            currentState = newState;
        }
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private void Jump()
    {
        if (isPlayerDead || isJumping || isSliding || isHurt) return;

        if (isGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }
    }

    private void Slide()
    {
        if (isPlayerDead || isHurt) return;

        float horizontal = moveInput.x + mobileInputX;

        if (isGrounded() && !isSliding && horizontal != 0)
        {
            isSliding = true;
            slideTime = slideDuration;
            rb.velocity = new Vector2(Mathf.Sign(horizontal) * slideSpeed, rb.velocity.y);
        }
    }

    public void MoveRight(bool isPressed)
    {
        if (isPlayerDead || isHurt) { mobileInputX = 0f; return; }
        if (isPressed)
            mobileInputX = 1f;
        else if (mobileInputX == 1f)
            mobileInputX = 0f;
    }

    public void MoveLeft(bool isPressed)
    {
        if (isPlayerDead || isHurt) { mobileInputX = 0f; return; }
        if (isPressed)
            mobileInputX = -1f;
        else if (mobileInputX == -1f)
            mobileInputX = 0f;
    }

    public void MobileJump()
    {
        if (isPlayerDead || isHurt) return;
        Jump();
    }

    public void MobileSlide()
    {
        if (isPlayerDead || isHurt) return;
        Slide();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isPlayerDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (isSliding)
                {
                    Debug.Log("Sliding into enemy! Enemy dies.");
                    enemy.Die();
                }
                else
                {
                    Debug.Log("Touched enemy normally. Player takes damage.");
                    playerHealth.TakeDamage(1);
                }
            }
        }
        else if (collision.gameObject.CompareTag("Trap"))
        {
            Debug.Log("Hit a trap! Player takes damage.");
            playerHealth.TakeDamage(1);
        }
    }

    private void HandlePlayerDeath()
    {
        isPlayerDead = true;
        currentState = MovementState.dead;
        anim.SetInteger("state", (int)currentState);
        Debug.Log("PlayerMovement: Menangani kematian. Mengatur state Dead.");

        playerController.Disable();
        mobileInputX = 0f;
        isSliding = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void HandlePlayerRespawn()
    {
        isPlayerDead = false;
        isHurt = false;
        playerController.Enable();

        currentState = MovementState.idle;
        anim.SetInteger("state", (int)currentState);

        Debug.Log("Player has respawned! Mengatur state Idle.");
    }

    private void HandlePlayerHurt()
    {
        if (isPlayerDead) return;

        Debug.Log("PlayerMovement: Player terkena damage, masuk state HURT.");

        isHurt = true;
        currentState = MovementState.hurt;
        anim.SetInteger("state", (int)currentState);
        StartCoroutine(ExitHurtStateAfterDelay());
    }

    private IEnumerator ExitHurtStateAfterDelay()
    {
        float hurtDuration = anim.GetCurrentAnimatorStateInfo(0).length;

        if (hurtDuration <= 0f) hurtDuration = 0.5f;

        yield return new WaitForSeconds(hurtDuration);

        isHurt = false;
        Debug.Log("PlayerMovement: Keluar dari state HURT.");
    }
}