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
        if (Input.GetButton("Horizontal") && PController.checkIfGrounded()) {
            anim.SetBool("isRunning", true);
        } else {
            anim.SetBool("isRunning", false);
        }

    }
}
