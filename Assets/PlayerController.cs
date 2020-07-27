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
    
    void Start(){
        rigBody = GetComponent<Rigidbody2D>();  //Get the rigidbody component
    }

    // Update is called once per frame
    void Update(){
        Move();
        Jump();
        checkIfGrounded();
    }

    void Move() {
        float x = Input.GetAxis("Horizontal");
        float movement = x * speed;           //Decides movement speed per second
        rigBody.velocity = new Vector2(movement, rigBody.velocity.y);
    }
    void Jump() {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            rigBody.velocity = new Vector2(rigBody.velocity.x, elasticity);
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
