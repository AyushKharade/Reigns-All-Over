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

    /// <summary>
    /// if you get hit from behind while blocking, you wont be able to block for the next 'x' seconds.
    /// </summary>
    float blockRecoveryTimer=0f;

    /// <summary>
    /// is set true, if you get hit from behind while blocking, you canot block for the next 'x' seconds.
    /// </summary>
    [HideInInspector] public bool blockRecovery;


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

        // cooldown on blockrecovery
        if (blockRecovery)
        {
            blockRecoveryTimer += Time.deltaTime;
            if (blockRecoveryTimer > 1f)
            {
                blockRecovery = false;
                blockRecoveryTimer = 0f;
            }
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
                float rateModifier = 1f;
                if (CombatRef.isBlocking) rateModifier -= 0.5f;

                stamina += staminaRegenRate *rateModifier* Time.deltaTime;
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
   /// Use to deal damage to the player, specific a direction to see if player can block it
   /// </summary>
   /// <param name="dmg">damage amount in float</param>
   /// <param name="dir">direction of the attack</param>
   /// <returns>returns true if this attack killed the player</returns>
    public bool DealDamage(float dmg, Vector3 dir)
    {
        if (!MovementRef.isDead && !invincible && !dodgeInvincible)
        {
            // see if block was front front, else take full damage [Skill allows block from all directions]



            if (CombatRef.isBlocking && Vector3.Angle(dir, transform.forward) > 50)
            {
                health -= (dmg - dmg * blockDMG_Absorb);          // make sure to take into consideration blocktime from Combat


                Debug.Log("Blocked Damage: " + dmg + " >> " + (dmg - dmg * blockDMG_Absorb));
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

                if (CombatRef.isBlocking)
                    CancelPlayerBlock();   
            } 

            if (!CombatRef.inCombat)
                animator.SetLayerWeight(1, 1);

            if (CombatRef.attacking)
                CombatRef.InteruptAttack();
            if (CombatRef.isCastingSpell)
                CombatRef.InteruptSpellCast();
            if (CombatRef.archerBowDraw)
                CombatRef.InteruptArchery();

            if (health <= 0)
            {
                health = 0;
                KillPlayer();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// sets block recovery bool to true, player cannot block for the next 'x' seconds. should be small enough.
    /// </summary>
    public void CancelPlayerBlock()
    {
        blockRecovery = true;
        blockRecoveryTimer = 0f;
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