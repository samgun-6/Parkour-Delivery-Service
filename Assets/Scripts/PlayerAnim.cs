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
        Run();
        Jump();
        Crouch();

    }
    void Run() {
        if (Input.GetButton("Horizontal") && PController.checkIfGrounded()) {
            anim.SetBool("isRunning", true);
        } else {
            anim.SetBool("isRunning", false);
        }
    }
    void Jump() {
        if (Input.GetButtonDown("Jump") && PController.checkIfGrounded()) {
            anim.SetTrigger("jump");
        } else if(Input.GetButtonUp("Jump")){
            anim.ResetTrigger("jump");
        }

        if (!PController.checkIfGrounded()) {
            anim.SetBool("flying", true);
        }else if (PController.checkIfGrounded()){
            anim.SetBool("flying", false);
        }

    }
    void Crouch() {
        if(Input.GetKeyDown("down") || Input.GetKeyDown("s")) {
            anim.SetBool("crouch", true);
        }

        if(Input.GetKeyUp("down") || Input.GetKeyUp("s")) {
            anim.SetBool("crouch", false);
        }
    }
}
