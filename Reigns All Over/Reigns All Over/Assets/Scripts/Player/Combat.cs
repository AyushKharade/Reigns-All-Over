using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class deals with combat system and animations.
/// </summary>
public class Combat : MonoBehaviour
{
    #region Variables
    [Header("Combat States")]
    public bool inCombat;
    public bool ready;                 // if attacks can be done or not.
    

    public bool attacking;             // if any attack is being performed.
    public bool isBlocking;
    public bool isCastingSpell;
    /// <summary>
    /// If you blocked recently, opportunity to reposte attack.
    /// </summary>
    [HideInInspector]public float blockTime;          

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
    public int combo=1;                                  // use when higher combo should deal higher damage
    // controls Light/Heavy attack blend tree value
    [HideInInspector] public float attackAnimValue=7.5f;

    public float arenaline;                 // witcher adrenaline system, staying in combat builds up adrenaline and deal higher damage
    public float arenalineGainRate;

    Vector3 attackDirection;           // orient here.

    [Header("Rates / Cooldowns")]
    public float heavyAttackCost;

    [Header("Equipped Weapon")]
    public GameObject HandWeaponSlot;
    public GameObject SheathSlot;
    public GameObject EquippedWeapon;
    public GameObject SheathedWeapon;

    [Header("Equipped Spells")]
    public Transform CastHandRef;
    public GameObject EquippedSpell;
    public GameObject QuickSpell1;
    public GameObject QuickSpell2;



    // script ref
    PlayerMovement MovementRef;
    PlayerAttributes PAttributesRef;
    Animator animator;

    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        ready = false;

        UnEquipWeapon();

        MovementRef = GetComponent<PlayerMovement>();
        PAttributesRef = GetComponent<PlayerAttributes>();
    }

    void Update()
    {

        CombatControls();
        AttackOrient();
        SmoothSwitchOffCombatLayers();
        SmoothSwitchAttacks();

    }

    [HideInInspector]public float blockImpactAnimTimer=0f;
    private void FixedUpdate()
    {
        blockImpactAnimTimer += Time.deltaTime;
        if (animator.GetBool("BlockingImpact"))
        {
            if (blockImpactAnimTimer > 0.6f)
            {
                blockImpactAnimTimer = 0;
                animator.SetBool("BlockingImpact", false);
                animator.SetBool("BlockingImpactRepeat", false);
            }
            if(blockImpactAnimTimer>0.05f)
                animator.SetBool("BlockingImpactRepeat", false);
        }
        else
            blockImpactAnimTimer = 0f;


    }

    /// <summary>
    /// Equip & Unequip weapons, block and cast spells, Takes keyboard input and calls functions to attack/block/cast spells.
    /// </summary>
    void CombatControls()
    {

        if (Input.GetKeyDown(KeyCode.E) && !MovementRef.isDead)
        {
            if (inCombat && !MovementRef.isDodging && MovementRef.isGrounded && !attacking && !isCastingSpell)
            {
                animator.SetBool("inCombat", false);
                animator.SetBool("Hurting", false);
                animator.SetBool("ExitedCombat", true);         // does sheathing
                ready = false;
                //animator.SetBool("Hurting", false);
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

        // blocking controls
        if (ready && Input.GetKey(KeyCode.X) && !MovementRef.isDodging && !MovementRef.isDead && MovementRef.isGrounded)
        {
            isBlocking = true;
            animator.SetBool("Blocking",true);
            if (attacking)
                InteruptAttack();

            blockTime += Time.deltaTime;

            // blocking walk animation.
            BlockingMovementAnim();
        }
        else
        {
            isBlocking = false;
            blockTime = 0;
            animator.SetBool("Blocking", false);
        }




        // Spell Controls
        if (ready && !isBlocking && !MovementRef.isDead && !MovementRef.isDodging)
        {
            if (!isCastingSpell && Input.GetMouseButtonDown(2) && PAttributesRef.HasEnoughMana(EquippedSpell.GetComponent<DummyForceSpell>().cost))
            {
                PAttributesRef.ConsumeMana(EquippedSpell.GetComponent<DummyForceSpell>().cost);

                if (attacking)
                    InteruptAttack();

                isCastingSpell = true;
                animator.SetBool("CastingSpell",true);

                // read what type of spell it is and change spell blend value., also add if can afford from mana and deduct mana
                // make an interupt spell function.

                // if spell has condition that you cannot move dont make player move.
            }
        }
    }


    float attackStaminaCost=0f;
    /// <summary>
    /// receive input and attack direction from PlayerMovement, get type of attack and process in this function.s
    /// </summary>
    /// <param name="type"> Attack type: light(0) or heavy (1)</param>
    /// <param name="dir">Direction of attack so player can be oriented.</param>
    public void FightingControls(int type, Vector3 Dir)
    {
        attackDirection = Dir;
        attackAnimValue = type;

        attackStaminaCost = combo * heavyAttackCost * attackAnimValue;            // calculate heavy attack stamina cost
        if (!attacking)
        {

            if (PAttributesRef.stamina - attackStaminaCost >= 0)
            {
                animator.SetLayerWeight(2, 1);
                animator.SetBool("Attacking", true);
                animator.SetFloat("attackAnimValue", attackAnimValue);
                attacking = true;
                chained = false; chainAttack = false; chainWindowOpen = false;                    // just incase they were left on

                combo = 1;

                // turn on sword dmg
                EquippedWeapon.GetComponent<Weapon>().doDMG = true;

                PAttributesRef.ReduceStamina(attackStaminaCost);
            }

        }
        else if (attacking && chained)                     // clicked while attacking during the chain window
        {
            combo++;
            chainAttack = true;
            if (chainWindowOpen && ((PAttributesRef.stamina - attackStaminaCost) >= 0))
            {
                ExecuteChainAttack();
            }
        }
       
    }

    /// <summary>
    /// moves attacking animation to next in the combo, if there is no combo, it will act as if you didnt press the attack button.
    /// </summary>
    public void ExecuteChainAttack()
    {
        animator.SetFloat("attackAnimValue", attackAnimValue);           // chooses light or heavy.
        animator.SetLayerWeight(2, 1);
        animator.SetBool("Attacking", true);
        animator.SetBool("ChainAttack", true);
        chainAttack = false;
        chained = false;
        chainWindowOpen = false;
        attacking = true;

        // turn on sword dmg
        EquippedWeapon.GetComponent<Weapon>().doDMG = true;

        PAttributesRef.ReduceStamina(attackStaminaCost);
    }

    /// <summary>
    /// Smooth blending between light and heavy attacks.
    /// </summary>
    void SmoothSwitchAttacks()
    {
        if (attacking)
        {
            if (animator.GetFloat("attackAnimValue") < 1 && attackAnimValue == 1)
                animator.SetFloat("attackAnimValue",animator.GetFloat("attackAnimValue")+0.03f);
            else if (animator.GetFloat("attackAnimValue") > 0 && attackAnimValue == 0)
                animator.SetFloat("attackAnimValue", animator.GetFloat("attackAnimValue") - 0.03f);
        }
    }


    /// <summary>
    /// Orient towards the direction you attacked if you are not facing it already.
    /// </summary>
    void AttackOrient()
    {
        if (attacking)
            MovementRef.AlignOrientation(attackDirection);

        if (isBlocking)
        {
            Vector3 blockLookAt = Camera.main.transform.forward;
            blockLookAt.y = 0;
            MovementRef.AlignOrientation(blockLookAt);
        }

        if (MovementRef.isRunning && !MovementRef.isDead && attacking && !isBlocking && attackAnimValue==1)
            MovementRef.PlayerHolder.Translate(transform.forward * 1f * Time.deltaTime);
        else if(MovementRef.isRunning && !MovementRef.isDead && attacking && !isBlocking && attackAnimValue == 0)
            MovementRef.PlayerHolder.Translate(transform.forward * 0.5f * Time.deltaTime);



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

        // Smoothly turn off combat layer when player is dodging.
        if(MovementRef.isDodging && animator.GetLayerWeight(1)>0)
            animator.SetLayerWeight(1, animator.GetLayerWeight(1) - 0.025f);
    }



    /// <summary>
    /// Will interupt any attacks being done, usually called when you block / dodge / cast spell or get hurt.
    /// </summary>
    public void InteruptAttack()
    {
        animator.SetBool("Attacking", false);
        animator.SetBool("ChainAttack", false);
        attacking = false;
        chainAttack = false;
        chained = false;
        chainWindowOpen = false;

        combo = 1;

        EquippedWeapon.GetComponent<Weapon>().doDMG = false;
    }

    /// <summary>
    /// Interupts any current spell casting. Called when you block / dodge / attack or get hurt.
    /// </summary>
    public void InteruptSpellCast()
    {
        animator.SetBool("CastingSpell", false);
        isCastingSpell = false;
        Debug.Log("Interupted Spell Casting");
    }



    // spellcasting 
    /// <summary>
    /// Called from animation event, casts equipped spell if you have mana
    /// </summary>
    public void CastSpell()
    {
        GameObject spell = Instantiate(EquippedSpell,CastHandRef.position,Quaternion.identity);
        //spell.transform.LookAt(transform.forward);
    }



    /// <summary>
    /// Collects input for the 2D blend tree and also applies directonal translation for faster movement while blocking)
    /// </summary>
    void BlockingMovementAnim()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        animator.SetFloat("180X_Dir_Input",x);
        animator.SetFloat("180Y_Dir_Input",y);

        float strafeSpeed = 0.9f;
        //if (x != 0 && y != 0)
        //    strafeSpeed = 1.2f;

        MovementRef.PlayerHolder.Translate(MovementRef.PlayerDirection * strafeSpeed * Time.deltaTime);
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
