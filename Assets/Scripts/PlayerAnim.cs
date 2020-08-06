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
            anim.SetTrigger("landing");
        }

    }
    void Crouch() {

    }
}
