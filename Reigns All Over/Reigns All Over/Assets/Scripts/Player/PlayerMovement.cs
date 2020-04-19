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

        CheckGrounded();

        if (!isDead && !controlLock)
            PMovement();

        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerHolder.position = new Vector3(0, 27, -13);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            PAttributesRef.DealDamage(10);
        }

    }

    #region Movement & Translation
    /// <summary>
    /// Upon directonal keypress, move in direction, as well as align. Uses 360 movement when not in lock on combat.
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
        if (Input.GetKey(KeyCode.LeftShift) && PAttributesRef.stamina>0)
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
            Dodge(PlayerDirection);       // get a direction dodge.

        // orient dodging                   --> basically orient slowly towards direction of dodge so front roll looks smooth
        if (isDodging && doDodgeAlign)
        {
            DodgeAlign(dodgeDirection);

            // This could be a skill, on both dodges to dodge even further
            if (dodgeDirection == -1)
                PlayerHolder.Translate(transform.forward * -1 * 1.25f * Time.deltaTime);
        }

        // toggle walking
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            if (isWalking)
            {
                isWalking = false;
            }
            else if(!isSprinting && !CombatRef.inCombat)
            {
                isWalking = true;
                if(isRunning)
                    animator.SetFloat("Locomotion", 0.33f);
            }
        }



        // fighting input direction.
        if (!isDead && isGrounded && !isDodging && CombatRef.ready)
        {
            Vector3 attackDir=PlayerDirection;
            if (attackDir == Vector3.zero)
                attackDir = transform.forward;

            if (Input.GetMouseButtonDown(0))
                CombatRef.FightingControls(0, attackDir);
            else if (Input.GetMouseButtonDown(1))
                CombatRef.FightingControls(1, attackDir);
        }


        
    }





    /// <summary>
    /// handles Translation and Orientation, also movement animation update
    /// </summary>
    /// <param name="dir"></param>
    void PlayerTranslation(Vector3 dir)
    {
        // Translation
        float slowdownMultipier = 1f;
        if (Vector3.Angle(dir, transform.forward) < 35)            // move at maximum speed
            slowdownMultipier = 1f;
        else
            slowdownMultipier = 0.4f;

        if (isGrounded && !isDodging && !CombatRef.attacking)
        {
            // check whether running or walking or sprinting
            if (isWalking)
                PlayerHolder.Translate(dir * walkSpeed * slowdownMultipier * Time.deltaTime);
            else if(!isSprinting)// running
                PlayerHolder.Translate(dir * runSpeed *  slowdownMultipier * Time.deltaTime);
            else if(isSprinting)
                PlayerHolder.Translate(dir * (runSpeed-1f) * slowdownMultipier * Time.deltaTime);       // run speed is kind of fast
        }
        else if(!isDodging && !CombatRef.attacking)
        {
            PlayerHolder.Translate(dir * (runSpeed+3) *  slowdownMultipier * Time.deltaTime);         // this is for mid air movement
        }




        // Anim Update && Alignment
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if(!isDodging && !CombatRef.attacking)
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

    #endregion

    #region Dodging
    void Dodge(Vector3 Dir)
    {
        // we compute the type of dodge depending on the Dir and the angle of camera and player.
        // Both rolls are stored in a blend tree. respect values are enabled.

        isDodging = true;
        PAttributesRef.dodgeInvincible = true;

        #region Interupt Attacks
        if (CombatRef.attacking)
        {
            animator.SetBool("Attacking", false);
            animator.SetBool("ChainAttack", false);
            //animator.SetLayerWeight(2, 0);              // looks smoother, since it will slowly turn off when attacking is false
            CombatRef.attacking = false;
            CombatRef.chainAttack = false;
            CombatRef.chained = false;
            CombatRef.chainWindowOpen = false;

            CombatRef.combo = 1;
        }



        #endregion


        animator.SetLayerWeight(1, 0);       // switch off combat layer until then.
        animator.SetBool("isDodging", true);
        GetComponent<CapsuleCollider>().height = 1.5f;
        
        
        float angle = Vector3.Angle(Dir, transform.forward);
        //Debug.Log("Dodge Angle: " + angle);
        if (angle <= 125)
        {
            animator.SetFloat("DodgeRoll", 0f);     // front roll
            DodgeAlignDir = Dir;
            dodgeDirection = 1;
            doDodgeAlign = true;
        }
        else if(angle> 125 && angle < 185)
        {
            DodgeAlignDir = Dir;
            dodgeDirection = -1;
            doDodgeAlign = true;
            animator.SetFloat("DodgeRoll", 1f);     // Backroll roll
        }
    }

    Vector3 DodgeAlignDir;
    bool doDodgeAlign;
    /// <summary>
    /// Calls align in the direction you are dodging in
    /// </summary>
    int dodgeDirection = 1;
    void DodgeAlign(int direction)
    {
        if (DodgeAlignDir == Vector3.zero)
            AlignOrientation(transform.forward);
        else
            AlignOrientation(DodgeAlignDir*direction);
    }



    /// <summary>
    /// Called from animation event to cancel invincibility.
    /// </summary>
    public void UnDodge()
    {
        isDodging = false;
        PAttributesRef.dodgeInvincible = false;

        animator.SetLayerWeight(1, 1);       // switch off combat layer until then.
        animator.SetBool("isDodging",false);
        GetComponent<CapsuleCollider>().height = ogColliderHeight;
        doDodgeAlign = false;
    }

    #endregion


    /// <summary>
    /// Checks raycasts if falling
    /// </summary>
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
            if (fallDuration > 2f)
            {
                PAttributesRef.DealDamage(1000);
            }
            fallDuration = 0f;
        }
        else
        {
            fallDuration += Time.deltaTime;
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }

    }
    
 
    public void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, alignSpeed);
    }
}
