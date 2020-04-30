﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// saves data such as health, stamina, gold, inventory, skills, experience etc.
/// </summary>
public class PlayerAttributes : MonoBehaviour
{
    [Header("Player Information")]
    // Health
    public float health=100;
    [HideInInspector]public float maxHealth;
    // stamina
    public float stamina=100;
    [HideInInspector] public float maxStamina;
    public float staminaRegenRate;
    public float staminaRegenDelay;
    public bool onStaminaRegenDelay;
    float staminaTimer;


    public float blockDMG_Absorb;

    // Mana
    [Header("Magic")]
    public int mana=6;                    // currently thinking slots of mana (like the 6 slots in Prince Of Persia)
    public int manaCapacity = 6;          // how many slots are unlocked.
                                                    //int maxMana = 6;
    public List<float> manaSlots=new List<float>();



    public int curManaSlotIndex;
    public float manaRegenRate;
    public float manaRegenDelay;
    bool onManaRegenDelay;
    float manaRegenTimer;
    [HideInInspector] public bool invincible;
    [HideInInspector] public bool dodgeInvincible;


    [Header("Character Development")]
    public CharacterDev character=new CharacterDev();

    [Header("Inventory")]
    public PlayerInventory inventory = new PlayerInventory();
    
    //references
    PlayerMovement MovementRef;
    Combat CombatRef;
    Animator animator;

    private void Start()
    {
        maxHealth = health;
        maxStamina = stamina;

        MovementRef = GetComponent<PlayerMovement>();
        CombatRef = GetComponent<Combat>();
        animator = GetComponent<Animator>();

        curManaSlotIndex = mana;

        InitManaSlots();
    }

    private void Update()
    {
        if (!MovementRef.isDead)
        {
            RegenStamina();
            RegenerateMana();
        }
    }

    /// <summary>
    /// Regenerates stamina based on regenerate value specified.
    /// </summary>
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


    /// <summary>
    /// Regenerates each mana stone one by one.
    /// </summary>
    void RegenerateMana()
    {
        if (curManaSlotIndex < manaCapacity)
        {
            if (onManaRegenDelay)
            {
                manaRegenTimer += Time.deltaTime;
                if (manaRegenTimer > manaRegenDelay)
                {
                    onManaRegenDelay = false;
                    manaRegenTimer = 0f;
                }
            }
            else
            {
                //regenerate current slot's value and move to next one once its full.
                if (manaSlots[curManaSlotIndex] >= 10)
                {
                    curManaSlotIndex++;
                    mana++;
                }
                else
                {
                    manaSlots[curManaSlotIndex] += Time.deltaTime;
                }
                
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




    public void HealPlayer(float HP)
    {
        health += HP;
        if (health > maxHealth)
            health = maxHealth;
    }


    /// <summary>
    /// Consume stamina from actions, called from other scripts that do actions.
    /// </summary>
    /// <param name="val"></param>
    public void ReduceStamina(float val)
    {
        stamina -= val;
        if (stamina < 0)
            stamina = 0;
        onStaminaRegenDelay = true;

    }

    public void ConsumeMana(int cost)
    {
        mana -= cost;
        onManaRegenDelay = true;

        for (int i = curManaSlotIndex-1; i >= mana; i--)
        {
            manaSlots[i] = 0f;      // so ui update reflects on this

        }

        //manaSlots[curManaSlotIndex-1] = 0f;      // so ui update reflects on this


        // also remove all charges from the next slot, this will be an upgrade so partial charges on a stone remain.
        if (mana!=manaCapacity-1)
            manaSlots[curManaSlotIndex] = 0f;      // so ui update reflects on this

        curManaSlotIndex=mana;
    }

    public bool HasEnoughMana(int val)
    {
        if (mana - val >= 0)
            return true;
        else
            return false;
    }


    public void InitManaSlots()
    {
        for (int i = 0; i < manaCapacity; i++)
        {
            manaSlots[i] = 10f;
        }
    }


    // combat stuff
    [Header("Enemies attacking")]
    public int currentAttackingEnemies;
    public int maxMeleeEnemies;

    public bool RequestAttack()
    {
        if (currentAttackingEnemies == maxMeleeEnemies)
            return false;
        else
        {
            currentAttackingEnemies++;
            return true;
        }

    }

    public void RetractAttack()
    {
        if(currentAttackingEnemies>0)
            currentAttackingEnemies--;
    }

}



//___________________________________________________________________________________________________________________________________________
// Additional classes for skills, etc.

/// <summary>
/// Seperate class to keep tracking of skills & XP
/// </summary>
[System.Serializable]
public class CharacterDev
{
    [Header("Experience")]
    public int currentXP;
    public int currentLevel;


    // UI references && message prefab to pop up when level up.
    //[Header("Other things")]
    //public GameObject 

    // functions
    public void AddExperience(int xp)
    {
        currentXP += xp;
    }

    void UpdateXP_UI()
    {

    }

}



[System.Serializable]
public class PlayerInventory
{
    [Header("Overview")]
    public int maxCarryWeight=60;
    public int curCarryWeight=7;

}