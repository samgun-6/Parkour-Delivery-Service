/*
 * Thanks to tutorial by craftgames which I used as help when making most of this script
 * https://craftgames.co/unity-2d-platformer-movement/
 */
using System;
using ParkourDelivery.Input;
using UnityEngine;

public class PlayerController : MonoBehaviour{

    //Read-only state for animation and game logic
    public bool IsGrounded => isGrounded;
    public bool HasWall => wallDetect;
    public bool IsCrouching => isCrouched;
    public bool IsSprinting => sprintHeld;
    public bool IsWallSliding => wallSliding;
    public float HorizontalInput => horizontalInput;
    public event Action Jumped;

    //Player
    public float speed;
    public float sprintSpeed;
    public float elasticity;
    public Rigidbody2D rigBody;
    private bool facingRight;
    private float movement;

    //Ground
    bool isGrounded = false;
    public Transform isGroundedCheck;
    float checkGroundRadius = 0.05f;
    public LayerMask groundLayer;

    //Better jump
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    //Crouching
    private bool isCrouched;
    private float crouchHeight = 1.7f;
    private float standHeight = 2.56f;
    private float crouchOffset = -0.44f;

    //Head
    bool hasTopObstacle = false;
    public Transform topObstacleCheck;

    //Wall
    bool wallDetect = false;
    public Transform rightWallCheck;
    public Transform leftWallCheck;
    public float wallSlidingSpeed;
    bool wallSliding;
    int wallSide; //+1 wall to the right, -1 to the left, 0 none

    //Wall jump: push away from the wall, briefly ignoring move input
    //so the player cannot immediately steer back into the wall
    public Vector2 wallJumpForce = new Vector2(10f, 13f);
    public float wallJumpLockTime = 0.2f;
    float wallJumpLockUntil;

    CapsuleCollider2D playerCollider;

    //Input sampled in Update, consumed in FixedUpdate
    PlayerControls controls;
    float horizontalInput;
    bool sprintHeld;
    bool jumpHeld;
    bool crouchHeld;
    bool jumpRequested;

    void Awake(){
        controls = new PlayerControls();
    }

    void OnEnable(){
        controls.Player.Enable();
    }

    void OnDisable(){
        controls.Player.Disable();
    }

    void OnDestroy(){
        controls.Dispose();
    }

    void Start(){
        rigBody = GetComponent<Rigidbody2D>();  //Get the rigidbody component
        playerCollider = GetComponent<CapsuleCollider2D>(); //Get the player collider
        //Without a frictionless body, pushing into a wall pins the player in place
        //and wall sliding never starts
        if (rigBody.sharedMaterial == null) {
            rigBody.sharedMaterial = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f };
        }
        facingRight = true;
        isCrouched = false;
    }

    // Update is called once per frame: read input and handle non-physics state
    void Update(){
        var input = controls.Player;
        horizontalInput = input.Move.ReadValue<float>();
        sprintHeld = input.Sprint.IsPressed();
        jumpHeld = input.Jump.IsPressed();
        crouchHeld = input.Crouch.IsPressed();
        //Latch the press so a FixedUpdate that runs after this frame still sees it
        if (input.Jump.WasPressedThisFrame()) {
            jumpRequested = true;
        }
        if (!IsWallJumpLocked()) {
            Flip(horizontalInput);
        }
        Crouch();
        StandUp();
    }

    // Physics runs at a fixed rate, independent of frame rate
    void FixedUpdate(){
        isGrounded = CheckIfGrounded();
        wallSide = GetWallSide();
        wallDetect = wallSide != 0;
        //Slide when airborne, falling and pushing into a wall
        wallSliding = !isGrounded && wallDetect && horizontalInput * wallSide > 0 && rigBody.linearVelocity.y < 0;
        if (!IsWallJumpLocked()) {
            Move(horizontalInput);
        }
        Jump();
        BetterJump();
        WallSlide();
    }

    bool IsWallJumpLocked() {
        return Time.time < wallJumpLockUntil;
    }

    void Move(float horizontal) {

        //Set movement speed per second
        if (sprintHeld) {
            movement = horizontal * sprintSpeed;
        } else{
            movement = horizontal * speed;
        }
        //Apply force
        rigBody.linearVelocity = new Vector2(movement, rigBody.linearVelocity.y);
    }
    void Flip(float horizontal) {
        if(horizontal > 0 && !facingRight || horizontal < 0 && facingRight) {
            facingRight = !facingRight;
            Vector3 playerScale = transform.localScale;
            playerScale.x *= -1;
            transform.localScale = playerScale;
        }
    }
    void Jump() {
        if (jumpRequested && isGrounded) {
            rigBody.linearVelocity = new Vector2(rigBody.linearVelocity.x, elasticity);
            Jumped?.Invoke();
        } else if (jumpRequested && wallDetect && (wallSliding || horizontalInput * wallSide > 0)) {
            WallJump();
        }
        jumpRequested = false;
    }

    void WallJump() {
        rigBody.linearVelocity = new Vector2(-wallSide * wallJumpForce.x, wallJumpForce.y);
        wallJumpLockUntil = Time.time + wallJumpLockTime;
        wallSliding = false;
        Flip(-wallSide);
        Jumped?.Invoke();
    }

    void WallSlide() {
        if (wallSliding && rigBody.linearVelocity.y < -wallSlidingSpeed) {
            rigBody.linearVelocity = new Vector2(rigBody.linearVelocity.x, -wallSlidingSpeed);
        }
    }

    /*
     * Thanks to "Board To Bits Games" channel for this "BetterJump" method
     * Channel https://www.youtube.com/channel/UCifiUB82IZ6kCkjNXN8dwsQ
     * Tutorial https://www.youtube.com/watch?reload=9&v=7KiK0Aqtmzc
     */
    void BetterJump() {
        if(rigBody.linearVelocity.y < 0) {
            rigBody.linearVelocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }else if(rigBody.linearVelocity.y > 0 && !jumpHeld) {
            rigBody.linearVelocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void Crouch() {
        if (crouchHeld && isCrouched == false) {
            isCrouched = true;
            playerCollider.size = new Vector2(playerCollider.size.x, crouchHeight);
            playerCollider.offset = new Vector2(0, crouchOffset);
        }
    }

    void StandUp() {
        //Stay crouched under a low ceiling even when the key is released
        hasTopObstacle = isCrouched && CheckIfGroundAbove();
        if(!crouchHeld && isCrouched == true && !hasTopObstacle) {
            isCrouched = false;
            playerCollider.size = new Vector2(playerCollider.size.x, standHeight);
            playerCollider.offset = new Vector2(0, 0);
        }
    }

    public bool CheckIfGrounded() {
        Collider2D groundCollider = Physics2D.OverlapCircle(isGroundedCheck.position, checkGroundRadius, groundLayer);
        if(groundCollider != null) {
            return true;
        } else {
            return false;
        }
    }
    public bool CheckIfGroundAbove() {
        Collider2D topCollider = Physics2D.OverlapCircle(topObstacleCheck.position, checkGroundRadius, groundLayer);
        if (topCollider != null) {
            return true;
        } else {
            return false;
        }
    }
    public bool CheckIfHasWall() {
        return GetWallSide() != 0;
    }

    //The wall checkers flip with the sprite, so derive the world side from their position
    int GetWallSide() {
        if (Physics2D.OverlapCircle(rightWallCheck.position, checkGroundRadius, groundLayer) != null) {
            return Math.Sign(rightWallCheck.position.x - transform.position.x);
        }
        if (Physics2D.OverlapCircle(leftWallCheck.position, checkGroundRadius, groundLayer) != null) {
            return Math.Sign(leftWallCheck.position.x - transform.position.x);
        }
        return 0;
    }
}
