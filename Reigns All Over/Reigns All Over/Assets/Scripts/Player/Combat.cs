using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class deals with combat system and animation
/// </summary>
public class Combat : MonoBehaviour
{
    #region Variables
    [Header("Combat States")]
    public bool inCombat;              // 
    public bool ready;                 // if attacks can be done or not.
    // Fighting states
    // --> Uses a new layer to do combat, 4 blend trees each with two animations (light & heavy attacks)

    public bool attacking;             // if any attack is being performed.

    /// <summary>
    /// Is enabled after which chain input will be accepted.
    /// </summary>
    public bool chained;               
    /// <summary>
    /// If user chained attack during the chain window (Window Duration: StartChain to EndAttack)
    /// </summary>
    public bool chainAttack;           

    /// <summary>
    /// Shows if chaining can be performed. (input can be accepted before attack can be chained. If this value is true, it means 
    /// attack can be chained. Dont have to wait)
    /// </summary>
    [HideInInspector]public bool chainWindowOpen;
    public int combo;                                  // use when higher combo should deal higher damage
    // controls Light/Heavy attack blend tree value
    float attackAnimValue;             

    Vector3 attackDirection;           // orient here.

    [Header("Equipped Weapon")]
    public GameObject HandWeaponSlot;
    public GameObject SheathSlot;
    public GameObject EquippedWeapon;
    public GameObject SheathedWeapon;


    // script ref
    PlayerMovement MovementRef;

    Animator animator;

    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        ready = false;

        UnEquipWeapon();

        MovementRef = GetComponent<PlayerMovement>();
    }

    void Update()
    {

        CombatControls();
        AttackOrient();
        SmoothSwitchOffCombatLayers();
    }

    /// <summary>
    /// Equip & Unequip weapons
    /// </summary>
    void CombatControls()
    {

        if (Input.GetKeyDown(KeyCode.E) && !MovementRef.isDead)
        {
            if (inCombat && !MovementRef.isDodging && MovementRef.isGrounded && !attacking)
            {
                animator.SetBool("inCombat", false);
                animator.SetBool("Hurting", false);
                animator.SetBool("ExitedCombat", true);         // does unsheathing
                ready = false;
            }
            else if (!MovementRef.isDodging && MovementRef.isGrounded)
            {
                animator.SetLayerWeight(1, 1);
                MovementRef.isWalking = false;
                inCombat = true;
                ready = false;
                animator.SetBool("inCombat", true);
                animator.SetBool("Hurting", false);
                animator.SetBool("EnteredCombat", true);
            }
        }
    }



    /// <summary>
    /// receive input and attack direction from PlayerMovement, get type of attack and process in this function.s
    /// </summary>
    /// <param name="type"> Attack type: light(0) or heavy (1)</param>
    /// <param name="dir">Direction of attack so player can be oriented.</param>
    public void FightingControls(int type, Vector3 Dir)
    {
        attackDirection = Dir;
        attackAnimValue = type;
        if (!attacking)
        {
            animator.SetFloat("attackAnimValue", type);           // chooses light or heavy.
            animator.SetLayerWeight(2, 1);
            animator.SetBool("Attacking", true);

            attacking = true;
            chained = false; chainAttack = false; chainWindowOpen = false;                    // just incase they were left on
        }
        else if (attacking && chained)                     // clicked while attacking during the chain window
        {
            chainAttack = true;
            if (chainWindowOpen)
                ExecuteChainAttack();
        }
       
    }

    public void ExecuteChainAttack()
    {
        animator.SetFloat("attackAnimValue", attackAnimValue);           // chooses light or heavy.
        animator.SetLayerWeight(2, 1);
        animator.SetBool("Attacking", true);
        animator.SetBool("ChainAttack", true);
        chainAttack = false;
        chained = false;
        attacking = true;
    }


    void AttackOrient()
    {
        if (attacking)
            MovementRef.AlignOrientation(attackDirection);
    }


    /// <summary>
    /// Slowly turns off combat layer (layer 2, index 1)
    /// </summary>
    void SmoothSwitchOffCombatLayers()
    {
        if (!inCombat && !animator.GetBool("Hurting"))
        {
            if (animator.GetLayerWeight(1) > 0)
                animator.SetLayerWeight(1, animator.GetLayerWeight(1)-0.02f);
        }
        if (!attacking)
        {
            if (animator.GetLayerWeight(2) > 0)
                animator.SetLayerWeight(2, animator.GetLayerWeight(2) - 0.04f);
        }
    }

    // these methods will just turn on and off weapons
    public void EquipWeapon()
    {
        EquippedWeapon.SetActive(true);
        SheathedWeapon.SetActive(false);

    }

    public void UnEquipWeapon()
    {
        EquippedWeapon.SetActive(false);
        SheathedWeapon.SetActive(true);
    }

    
}
