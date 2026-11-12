using UnityEngine;

// Drives the Animator from PlayerController's state; reads no input itself.
public class PlayerAnim : MonoBehaviour {

    private Animator anim;
    private PlayerController PController;

    void Awake() {
        anim = GetComponent<Animator>();
        PController = GetComponent<PlayerController>();
    }

    void OnEnable() {
        PController.Jumped += OnJumped;
    }

    void OnDisable() {
        PController.Jumped -= OnJumped;
    }

    void Update() {
        Move();
        Crouch();
        CheckIfMoving();
        CheckColliders();

    }
    void Move() {
        //Check if character is running or sprinting
        if (PController.HorizontalInput != 0 && PController.IsGrounded) {
            if (PController.IsSprinting) {
                anim.SetBool("isSprinting", true);
                anim.SetBool("isRunning", true);
            } else {
                anim.SetBool("isRunning", true);
                anim.SetBool("isSprinting", false);
            }
        } else {
            anim.SetBool("isRunning", false);
            anim.SetBool("isSprinting", false);
        }
    }
    void OnJumped() {
        anim.SetTrigger("jump");
    }
    void Crouch() {
        anim.SetBool("crouch", PController.IsCrouching);
    }

    void CheckIfMoving() {
        if(PController.rigBody.linearVelocity.x != 0) {
            anim.SetBool("isMoving", true);
        } else {
            anim.SetBool("isMoving", false);
        }
    }

   void CheckColliders() {
        //Ground collider
        if (PController.IsGrounded) {
            anim.SetBool("isGrounded", true);
        } else {
            anim.SetBool("isGrounded", false);
        }

        //Wall collieder
        if (PController.HasWall) {
            anim.SetBool("hasWall", true);
        } else {
            anim.SetBool("hasWall", false);
        }

    }
}
