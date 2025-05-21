using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    #region Ability Unlock System
    [Header("Ability Unlock System")]
    [SerializeField] private bool canDoubleJumpUnlocked = false;
    [SerializeField] private bool wallJumpUnlocked = false;
    [SerializeField] private bool sprintUnlocked = false;
    [SerializeField] private bool dashUnlocked = false;
    [SerializeField] private bool slideUnlocked = false;
    #endregion

    #region Movement Parameters
    [Header("Force Movement Parameters")]
    [SerializeField] private float walkForce = 90.0f;
    [SerializeField] private float sprintForce = 120.0f;
    [SerializeField] private float walkForceApplyLimit = 18.0f;
    [SerializeField] private float airControl = 0.6f;
    [SerializeField] private float airForceApplyLimit = 15.0f;
    [SerializeField] private float stoppingForce = 100.0f;
    [SerializeField] private bool applyStoppingForceWhenActivelyBraking = true;
    [SerializeField] private float dragConstant = 1.0f;
    [SerializeField] private float frictionConstant = 2.0f;
    [SerializeField] private float velocityCleanupThreshold = 0.01f; 
    #endregion

    #region Jump Parameters
    [Header("Jump Parameters")]
    [SerializeField] private float jumpVelocity = 32.0f;
    [SerializeField] private float jumpCutVelocity = 10.0f;
    [SerializeField] private float minAllowedJumpCutVelocity = 18.0f;
    [SerializeField] private float groundedToleranceTime = 0.1f;
    [SerializeField] private float jumpCacheTime = 0.1f;
    [SerializeField] private float doubleJumpVelocity = 30.0f;
    [SerializeField] private float horizontalJumpBoostFactor = 0.2f;
    [SerializeField] private bool resetVerticalSpeedOnJumpIfMovingDown = true;
    [SerializeField] private float gravity = 50.0f;
    [SerializeField] private bool applyGravityOnGround = true;
    #endregion

    #region Wall Jump Parameters
    [Header("Wall Jump Parameters")]
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private float wallJumpXVelocity = 15f;
    [SerializeField] private float wallJumpYVelocity = 18f;
    [SerializeField] private float wallJumpTime = 0.15f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.5f;
    #endregion

    #region Dash Parameters
    [Header("Dash Parameters")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    #endregion

    #region Slide Parameters
    [Header("Slide Parameters")]
    [SerializeField] private float slideSpeed = 20f;
    [SerializeField] private float slideSpeedBoost = 1.5f;
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private float slideCooldown = 0.5f;
    [SerializeField] private Vector2 standingColliderSize = new Vector2(0.8f, 1.8f);
    [SerializeField] private Vector2 slidingColliderSize = new Vector2(1.6f, 0.9f);
    #endregion

    #region Ground Check Parameters
    [Header("Ground Check Parameters")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    #endregion

    // Component References
    private Rigidbody2D rb;
    private CapsuleCollider2D playerCollider;
    private Animator animator;

    // States
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isJumping;
    private bool jumpCutPossible;
    private bool canDoubleJump;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool isDashing;
    private bool isSliding;
    private bool canDash = true;
    private bool canSlide = true;
    private float slideDirection;
    private float currentSlideTimer;

    // Input cache system
    private bool jumpInputIsCached;
    private float lastJumpPressedTime;
    private float lastJumpTime;
    private float lastGroundedTime;
    private float wallJumpCounter;
    private float dashCooldownCounter;
    private float slideCooldownCounter;

    // Input cache
    private Vector2 inputMovement;
    private bool jumpInputReleased;
    private bool sprintInput;
    private bool dashInput;
    private bool slideInput;
    private Vector2 lastGroundedNormal = Vector2.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        GetInputs();
        CheckGrounded();
        
        if (wallJumpUnlocked)
        {
            CheckWallSliding();
        }
        else
        {
            isWallSliding = false;
            isWallJumping = false;
        }
        
        UpdateTimers();
        UpdateAnimations();
    }
    
    private void FixedUpdate()
    {
        if (TryJump()) 
        {
            UpdateWithVelocity(rb.velocity);
            return;
        }
        
        UpdateJumpCut();
        
        if (isSliding)
        {
            HandleSlideMovement();
            return;
        }
        
        if (!isDashing && !isWallJumping)
        {
            Move();
        }
        
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        
        if (!isWallJumping && !isSliding && Mathf.Abs(inputMovement.x) > 0.01f)
        {
            if (inputMovement.x > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (inputMovement.x < 0 && isFacingRight)
            {
                Flip();
            }
        }
        
        CleanupVelocity();
    }

    #region Input Methods
    private void GetInputs()
    {
        inputMovement.x = Input.GetAxisRaw("Horizontal");
        inputMovement.y = Input.GetAxisRaw("Vertical");
        
        // Only enable sprint input if sprint is unlocked
        sprintInput = sprintUnlocked && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            lastJumpPressedTime = Time.time;
            jumpInputIsCached = true;
        }
        
        if (jumpInputIsCached && Time.time - lastJumpPressedTime >= jumpCacheTime)
        {
            jumpInputIsCached = false;
        }
        
        jumpInputReleased = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow);
        
        // Only capture dash input if dash is unlocked
        dashInput = dashUnlocked && Input.GetKeyDown(KeyCode.E);
        
        // Only capture slide input if slide is unlocked
        slideInput = slideUnlocked && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl));
        
        if (isSliding)
        {
            float oppositeDirection = -slideDirection;
            if (Mathf.Sign(inputMovement.x) == Mathf.Sign(oppositeDirection) && Mathf.Abs(inputMovement.x) > 0.5f)
            {
                StopSlide();
            }
        }
        
        if (dashInput && canDash && !isGrounded && !isWallSliding && !isSliding)
        {
            StartCoroutine(DashCoroutine());
        }
        
        if (slideInput && canSlide && isGrounded && !isSliding)
        {
            StartCoroutine(SlideCoroutine());
        }
    }
    #endregion

    #region Movement Methods
    private void Move()
    {
        Vector2 currentVel = rb.velocity;
        Vector2 inputForce = GetInputForce();
        Vector2 gravityForce = GetGravity();
        Vector2 dragForce = -0.5f * (currentVel.sqrMagnitude) * dragConstant * currentVel.normalized;
        Vector2 summedForce = inputForce + gravityForce + dragForce;
        Vector2 newVel = currentVel + summedForce * Time.fixedDeltaTime;
        
        if (isGrounded)
        {
            newVel += GetStoppingForce(newVel);
            newVel += GetFriction(newVel, summedForce);
        }
        
        UpdateWithVelocity(newVel);
    }

    private Vector2 GetInputForce()
    {
        Vector2 inputDirection = new Vector2(inputMovement.x, 0);
        float forceAmount = sprintInput ? sprintForce : walkForce;
        
        if (!isGrounded)
        {
            forceAmount *= airControl;
        }
        
        Vector2 force = inputDirection * forceAmount;
        return ClampInputForce(force, rb.velocity);
    }

    private Vector2 ClampInputForce(Vector2 inputForce, Vector2 currentVelocity)
    {
        if (inputForce.sqrMagnitude < 0.01f)
            return Vector2.zero;
        
        float velocityLimit = isGrounded ? walkForceApplyLimit : airForceApplyLimit;
        float dot = Vector2.Dot(inputForce.normalized, currentVelocity);
        
        if (dot > velocityLimit)
        {
            return Vector2.zero;
        }
        
        return inputForce;
    }

    private Vector2 GetStoppingForce(Vector2 velocity)
    {
        if (velocity.sqrMagnitude < 0.01f)
            return Vector2.zero;
        
        bool shouldApplyStoppingForce = Mathf.Abs(inputMovement.x) < 0.01f || 
            (applyStoppingForceWhenActivelyBraking && 
             Mathf.Sign(inputMovement.x) != Mathf.Sign(velocity.x));
        
        if (!shouldApplyStoppingForce)
            return Vector2.zero;
        
        Vector2 direction = -velocity.normalized;
        Vector2 maxForceSpeedChange = direction * stoppingForce * Time.fixedDeltaTime;
        
        float currentSpeedInDir = Mathf.Abs(Vector2.Dot(velocity, direction));
        if (currentSpeedInDir < maxForceSpeedChange.magnitude)
        {
            return direction * currentSpeedInDir;
        }
        
        return maxForceSpeedChange;
    }

    private Vector2 GetFriction(Vector2 velocity, Vector2 currentForce)
    {
        if (!isGrounded || velocity.sqrMagnitude < 0.01f)
            return Vector2.zero;
        
        Vector2 direction = -velocity.normalized;
        Vector2 maxFrictionSpeedChange = direction * frictionConstant * Time.fixedDeltaTime;
        
        float currentSpeedInDir = Mathf.Abs(Vector2.Dot(velocity, direction)); 
        if (currentSpeedInDir < maxFrictionSpeedChange.magnitude)
        {
            return direction * currentSpeedInDir;
        }
        
        return maxFrictionSpeedChange;
    }

    private Vector2 GetGravity()
    {
        if (isGrounded && !applyGravityOnGround)
            return Vector2.zero;
            
        return Vector2.down * gravity;
    }

    private void Flip()
    {
        if (isSliding) return;
        
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
    
    private void UpdateWithVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }
    
    private void CleanupVelocity()
    {
        Vector2 velocity = rb.velocity;
        
        if (Mathf.Abs(velocity.x) < velocityCleanupThreshold)
        {
            velocity.x = 0;
        }
        
        if (Mathf.Abs(velocity.y) < velocityCleanupThreshold && isGrounded)
        {
            velocity.y = 0;
        }
        
        rb.velocity = velocity;
    }
    #endregion

    #region Jump Methods
    private bool TryJump()
    {
        // Wall Jump logic - only if wall jump is unlocked
        if (wallJumpUnlocked && isWallSliding && jumpInputIsCached)
        {
            isWallJumping = true;
            wallJumpCounter = wallJumpTime;
            
            float wallJumpDirection = isFacingRight ? -1f : 1f;
            Vector2 wallJumpVelocity = new Vector2(wallJumpDirection * wallJumpXVelocity, wallJumpYVelocity);
            
            animator.SetTrigger("wallJump");
            Jump(wallJumpVelocity, true);
            
            // Can only double jump after wall jump if double jump is unlocked
            canDoubleJump = canDoubleJumpUnlocked;
            Flip();
            
            return true;
        }
        
        // Normal jump logic
        if (jumpInputIsCached && (isGrounded || Time.time - lastGroundedTime <= groundedToleranceTime) && !DidJustJump())
        {
            Vector2 currentVelocity = rb.velocity;
            
            if (resetVerticalSpeedOnJumpIfMovingDown && currentVelocity.y < 0)
            {
                currentVelocity.y = 0;
            }
            
            float maxHorizontalInfluence = 2.0f;
            float horizontalBoost = Mathf.Clamp(currentVelocity.x * horizontalJumpBoostFactor, -maxHorizontalInfluence, maxHorizontalInfluence);
            Vector2 jumpVel = new Vector2(horizontalBoost, jumpVelocity);
            
            Jump(jumpVel, false);
            return true;
        }
        
        // Double jump logic - only if double jump is unlocked
        if (canDoubleJumpUnlocked && jumpInputIsCached && canDoubleJump && !isGrounded && !isWallSliding)
        {
            canDoubleJump = false;
            animator.SetTrigger("doubleJump"); 
            Vector2 doubleJumpVel = new Vector2(rb.velocity.x, doubleJumpVelocity);
            
            Jump(doubleJumpVel, true);
            return true;
        }
        
        return false;
    }

    private void Jump(Vector2 jumpVelocity, bool overridePreviousVelocity)
    {
        jumpCutPossible = true;
        jumpInputIsCached = false;
        lastJumpTime = Time.time;
        isJumping = true;
        
        if (isSliding)
        {
            StopSlide();
        }
        
        if (overridePreviousVelocity)
        {
            rb.velocity = jumpVelocity;
        }
        else
        {
            float preservedHorizontalVelocity = rb.velocity.x * 0.8f;
            rb.velocity = new Vector2(preservedHorizontalVelocity, 0) + jumpVelocity;
        }
    }

    private void UpdateJumpCut()
    {
        if (jumpCutPossible && jumpInputReleased && rb.velocity.y > 0)
        {
            if (rb.velocity.y > minAllowedJumpCutVelocity)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpCutVelocity);
                jumpCutPossible = false;
            }
        }
        
        if (jumpCutPossible && rb.velocity.y <= jumpCutVelocity)
        {
            jumpCutPossible = false;
        }
    }
    #endregion

    #region Wall Jump Methods
    private void CheckWallSliding()
    {
        if (!wallJumpUnlocked)
        {
            isWallSliding = false;
            return;
        }
        
        bool isTouchingWall = Physics2D.Raycast(transform.position, 
                                              isFacingRight ? Vector2.right : Vector2.left, 
                                              wallCheckDistance, 
                                              wallLayer);
        
        bool wasWallSliding = isWallSliding;                                 
        isWallSliding = isTouchingWall && !isGrounded && Mathf.Abs(inputMovement.x) > 0;
        
        if (!wasWallSliding && isWallSliding)
        {
            // Only enable double jump after wall slide if double jump is unlocked
            canDoubleJump = canDoubleJumpUnlocked;
        }
        
        if (isWallJumping)
        {
            wallJumpCounter -= Time.deltaTime;
            if (wallJumpCounter <= 0)
            {
                isWallJumping = false;
            }
        }
    }
    #endregion

    #region Dash Methods
    private IEnumerator DashCoroutine()
    {
        if (!dashUnlocked)
        {
            yield break;
        }
        
        isDashing = true;
        canDash = false;
        dashCooldownCounter = dashCooldown;
        
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        
        float dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0);
        
        yield return new WaitForSeconds(dashDuration);
        
        isDashing = false;
        rb.gravityScale = originalGravity;
    }
    #endregion

    #region Slide Methods
    private IEnumerator SlideCoroutine()
    {
        if (!slideUnlocked)
        {
            yield break;
        }
        
        isSliding = true;
        canSlide = false;
        slideCooldownCounter = slideCooldown;
        currentSlideTimer = 0f;
        
        slideDirection = isFacingRight ? 1f : -1f;
        
        playerCollider.size = slidingColliderSize;
        playerCollider.offset = new Vector2(0, -0.45f);
        
        float currentSpeedInSlideDirection = Mathf.Abs(Vector2.Dot(rb.velocity, new Vector2(slideDirection, 0)));
        float boostedSlideSpeed = currentSpeedInSlideDirection > slideSpeed ? 
                                 currentSpeedInSlideDirection * slideSpeedBoost : 
                                 slideSpeed;
        
        rb.velocity = new Vector2(slideDirection * boostedSlideSpeed, rb.velocity.y);
        
        while (currentSlideTimer < slideDuration && isSliding)
        {
            currentSlideTimer += Time.deltaTime;
            
            if (Physics2D.Raycast(transform.position, new Vector2(slideDirection, 0), 0.6f, wallLayer))
            {
                break;
            }
            
            yield return null;
        }
        
        StopSlide();
    }
    
    private void HandleSlideMovement()
    {
        Vector2 currentVelocity = rb.velocity;
        
        float slowdownRate = 4.0f;
        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, 0, slowdownRate * Time.fixedDeltaTime);
        currentVelocity.y += GetGravity().y * Time.fixedDeltaTime;
        
        rb.velocity = currentVelocity;
    }
    
    private void StopSlide()
    {
        if (!isSliding) return;
        
        isSliding = false;
        
        playerCollider.size = standingColliderSize;
        playerCollider.offset = new Vector2(0, 0);
    }
    #endregion

    #region State Check Methods
    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            lastGroundedNormal = Vector2.up;
            
            isJumping = false;
            
            // Only enable double jump when grounded if double jump is unlocked
            canDoubleJump = canDoubleJumpUnlocked;
        }
    }

    private void UpdateTimers()
    {
        if (!canDash)
        {
            dashCooldownCounter -= Time.deltaTime;
            if (dashCooldownCounter <= 0)
            {
                canDash = true;
            }
        }
        
        if (!canSlide)
        {
            slideCooldownCounter -= Time.deltaTime;
            if (slideCooldownCounter <= 0)
            {
                canSlide = true;
            }
        }
    }
    
    private bool DidJustJump()
    {
        return (Time.time - lastJumpTime <= 0.02f + groundedToleranceTime);
    }
    
    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isGrounded", isGrounded);
            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isWallSliding", isWallSliding);
            animator.SetBool("isDashing", isDashing);
            animator.SetBool("isSliding", isSliding);
            animator.SetFloat("horizontalSpeed", Mathf.Abs(rb.velocity.x));
            animator.SetFloat("verticalSpeed", rb.velocity.y);
            animator.SetBool("isSprinting", sprintInput && Mathf.Abs(inputMovement.x) > 0);
        }
    }
    #endregion

    #region Public Methods
    public void ResetPlayer()
    {
        rb.velocity = Vector2.zero;
        
        isJumping = false;
        isWallJumping = false;
        isWallSliding = false;
        isDashing = false;
        
        if (isSliding)
        {
            StopSlide();
        }
        
        canDoubleJump = canDoubleJumpUnlocked;
        canDash = true;
        canSlide = true;
        
        jumpInputIsCached = false;
        lastJumpPressedTime = 0;
        lastJumpTime = 0;
        lastGroundedTime = 0;
        wallJumpCounter = 0;
        dashCooldownCounter = 0;
        slideCooldownCounter = 0;
        currentSlideTimer = 0;
        
        if (!isFacingRight)
        {
            Flip();
        }
    }
    
    // Methods to unlock/disable abilities at runtime
    public void UnlockDoubleJump(bool unlock)
    {
        canDoubleJumpUnlocked = unlock;
        if (isGrounded)
        {
            canDoubleJump = unlock;
        }
    }
    
    public void UnlockWallJump(bool unlock)
    {
        wallJumpUnlocked = unlock;
        if (!unlock)
        {
            isWallSliding = false;
            isWallJumping = false;
        }
    }
    
    public void UnlockSprint(bool unlock)
    {
        sprintUnlocked = unlock;
    }
    
    public void UnlockDash(bool unlock)
    {
        dashUnlocked = unlock;
        if (!unlock)
        {
            isDashing = false;
        }
    }
    
    public void UnlockSlide(bool unlock)
    {
        slideUnlocked = unlock;
        if (!unlock && isSliding)
        {
            StopSlide();
        }
    }
    #endregion
    
    public Vector2 GetCurrentVelocity()
    {
        return rb.velocity;
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        rb.velocity = newVelocity;
    }
    
    

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        Gizmos.color = Color.red;
        Vector2 wallCheckDirection = isFacingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + wallCheckDirection * wallCheckDistance);
    }
    #endregion
}