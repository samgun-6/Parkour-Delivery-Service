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

    }
    void Move() {
        //Check if character is running, sprinting or both 
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
        if (Input.GetButtonDown("Jump") && PController.CheckIfGrounded()) {
            anim.SetTrigger("jump");
        } else if(Input.GetButtonUp("Jump")){
            anim.ResetTrigger("jump");
        }

        //Check if the caracter is still in the air
        if (!PController.CheckIfGrounded()) {
            anim.SetBool("flying", true);
        }else if (PController.CheckIfGrounded()){
            anim.SetBool("flying", false);
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
}
