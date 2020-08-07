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
    
    void Start(){
        rigBody = GetComponent<Rigidbody2D>();  //Get the rigidbody component
        facingRight = true;
    }

    // Update is called once per frame
    void Update(){
        float horizontal = Input.GetAxis("Horizontal");
        Move(horizontal);
        Flip(horizontal);
        Jump();
        isGrounded = checkIfGrounded();
        BetterJump();
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

    public bool checkIfGrounded() {
        Collider2D collider = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, groundLayer);
        if(collider != null) {
            return true;
        } else {
            return false;
        }
    }
}
