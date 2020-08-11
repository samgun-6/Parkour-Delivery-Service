/*
 * Thanks to tutorial by craftgames which I used as help when making most of this script
 * https://craftgames.co/unity-2d-platformer-movement/
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{

    public float speed;
    public float sprintSpeed;
    public float elasticity;
    private Rigidbody2D rigBody;
    private bool facingRight;
    private float movement;

    //Ground variables
    bool isGrounded = false;
    public Transform isGroundedChecker;
    public float checkGroundRadius;
    public LayerMask groundLayer;

    //Variables for a better jump
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    //Variables for crouching
    private bool isCrouched;
    private float crouchHeight = 1.7f;
    private float standHeight = 2.56f;
    private float crouchOffset = -0.44f;

    CapsuleCollider2D playerCollider;
    
    void Start(){
        rigBody = GetComponent<Rigidbody2D>();  //Get the rigidbody component
        playerCollider = GetComponent<CapsuleCollider2D>(); //Get the player collider
        facingRight = true;
        isCrouched = false;
    }

    // Update is called once per frame
    void Update(){
        float horizontal = Input.GetAxis("Horizontal");
        Move(horizontal);
        Flip(horizontal);
        Jump();
        isGrounded = CheckIfGrounded();
        BetterJump();
        Crouch();
        StandUp();
    }

    void Move(float horizontal) {

        //Set movement speed per second
        if (Input.GetKey(KeyCode.J)) {
            movement = horizontal * sprintSpeed;
        } else{
            movement = horizontal * speed;          
        }
        //Apply force
        rigBody.velocity = new Vector2(movement, rigBody.velocity.y);
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
        if (Input.GetButtonDown("Jump") && isGrounded) {
            rigBody.velocity = new Vector2(rigBody.velocity.x, elasticity);
        }
    }

    /*
     * Thanks to "Board To Bits Games" channel for this "BetterJump" method
     * Channel https://www.youtube.com/channel/UCifiUB82IZ6kCkjNXN8dwsQ
     * Tutorial https://www.youtube.com/watch?reload=9&v=7KiK0Aqtmzc
     */
    void BetterJump() {
        if(rigBody.velocity.y < 0) {
            rigBody.velocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.deltaTime;
        }else if(rigBody.velocity.y > 0 && !Input.GetButtonDown("Jump")) {
            rigBody.velocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void Crouch() {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) && isCrouched == false) {
            isCrouched = true;
            playerCollider.size = new Vector2(playerCollider.size.x, crouchHeight);
            playerCollider.offset = new Vector2(0, crouchOffset);
        }
    }

    void StandUp() {
        if(Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S) && isCrouched == true) {
            isCrouched = false;
            playerCollider.size = new Vector2(playerCollider.size.x, standHeight);
            playerCollider.offset = new Vector2(0, 0);
        }
    }

    public bool CheckIfGrounded() {
        Collider2D groundCollider = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, groundLayer);
        if(groundCollider != null) {
            return true;
        } else {
            return false;
        }
    }
}
