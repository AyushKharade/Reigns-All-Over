using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for animation events and other stuff.
/// </summary>
public class PlayerEvents : MonoBehaviour
{

    //script references
    PlayerMovement MovementRef;
    Combat CombatRef;

    Animator animator;

    void Start()
    {
        MovementRef = GetComponent<PlayerMovement>();
        CombatRef = GetComponent<Combat>();

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Called at the end of animation of drawing weapon.
    /// </summary>
    public void EnterCombat()
    {
        CombatRef.ready = true;
        CombatRef.EquipWeapon();
        animator.SetBool("EnteredCombat",false);

    }

    public void ExitCombat()
    {
        //CombatRef.ready = false;
        CombatRef.inCombat = false;
        animator.SetBool("ExitedCombat",false);

        animator.SetLayerWeight(1, 0);
    }

    public void UnEquip()
    {
        CombatRef.UnEquipWeapon();
    }

    public void Equip()
    {
        CombatRef.EquipWeapon();
    }

}
