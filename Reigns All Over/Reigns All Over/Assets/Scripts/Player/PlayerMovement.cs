using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Variables")]
    public float runSpeed;
    public float sprintMultiplier = 1f;
    public float alignSpeed;

    [Header("Character States")]
    public bool isGrounded;
    public bool isRunning;
    public bool isDodging;
    public bool isDead;

    // Private stuff
    Vector3 FaceDirection;                             // Input Direction, match forward vector with this

    // camera reference:
    [Header("Object References")]
    public Transform CamRef;
    public Transform FeetRef;
    public Transform PlayerMesh;
    Transform TargetRef;

    //animator ref
    Animator animator;

    void Start()
    {
        TargetRef = CamRef.parent;
        animator = PlayerMesh.GetComponent<Animator>();
    }

    void Update()
    {
        transform.rotation = Quaternion.identity;                 // Make sure that PlayerHolder Never rotates.

        // to make camera follow on jumps and fall
        TargetRef.transform.position = new Vector3(transform.position.x, PlayerMesh.position.y, transform.position.z);     
        // make same thing for PlayerHolder too if any thing bugs out

        if(!isDead)
            PMovement();
        CheckGrounded();
    }

    /// <summary>
    /// Upon directonal keypress, move in direction, as well as align. Uses 360 movement when not in combat.
    /// </summary>
    void PMovement()
    {
        Vector3 PlayerDirection = Vector3.zero;

        // Movement Input --------------------------------------------------------------------
        // Front & Back.
        if(Input.GetKey(KeyCode.W))                                                     // Takes in player input and set PlayerDirection
            PlayerDirection += TargetRef.forward;
        else if(Input.GetKey(KeyCode.S))
            PlayerDirection += TargetRef.forward * -1;
        // Sideways
        if (Input.GetKey(KeyCode.A))
            PlayerDirection += TargetRef.right * -1;
        else if(Input.GetKey(KeyCode.D))
            PlayerDirection += TargetRef.right;
        //--------------------------------------------------------------------------------------
        // Sprint
        if (Input.GetKey(KeyCode.LeftShift))
        { sprintMultiplier = 1.4f; }
        else
            sprintMultiplier = 1f;
                
        //---------------------------------------------------------------------------------------


        PlayerDirection = PlayerDirection.normalized;
        PlayerDirection.y = 0;

        // Translation
        if(Vector3.Angle(PlayerDirection,PlayerMesh.forward) < 35)
            transform.Translate(PlayerDirection * runSpeed * sprintMultiplier* Time.deltaTime);
        else
            transform.Translate(PlayerDirection * runSpeed * sprintMultiplier * 0.4f* Time.deltaTime);

        // Anim Update && Alignment
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            AlignOrientation(PlayerDirection);
            isRunning = true;
            //Debug.DrawRay(transform.position, PlayerDirection, Color.green);

            // animation stuff
            // increase locomotion variable
            if (animator.GetFloat("Locomotion") < 0.5 && sprintMultiplier<=1)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion")+0.04f);
            else if(animator.GetFloat("Locomotion") < 1 && sprintMultiplier > 1)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
        }
        else
        {
            isRunning = false;
            if (animator.GetFloat("Locomotion") > 0)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);
        }




        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            PlayerMesh.GetComponent<Rigidbody>().AddForce(55f*Vector3.up,ForceMode.Impulse);
        }

    }

    void CheckGrounded()
    {
        // Use Physics raycast to determine if player is grounded or not.
        Vector3 CastPoint = FeetRef.transform.position;
        if (Physics.Raycast(CastPoint, Vector3.down, 0.3f))
        {
            isGrounded = true;
            animator.SetBool("isGrounded", true);
        }
        else
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }
    
 
    void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        PlayerMesh.transform.localRotation = Quaternion.RotateTowards(PlayerMesh.localRotation, lookDirection, alignSpeed);
    }
}
