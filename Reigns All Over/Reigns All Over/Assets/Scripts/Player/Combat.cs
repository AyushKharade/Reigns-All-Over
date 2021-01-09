using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public bool isStunned;
    public bool isStunnedKnockedDown;
    public bool canDodgeFromStun;


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


    [Header("Archery States")]
    public bool archerAiming;             // Holding RMB
    public bool archerBowDraw;            // holding LMB
    public float archerDrawTime=0f;
    public bool nextShotReady;

    public GameObject aimReticleParent;
    public Image aimReticleFG;
    public GameObject equippedArrowPrefab;
    public float arrowShotForce;
    public Transform arrowShootPoint;

    [Header("Archer Aim Offsets")]
    public float xOffset_Aim;
    public float yOffset_Aim;


    [Header("Rates / Cooldowns")]
    public float heavyAttackCost;
    public float sprintAttackCost;

    [Header("Equipped Weapon")]
    public GameObject HandWeaponSlot;
    public GameObject SheathSlot;
    public GameObject EquippedWeapon;
    public GameObject SheathedWeapon;

    public GameObject EquippedBow;
    public GameObject SheathedBow;
    public GameObject HeldArrow;

    [Header("Equipped Spells")]
    public Transform CastHandRef;
    public GameObject EquippedSpell;
    public GameObject QuickSpell1;
    public GameObject QuickSpell2;

    public enum CurrentFightStyle { Melee, Archery};
    [Header("Current Weapon (Sword/Archery)")]
    public CurrentFightStyle fightStyle = new CurrentFightStyle();

    // script ref & other references
    PlayerMovement MovementRef;
    PlayerAttributes PAttributesRef;
    PlayerEvents PEventsRef;
    Animator animator;
    [HideInInspector]public Animator bowAnimator;

    GameObject mainCam;
    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        ready = false;

        UnEquipWeapon();

        MovementRef = GetComponent<PlayerMovement>();
        PAttributesRef = GetComponent<PlayerAttributes>();
        PEventsRef = GetComponent<PlayerEvents>();

        //fightStyle = CurrentFightStyle.Archery;                       // for now to test archery
        //animator.SetBool("usingArchery", true);
        UnEquipBow();

        //mainCam = Camera.main.gameObject;
        mainCam = MovementRef.CamRef.gameObject;

        bowAnimator = EquippedBow.transform.GetChild(0).GetComponent<Animator>();


    }

    void Update()
    {
        ChangeFightStyleInput();
        CombatControls();
        AttackOrient();
        SmoothSwitchOffCombatLayers();
        SmoothSwitchAttacks();
        UpdateAimReticle();



        // debug
        if (Input.GetKeyDown(KeyCode.Y))
        {
            StunPlayer(1f,transform.forward*1f);
        }

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



    // stunned modes
    public void StunPlayer(float stunValue, Vector3 stunDir)
    {
        if (!MovementRef.isDodging)
        {
            if (stunValue >= 1f)
                isStunnedKnockedDown = true;


            if (attacking)
                InteruptAttack();
            InteruptArchery();
            if (isCastingSpell)
                InteruptSpellCast();

            isStunned = true;

            animator.SetTrigger("Stunned");
            animator.SetFloat("StunValue", stunValue);

            if (Vector3.Angle(stunDir, transform.forward) < 90)
                animator.SetFloat("StunDir", 1f);
            else
                animator.SetFloat("StunDir", 0f);

            MovementRef.controlLock = true;
            PAttributesRef.ReduceStamina(0);
            animator.SetBool("Hurting", false);
        }
    }

    public void EndPlayerStun()
    {
        MovementRef.controlLock = false;
        isStunned = false;
        isStunnedKnockedDown = false;
    }

    /// <summary>
    /// Press a button to switch between melee and archery.
    /// </summary>
    public void ChangeFightStyleInput()
    {
        if (!MovementRef.isDead && !MovementRef.controlLock)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2) && fightStyle == CurrentFightStyle.Melee)
                ChangeFightStyleTo(CurrentFightStyle.Archery);
            if(Input.GetKeyDown(KeyCode.Alpha1) && fightStyle==CurrentFightStyle.Archery)
                ChangeFightStyleTo(CurrentFightStyle.Melee);
        }
    }


    /// <summary>
    /// actual do the process to change styles, update animatons and UI.
    /// </summary>
    /// <param name="style">Enumerator value of current fighting style.</param>
    void ChangeFightStyleTo(CurrentFightStyle style)
    {
        if (style == CurrentFightStyle.Archery)
        {
            fightStyle = CurrentFightStyle.Archery;

            if (inCombat)          // exit combat first the equip the other weapon
            {
                PEventsRef.ExitCombat();

                animator.SetBool("inCombat", false);
                animator.SetBool("Hurting", false);
                //animator.SetBool("ExitedCombat", true);         // does sheathing
                ready = false;
            }

            animator.SetBool("usingArchery", true);
        }
        else
        {
            fightStyle = CurrentFightStyle.Melee;
            if (inCombat)
                PEventsRef.ExitCombat();


            animator.SetBool("usingArchery", false);

        }
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
            else if (!MovementRef.isDodging && MovementRef.isGrounded && !attacking && !isCastingSpell)
            {
                if (fightStyle == CurrentFightStyle.Melee)
                    animator.SetLayerWeight(1, 1);
                else
                    animator.SetLayerWeight(3, 1);

                MovementRef.isWalking = false;
                inCombat = true;
                ready = false;
                animator.SetBool("inCombat", true);
                animator.SetBool("Hurting", false);
                animator.SetBool("EnteredCombat", true);
            }
        }

        // blocking controls
        if (ready && Input.GetKey(KeyCode.X) && !MovementRef.isDodging && !MovementRef.isDead && MovementRef.isGrounded && !PAttributesRef.blockRecovery
            && !isStunned)
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
        if (ready && !isBlocking && !MovementRef.isDead && !MovementRef.isDodging && fightStyle==CurrentFightStyle.Melee)
        {
            if (!isCastingSpell && Input.GetMouseButtonDown(2) && PAttributesRef.HasEnoughMana(EquippedSpell.GetComponent<Spell>().cost))
            {
                PAttributesRef.ConsumeMana(EquippedSpell.GetComponent<Spell>().cost);

                if (attacking)
                    InteruptAttack();

                isCastingSpell = true;
                animator.SetBool("CastingSpell",true);

                // read what type of spell it is and change spell blend value., also add if can afford from mana and deduct mana
                // make an interupt spell function.

                // if spell has condition that you cannot move dont make player move.
            }
        }


        // archer controls
        if (ready && fightStyle == CurrentFightStyle.Archery && archerBowDraw)
        {
            if (Input.GetMouseButton(0) && nextShotReady)
            {
                if(archerDrawTime<1f)
                    archerDrawTime += Time.deltaTime;
                animator.SetBool("BowShooting",true);
                bowAnimator.SetBool("BowDraw", true);

            }
            if (Input.GetMouseButtonUp(0) && archerDrawTime >= 0.3f)
            {
                animator.SetBool("BowShooting", false);
                animator.SetTrigger("BowShot");

                ShootArrow(archerDrawTime);
                archerDrawTime = 0f;
            }
            else if (Input.GetMouseButtonUp(0) && archerDrawTime < 0.5f)
            {
                InteruptArchery();
            }
        }
    }

    public GameObject aimingAt;

    /// <summary>
    /// Shooter arrow where aiming, use hold time for shot strength and damage.
    /// </summary>
    /// <param name="holdTime"></param>
    public void ShootArrow(float holdTime)
    {
        
        GameObject arrow = Instantiate(equippedArrowPrefab, arrowShootPoint.position, Quaternion.identity);
        // set damage based on holdTime.
  
        // find the direction of the shot
        Vector3 shotDirection=Vector3.zero;
        Ray ray = mainCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 500f))
            shotDirection = (hit.point - arrowShootPoint.position).normalized;
        else
            shotDirection = (ray.GetPoint(500f) - arrowShootPoint.position).normalized;


        arrow.transform.LookAt(shotDirection);
        arrow.GetComponent<Arrow>().shotDirection = shotDirection;

        shotDirection.y += 0.025f;
        nextShotReady = false;

        arrow.GetComponent<Rigidbody>().AddForce(shotDirection*arrowShotForce,ForceMode.Impulse);
        Destroy(arrow.gameObject, 4f);

        // bow anims
        bowAnimator.SetBool("BowShoot",true);
        bowAnimator.SetBool("BowDraw",false);
        Invoke("ResetBowAnimAfterShot",0.1f);
    }


    void ResetBowAnimAfterShot()
    {
        bowAnimator.SetBool("BowShoot", false);
    }



    //############################################# controls above


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

        attackStaminaCost = combo * heavyAttackCost; //* attackAnimValue;            // calculate heavy attack stamina cost
        if (!attacking)
        {

            if (PAttributesRef.stamina - attackStaminaCost >= 0 && !MovementRef.isSprinting)
            {
                animator.SetLayerWeight(2, 1);
                animator.SetBool("Attacking", true);
                animator.SetFloat("attackAnimValue", attackAnimValue);
                attacking = true;
                chained = false; chainAttack = false; chainWindowOpen = false;                    // just incase they were left on

                combo = 1;

                // turn on sword dmg
                EquippedWeapon.GetComponent<Weapon>().doDMG = true;

                if(attackStaminaCost>0)
                    PAttributesRef.ReduceStamina(attackStaminaCost);
            }
            else if (MovementRef.isSprinting && PAttributesRef.stamina - sprintAttackCost >= 0)
            {
                animator.SetLayerWeight(2, 1);
                animator.SetBool("SprintAttack", true);
                animator.SetFloat("attackAnimValue", attackAnimValue);
                attacking = true;
                chained = false; chainAttack = false; chainWindowOpen = false;                    // just incase they were left on

                if (attackAnimValue == 0) combo = 2; else combo = 5;                 // since these attacks cost a lot so do lot damage

                // turn on sword dmg
                //EquippedWeapon.GetComponent<Weapon>().doDMG = true;
                PAttributesRef.ReduceStamina(sprintAttackCost);
            }
            else
                WarnNoStamina();

        }
        else if (attacking && chained)                     // clicked while attacking during the chain window
        {
            combo++;
            chainAttack = true;
            if (chainWindowOpen && ((PAttributesRef.stamina - attackStaminaCost) >= 0))
            {
                ExecuteChainAttack();
            }
            else if ((PAttributesRef.stamina - attackStaminaCost) <= 0)
                WarnNoStamina();

        }
       
    }

    /// <summary>
    /// moves attacking animation to next in the combo, if there is no combo, it will act as if you didnt press the attack button.
    /// </summary>
    public void ExecuteChainAttack()
    {
        if (PAttributesRef.stamina - attackStaminaCost >= 0)
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

            if(attackStaminaCost>0)                         // so that light attacks dont delay regeneration
                PAttributesRef.ReduceStamina(attackStaminaCost);
        }
        else
        {
            WarnNoStamina();
        }
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

        if (isBlocking || archerBowDraw)
        {
            Vector3 blockLookAt = mainCam.transform.forward;
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
            if (animator.GetLayerWeight(1) > 0)                                   // melee upperbody layer
                animator.SetLayerWeight(1, animator.GetLayerWeight(1) - 0.02f);

            if (animator.GetLayerWeight(3) > 0)                                    // archer upperbody layer
                animator.SetLayerWeight(3, animator.GetLayerWeight(3) - 0.02f);

        }
        if (!attacking)
        {
            if (animator.GetLayerWeight(2) > 0)
                animator.SetLayerWeight(2, animator.GetLayerWeight(2) - 0.04f);
        }

        // Smoothly turn off combat layer when player is dodging.
        if (MovementRef.isDodging && animator.GetLayerWeight(1) > 0)
            animator.SetLayerWeight(1, animator.GetLayerWeight(1) - 0.025f);

        if (MovementRef.isDodging && animator.GetLayerWeight(3) > 0)
            animator.SetLayerWeight(3, animator.GetLayerWeight(3) - 0.025f);
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

        animator.SetBool("SprintAttack", false);
    }

    public void InteruptArchery()
    {
        archerBowDraw = false;
        animator.SetBool("BowDraw", false);
        bowAnimator.SetBool("BowDraw", false);
        bowAnimator.SetBool("BowShoot", false);
        animator.ResetTrigger("BowShot");
        animator.SetBool("BowShooting", false);
        archerDrawTime = 0f;
        //nextShotReady = false;
        nextShotReady = true;

        PEventsRef.Archer_HideHeldArrow();
        GetComponent<PlayerIK_Controller>().useIK = false;
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
        spell.GetComponent<Spell>().Target = this.gameObject;
        //spell.transform.LookAt(transform.forward);
    }



    /// <summary>
    /// Collects input for the 2D blend tree and also applies directonal translation for faster movement while blocking)
    /// </summary>
    void BlockingMovementAnim()
    {
        
        Get_XY_Movement_Anim_Values();

        float strafeSpeed = 0.9f;
        

        // add a skill condition
        MovementRef.PlayerHolder.Translate(MovementRef.PlayerDirection * strafeSpeed * Time.deltaTime);
    }

    /// <summary>
    /// This function updates the x & y animation values used for 2d free form blend trees.
    /// </summary>
    public void Get_XY_Movement_Anim_Values()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        animator.SetFloat("180X_Dir_Input", x);
        animator.SetFloat("180Y_Dir_Input", y);
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

    public void EquipBow()
    {
        EquippedBow.SetActive(true);
        SheathedBow.SetActive(false);
        nextShotReady = true;
    }

    public void UnEquipBow()
    {
        EquippedBow.SetActive(false);
        SheathedBow.SetActive(true);
    }

    /// <summary>
    /// Simply flash stamina BG bar to show that current action requires higher stamin
    /// </summary>
    void WarnNoStamina()
    {
        MovementRef.PlayerHolder.GetComponent<PlayerUI>().WarnNoStaminaUI();
    }


    void UpdateAimReticle()
    {
        if (archerBowDraw)
        {
            aimReticleParent.SetActive(true);
            aimReticleFG.fillAmount = (archerDrawTime / 1f);
        }
        else
        {
            aimReticleParent.SetActive(false);
        }
    }
}
