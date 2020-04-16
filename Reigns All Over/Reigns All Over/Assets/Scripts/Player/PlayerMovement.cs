using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Variables")]
    public float walkSpeed;
    public float runSpeed;
    public float sprintMultiplier = 1f;
    public float alignSpeed;

    // Private Variables
    float fallDuration = 0f;
    bool jumped;

    [Header("Character States")]
    public bool isGrounded;
    public bool isRunning;
    public bool isDodging;
    public bool isDead;
    public bool isWalking;
    public bool controlLock;


    // Private stuff
    Vector3 FaceDirection;                             // Input Direction, match forward vector with this

    // camera reference:
    [Header("Object References")]
    public Transform CamRef;
    public Transform FeetRef;
    public Transform PlayerHolder;
    Transform TargetRef;

    //animator ref
    Animator animator;

    void Start()
    {
        TargetRef = CamRef.parent;
        animator = GetComponent<Animator>();
    }

    void Update()
    {

        // to make camera follow on jumps and fall
        TargetRef.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);     

        if(!isDead && !controlLock)
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
        {
            sprintMultiplier = 1.4f;
            animator.SetFloat("sprintMultiplier", 1.4f);
        }
        else
        {
            sprintMultiplier = 1f;
            animator.SetFloat("sprintMultiplier", 1f);
        } 
        //---------------------------------------------------------------------------------------


        PlayerDirection = PlayerDirection.normalized;
        PlayerDirection.y = 0;

        PlayerTranslation(PlayerDirection);
        

        
        


        // Jumping -- need to fix double jumping issue
        if (Input.GetKeyDown(KeyCode.Space) && fallDuration<0.1f && !jumped)
        {
            jumped = true;
            GetComponent<Rigidbody>().AddForce(50f*Vector3.up,ForceMode.Impulse);
        }


        // toggle walking
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            if (isWalking)
            {
                isWalking = false;
            }
            else
            {
                isWalking = true;
                if(isRunning)
                    animator.SetFloat("Locomotion", 0.5f);
            }
        }
    }

    void PlayerTranslation(Vector3 dir)
    {
        // Translation
        float slowdownMultipier = 1f;
        if (Vector3.Angle(dir, transform.forward) < 35)            // move at maximum speed
            slowdownMultipier = 1f;
        else
            slowdownMultipier = 0.4f;

        if (isGrounded)
        {
            // check whether running or walking or sprinting
            if (isWalking)
                PlayerHolder.Translate(dir * walkSpeed * slowdownMultipier * Time.deltaTime);
            else // running
                PlayerHolder.Translate(dir * runSpeed * sprintMultiplier * slowdownMultipier * Time.deltaTime);
        }
        else
        {
            PlayerHolder.Translate(dir * runSpeed *  slowdownMultipier * Time.deltaTime);
        }




        // Anim Update && Alignment
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            AlignOrientation(dir);
            isRunning = true;

            // increase locomotion variable
            if (animator.GetFloat("Locomotion") < 0.5 && isWalking)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
            else if (animator.GetFloat("Locomotion") < 1 && !isWalking)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
        }
        else
        {
            isRunning = false;
            if (animator.GetFloat("Locomotion") > 0)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);
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
            jumped = false;
            // check if fell for too long and kill
            fallDuration = 0f;
        }
        else
        {
            fallDuration += Time.deltaTime;
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }

    }
    
 
    void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, alignSpeed);
        //transform.Rotate(new Vector3(0,20,0));
    }
}
