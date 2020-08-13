using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour {

    private Animator anim;
    private PlayerController PController;
    void Start() {
        anim = GetComponent<Animator>();
        PController = GetComponent<PlayerController>();
    }

    void Update() {
        Move();
        Jump();
        Crouch();
        CheckIfMoving();
        CheckColliders();

    }
    void Move() {
        //Check if character is running or sprinting 
        if (Input.GetButton("Horizontal") && PController.CheckIfGrounded()) {
            if (Input.GetKey(KeyCode.J)) {
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
    void Jump() {
        //Check if the character is jumping
        if (Input.GetButtonDown("Jump")) {
            anim.SetTrigger("jump");
        } else if(Input.GetButtonUp("Jump")){
            anim.ResetTrigger("jump");
        }
    }
    void Crouch() {
        if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            anim.SetBool("crouch", true);
        }

        if(Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) {
            anim.SetBool("crouch", false);
        }
    }

    void CheckIfMoving() {
        if(PController.rigBody.velocity.x != 0) {
            anim.SetBool("isMoving", true);
        } else {
            anim.SetBool("isMoving", false);
        }
    }

   void CheckColliders() {
        //Ground collider
        if (PController.CheckIfGrounded()) {
            anim.SetBool("isGrounded", true);
        } else {
            anim.SetBool("isGrounded", false);
        }

        //Wall collieder
        if (PController.CheckIfHasWall()) {
            anim.SetBool("hasWall", true);
        } else {
            anim.SetBool("hasWall", false);
        }

    }
}
