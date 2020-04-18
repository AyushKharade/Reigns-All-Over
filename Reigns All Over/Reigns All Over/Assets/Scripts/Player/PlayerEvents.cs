﻿using System.Collections;
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
        CombatRef.ready = false;
        CombatRef.inCombat = false;
        animator.SetBool("ExitedCombat",false);

        //animator.SetLayerWeight(1, 0);
    }

    public void EndHurtingAnim()
    {
        animator.SetBool("Hurting",false);
        //if(!CombatRef.inCombat)
        //    animator.SetLayerWeight(1, 0);
    }

    /// <summary>
    /// called at the end of an attack, if player doesnt chain attacks.
    /// </summary>
    public void EndOfAttack()
    {
        CombatRef.attacking = false;
        CombatRef.chainAttack = false;
        animator.SetBool("Attacking",false);
        //animator.SetLayerWeight(2,0);         done smoothly
    }


    /// <summary>
    /// start chain window, so that players can chain attacks.
    /// </summary>
    public void EnableChainAttack()
    {
        CombatRef.chainAttack = true;
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
