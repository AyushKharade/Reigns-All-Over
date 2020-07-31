using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for mostly using animation events and other stuff.
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

    
    /// <summary>
    /// Called at the end of animation of drawing weapon. Ready Boolean will be toggled so player can attack.
    /// </summary>
    public void EnterCombat()
    {
        CombatRef.ready = true;

        if (CombatRef.fightStyle == Combat.CurrentFightStyle.Melee)
            CombatRef.EquipWeapon();
        else
            CombatRef.EquipBow();
        animator.SetBool("EnteredCombat",false);
    }

    /// <summary>
    /// Called at the end of unsheathing animation.
    /// </summary>
    public void ExitCombat()
    {
        CombatRef.ready = false;
        CombatRef.inCombat = false;
        animator.SetBool("ExitedCombat",false);
    }

    public void EndHurtingAnim()
    {
        animator.SetBool("Hurting",false);
    }

    /// <summary>
    /// called at the end of an attack animation, if player doesnt chain attacks or if there are no more combos.
    /// </summary>
    public void EndOfAttack()
    {
        //if (!CombatRef.chainAttack)                 // not sure if this condition check is necessary.
        //{
            CombatRef.attacking = false;
            CombatRef.chainAttack = false;
            CombatRef.chained = false;
            CombatRef.chainWindowOpen = false;
            animator.SetBool("Attacking", false);
            animator.SetBool("ChainAttack", false);

            CombatRef.combo = 1;

            if (animator.GetBool("SprintAttack"))
                animator.SetBool("SprintAttack", false);

          //  Debug.Log("EndOFAttack Called");
        //}
    }


    /// <summary>
    /// start chain window, so that players can chain attacks.
    /// </summary>
    public void EnableChainAttack()
    {
        if(!MovementRef.isDodging)
            CombatRef.chained = true;

        // reset older variables.
        animator.SetBool("ChainAttack", false);
        CombatRef.chainAttack = false;
        CombatRef.chainWindowOpen = false;

    }

    /// <summary>
    /// if user had pressed the chain attack button previously, perform chain now.
    /// </summary>
    public void PerformChainAttack()
    {
        // experimental -- turn off attacking state for weapon.
        if (!CombatRef.chainAttack)
            CombatRef.EquippedWeapon.GetComponent<Weapon>().doDMG = false;

        if (!MovementRef.isDodging)
            CombatRef.chainWindowOpen = true;

        if(!MovementRef.isDodging && CombatRef.chainAttack)
            CombatRef.ExecuteChainAttack();
    }

    /// <summary>
    /// Grants invincibility while dodging (Starts at a particular frame in the dodge animation.
    /// </summary>
    void EnableDodgeInvincibility()
    {
        GetComponent<PlayerAttributes>().dodgeInvincible = true;
    }

    /// <summary>
    /// This will be a skill, allows you to change direction while dodging. (Gives more fluid dodge movement)
    /// </summary>
    void AllowDodgingOrient()
    {
        MovementRef.allowDodgeOrient = true;
    }


    /// <summary>
    /// Call Cast spell function in Combat Script, will cast the currenly equipped spell. 
    /// </summary>
    void CastSpell()
    {
        CombatRef.CastSpell();
    }

    /// <summary>
    /// Anim event that calls at the end of an animation to turn off variables related to spell casting.
    /// </summary>
    void EndSpell()
    {
        CombatRef.isCastingSpell = false;
        animator.SetBool("CastingSpell",false);
    }




    
    /// <summary>
    /// Simply called by sheathing anim, calls unequip on Combat
    /// </summary>
    public void UnEquip()
    {
        if (CombatRef.fightStyle == Combat.CurrentFightStyle.Melee)
            CombatRef.UnEquipWeapon();
        else
            CombatRef.UnEquipBow();
    }

    /// <summary>
    /// Calls equip() on combat while drawing weapon
    /// </summary>
    public void Equip()
    {
        if (CombatRef.fightStyle == Combat.CurrentFightStyle.Melee)
            CombatRef.EquipWeapon();
        else
            CombatRef.EquipBow();
    }

}
