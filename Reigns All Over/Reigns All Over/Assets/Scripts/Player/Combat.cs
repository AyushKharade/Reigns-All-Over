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
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inCombat && !MovementRef.isDodging)
            {
                animator.SetBool("inCombat", false);
                animator.SetBool("ExitedCombat", true);         // does unsheathing
            }
            else if(!MovementRef.isDodging)
            {
                animator.SetLayerWeight(1, 1);
                inCombat = true;
                ready = false;
                animator.SetBool("inCombat", true);
                animator.SetBool("EnteredCombat",true);
            }
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
