using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// saves data such as health, stamina, gold, inventory, skills, experience etc.
/// </summary>
public class PlayerAttributes : MonoBehaviour
{
    [Header("Player Information")]
    public float health=100;
    [HideInInspector]public bool invincible;
    public bool dodgeInvincible;
    public float stamina=100;
    public float staminaRegenRate;
    public float staminaRegenDelay;
    public bool onStaminaRegenDelay;
    float staminaTimer;
    public float blockDMG_Absorb;
    public int mana=10;                    // currently thinking 10 slots of mana (like the 6 slots in Prince Of Persia)

    [Header("Character")]
    public int level;
    public int currentXP;

    [Header("Temp UI")]
    public Text HP;

    //references
    PlayerMovement MovementRef;
    Combat CombatRef;
    Animator animator;

    private void Start()
    {
        MovementRef = GetComponent<PlayerMovement>();
        CombatRef = GetComponent<Combat>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(!MovementRef.isDead)
            RegenStamina();

        HP.text = "Health: "+health+"";
    }

    void RegenStamina()
    {
        if (stamina < 100)
        {
            if (onStaminaRegenDelay)
            {
                staminaTimer += Time.deltaTime;
                if (staminaTimer > staminaRegenDelay)
                {
                    onStaminaRegenDelay = false;
                    staminaTimer = 0;
                }
            }
            else
            {
                stamina += staminaRegenRate * Time.deltaTime;
            }
        }
    }


    // Health Related
    /// <summary>
    /// General Damage from Environment, make a new function for enemies that includes a direction of attack,
    /// </summary>
    /// <param name="dmg"> How much damage to deal.</param>
    public void DealDamage(float dmg)
    {
        if (!MovementRef.isDead && !invincible && !dodgeInvincible)
        {
            if (CombatRef.isBlocking)
            {
                health -= (dmg - dmg * blockDMG_Absorb);          // make sure to take into consideration blocktime from Combat


                Debug.Log("Blocked Damage: "+dmg +" >> "+ (dmg - dmg * blockDMG_Absorb));
                if (!animator.GetBool("BlockingImpact"))
                    animator.SetBool("BlockingImpact", true);
                else
                    animator.SetBool("BlockingImpactRepeat", true);
                CombatRef.blockImpactAnimTimer = 0f;
            }
            else
            {
                health -= dmg;
                animator.SetBool("Hurting", true);
            }
            if (!CombatRef.inCombat)
                animator.SetLayerWeight(1, 1);

            if (CombatRef.attacking)
                CombatRef.InteruptAttack();
            if (CombatRef.isCastingSpell)
                CombatRef.InteruptSpellCast();

            if (health <= 0)
            {
                health = 0;
                KillPlayer();
            }
        }
      
    }
    
    void KillPlayer()
    {
        MovementRef.isDead = true;
        animator.SetBool("isDead",true);
        animator.SetLayerWeight(1, 0);    // Turn off combat layer since death is on first

    }
}
