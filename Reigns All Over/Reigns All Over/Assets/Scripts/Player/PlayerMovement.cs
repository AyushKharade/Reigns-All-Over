using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Variables")]
    public float walkSpeed;
    public float runSpeed;
    public float sprintCostRate = 10f;
    public float alignSpeed;

    // Private Variables
    float fallDuration = 0f;
    bool jumped;
    float ogColliderHeight;

    [Header("Character States")]
    public bool isGrounded;
    public bool isRunning;
    public bool isSprinting;
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

    // script References
    Combat CombatRef;
    PlayerAttributes PAttributesRef;

    void Start()
    {
        TargetRef = CamRef.parent;
        animator = GetComponent<Animator>();

        CombatRef = GetComponent<Combat>();
        PAttributesRef = GetComponent<PlayerAttributes>();

        ogColliderHeight = GetComponent<CapsuleCollider>().height;
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
        if (Input.GetKey(KeyCode.LeftShift) && !CombatRef.inCombat && PAttributesRef.stamina>0)
        {
            isSprinting = true;
            PAttributesRef.stamina -= sprintCostRate * Time.deltaTime;
            PAttributesRef.onStaminaRegenDelay = true;
            isWalking = false;
        }
        else
        {
            isSprinting = false;
        } 
        //---------------------------------------------------------------------------------------


        PlayerDirection = PlayerDirection.normalized;
        PlayerDirection.y = 0;

        PlayerTranslation(PlayerDirection);






        // Jumping -- need to fix double jumping issue
        if (Input.GetKeyDown(KeyCode.Space) && fallDuration < 0.1f && !jumped && !CombatRef.inCombat)
        {
            jumped = true;
            GetComponent<Rigidbody>().AddForce(50f * Vector3.up, ForceMode.Impulse);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && CombatRef.inCombat && !isDodging && CombatRef.ready)
        {
            // get a direction dodge.
            Dodge(PlayerDirection);
        }


        // toggle walking
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            if (isWalking)
            {
                isWalking = false;
            }
            else if(!isSprinting)
            {
                isWalking = true;
                if(isRunning)
                    animator.SetFloat("Locomotion", 0.33f);
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

        if (isGrounded && !isDodging)
        {
            // check whether running or walking or sprinting
            if (isWalking)
                PlayerHolder.Translate(dir * walkSpeed * slowdownMultipier * Time.deltaTime);
            else if(!isSprinting)// running
                PlayerHolder.Translate(dir * runSpeed *  slowdownMultipier * Time.deltaTime);
            else if(isSprinting)
                PlayerHolder.Translate(dir * (runSpeed-1f) * slowdownMultipier * Time.deltaTime);       // run speed is kind of fast
        }
        else if(!isDodging)
        {
            PlayerHolder.Translate(dir * (runSpeed+3) *  slowdownMultipier * Time.deltaTime);         // this is for mid air movement
        }




        // Anim Update && Alignment
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if(!isDodging)
                AlignOrientation(dir);
            isRunning = true;

            if (isWalking && animator.GetFloat("Locomotion") < 0.33f)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
            else if (!isWalking && !isSprinting && animator.GetFloat("Locomotion") < 0.66f)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
            else if(isSprinting && animator.GetFloat("Locomotion") < 1f)// sprinting
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);


            // cases if you stop doing things.
            if(animator.GetFloat("Locomotion") > 0.66f && !isSprinting)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);

        }
        else
        {
            isRunning = false;

            if (animator.GetFloat("Locomotion") > 0)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);
        }

    }

    void Dodge(Vector3 Dir)
    {
        // we compute the type of dodge depending on the Dir and the angle of camera and player.
        // e.g. if angle between player forward and camera is < 10, its a forward roll.
        // all 4 rolls are stored in a blend tree. respect values are enabled.
        isDodging = true;
        animator.SetLayerWeight(1, 0);       // switch off combat layer until then.
        animator.SetBool("isDodging", true);
        GetComponent<CapsuleCollider>().height = 1.5f;

        Vector3 raypos = transform.position;
        raypos.y += 1;
        Debug.DrawRay(raypos, Dir, Color.green);

        // commit in a direction and translate in that since we are not using root motion.
        float angle = Vector3.Angle(Dir, transform.forward);
        Debug.Log("Dodge Angle: " + angle);
        if (angle < 100)
        {
            animator.SetFloat("DodgeRoll", 0f);     // front roll
            // align towards dir
            if(angle>25)
                transform.LookAt(Dir);
        }
        else
        {
            animator.SetFloat("DodgeRoll", 0.33f);     // Backroll roll
        }
    }

    /// <summary>
    /// Called from animation event to cancel invincibility.
    /// </summary>
    public void UnDodge()
    {
        isDodging = false;
        animator.SetLayerWeight(1, 1);       // switch off combat layer until then.
        animator.SetBool("isDodging",false);
        GetComponent<CapsuleCollider>().height = ogColliderHeight;

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
