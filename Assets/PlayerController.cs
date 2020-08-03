/*
 * Thanks to tutorial by craftgames which I used as help when making this script
 * https://craftgames.co/unity-2d-platformer-movement/
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{

    public float speed;
    public float elasticity;
    private Rigidbody2D rigBody;
    bool isGrounded = false;
    public Transform isGroundedChecker;
    public float checkGroundRadius;
    public LayerMask groundLayer;

    //Variables for a better jump
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    
    void Start(){
        rigBody = GetComponent<Rigidbody2D>();  //Get the rigidbody component
    }

    // Update is called once per frame
    void Update(){
        Move();
        Jump();
        checkIfGrounded();
        BetterJump();
    }

    void Move() {
        float x = Input.GetAxis("Horizontal");
        float movement = x * speed;           //Decides movement speed per second
        rigBody.velocity = new Vector2(movement, rigBody.velocity.y);
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

    void checkIfGrounded() {
        Collider2D collider = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, groundLayer);
        if(collider != null) {
            isGrounded = true;
        } else {
            isGrounded = false;
        }
    }
}
