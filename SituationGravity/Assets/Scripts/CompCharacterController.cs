using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompCharacterController : MonoBehaviour
{
    public Animator animator;
    public PlayerController playerController;
    //public Transform playerTransform;
    //public Transform animationTransform;

    private void Start()
    {
        animator = GetComponent<Animator>();
        //animationTransform = new Vector3(0f,0f,0f);
        //playerTransform = GetComponentInParent<Transform>();
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        //animationTransform = playerTransform;
        animator.SetFloat("Forward", playerController.controllerSpeed);
    }
}
