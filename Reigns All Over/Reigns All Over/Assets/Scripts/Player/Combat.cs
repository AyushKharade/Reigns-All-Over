using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class deals with combat system and animation
/// </summary>
public class Combat : MonoBehaviour
{
    [Header("Combat States")]
    public bool inCombat;              // 
    public bool ready;                 // if attacks can be done or not.
    // Fighting states
    // --> Uses a new layer to do combat, 4 blend trees each with two animations (light & heavy attacks)

    public bool attacking;             // if an attack is in progress
    public bool chainAttack;           // enabled within the time window of a previous attack. If you attack again, it will chain.
    public int combo;                  // to indicate which combo you are on
    float attackAnimValue;             // used to play fast / heavy attacks using a blend tree

    Vector3 attackDirection;           // orient here.

    [Header("Equipped Weapon")]
    public GameObject HandWeaponSlot;
    public GameObject SheathSlot;
    public GameObject EquippedWeapon;
    public GameObject SheathedWeapon;


    // script ref
    PlayerMovement MovementRef;

    Animator animator;

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
            if (inCombat && !MovementRef.isDodging && MovementRef.isGrounded)
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
        if (!attacking)
        {
            animator.SetFloat("attackAnimValue", type);           // chooses light or heavy.
            animator.SetLayerWeight(2, 1);
            animator.SetBool("Attacking", true);

            attacking = true;
        }
        else if (attacking && chainAttack)          // clicked while attacking during the chain window
        {
            animator.SetFloat("attackAnimValue", type);           // chooses light or heavy.
            animator.SetLayerWeight(2, 1);
            animator.SetBool("Attacking", true);
            animator.SetBool("ChainAttack", true);
            chainAttack = false;
            attacking = true;
        }
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
