using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public PlayerController playerController;
    public CharacterController controller;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;
    

    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    public float wallLookAngle;
    private RaycastHit frontWallHit;
    private bool wallFront;

    [Header("Wall Jump")]
    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing) ClimbingMovement();
    }

    private void StateMachine()
    {
        // State 1 - Climbing
        if (wallFront && playerController.isHorizontalMoving && wallLookAngle < maxWallLookAngle)
        {
            if (!climbing && climbTimer > .5) StartClimbing();

            // Timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        } 

        // State 3 - None
        else
        {
            if (climbing) StopClimbing();
        }

        if (wallFront && playerController.jumpAction.triggered && climbJumpsLeft > 0) ClimbJump();
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        // Define what angle the player can look to wall jump
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        // Determine if the next wall that's hit is a new wall - MAY DELETE LATER
        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        // newWall - MAY DELETE LATER
        if (controller.isGrounded || (wallFront && newWall))
        {
            climbTimer = maxClimbTime;
            // Multiple Wall jumps
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        climbing = true;
        playerController.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;

        // camera fov change
    }

    private void ClimbingMovement()
    {
        playerController.jumpVelocity = new Vector3(playerController.jumpVelocity.x, climbSpeed, playerController.jumpVelocity.z);

        // play sound effect
    }

    private void StopClimbing()
    {
        climbing = false;
        playerController.climbing = false;
        // particle effect
    }

    // Update is called once per frame
    

    private void ClimbJump()
    {
        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        playerController.jumpVelocity = new Vector3(playerController.jumpVelocity.x, 0f, playerController.jumpVelocity.z);
        playerController.jumpVelocity += forceToApply;
        
        // Deacceleration of jump back force


        climbJumpsLeft--;
    }


}
