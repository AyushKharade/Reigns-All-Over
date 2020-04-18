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
        SmoothSwitchOffCombatLayer();
    }

    void CombatControls()
    {

        if (Input.GetKeyDown(KeyCode.E) && !MovementRef.isDead)
        {
            if (inCombat && !MovementRef.isDodging && MovementRef.isGrounded)
            {
                animator.SetBool("inCombat", false);
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
                animator.SetBool("EnteredCombat", true);
            }
        }
    }

    void SmoothSwitchOffCombatLayer()
    {
        if (!inCombat)
        {
            if (animator.GetLayerWeight(1) > 0)
                animator.SetLayerWeight(1, animator.GetLayerWeight(1)-0.02f);
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
