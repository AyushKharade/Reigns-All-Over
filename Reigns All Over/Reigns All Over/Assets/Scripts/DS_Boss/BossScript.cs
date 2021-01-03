﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Common Boss mechanics such as movement, etc. 
/// Specific boss behaviors are different scripts
/// </summary>
public class BossScript : MonoBehaviour
{
    // References
    public Transform targetRef;
    Animator animator;

    [Header("Dynamic data")]
    public BossProfile profileRef;
    public int health;
    int maxHealth;
    float alignSpeed;
    public bool shouldAlign;           // controlled by animation events and conditions, etc, if false, boss wont align towards target.
    public float chanceToBlock;        // chance that the boss will raise their shields

    [Header("States")]
    public bool isDead;
    public bool isAttacking;
    public bool isTargetDead;
    public bool isBlocking;
    public bool isCatchingBreath;              // after every attack the boss will wait x seconds, when this is true, boss will not move or align
    public bool disableUpperBodyLayer;
    public bool playerDefeated;

    [Header("Buff & Debuff abilities")]
    public float catchBreathMultiplier = 1f;


    public Attack_SO attackRef;                  // references to the current attack that the boss will be doing
    public bool isInRangeForCurAttack;           
    public float curAttackRange=4f;
    public float curDamageRange;
    float defaultRange = 4f;
    public bool attackSet;                       // boolean to show whether an attack was set from the boss behavior script
    


    // UI
    [Header("UI References")]
    public Transform bossHP_Parent;
    public Image HealthFG_UI;
    public Image HealthFG_CatchUp_UI;
    public Text BossNameUI;
    public float catchUpUI_Speed = 0.08f;
    float catchUp_Timer = 0f;


    [Header("Sound Ref")]
    public AudioClip footstepSound;

    // Start is called before the first frame update
    void Start()
    {
        health = profileRef.health;
        maxHealth = health;
        alignSpeed = profileRef.alignSpeed;

        animator = GetComponent<Animator>();

        // UI
        HealthFG_UI.fillAmount = health;
        HealthFG_CatchUp_UI.fillAmount = health;
        BossNameUI.text = profileRef.bName;

        //sound
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (attackRef == null)
            curAttackRange = defaultRange;
        // just for simple movement, move towards player

        if (!isDead && !isCatchingBreath && !playerDefeated)
        {
            if(shouldAlign)
                AlignOrientation((targetRef.position-transform.position).normalized);


            if (!isAttacking)
            {
                BossMovement();
                UpperBodyLayer_Control();
            }

        }



        




        //
        if (HealthFG_CatchUp_UI.fillAmount > HealthFG_UI.fillAmount && catchUp_Timer>0.8f)
        {
            HealthFG_CatchUp_UI.fillAmount -= catchUpUI_Speed;
            if (HealthFG_CatchUp_UI.fillAmount < HealthFG_UI.fillAmount)
                HealthFG_CatchUp_UI.fillAmount = HealthFG_UI.fillAmount;
        }
        catchUp_Timer += Time.deltaTime;


        
    }



    #region Movement 
    void BossMovement()
    {

        float distanceSqrMag = (targetRef.position - transform.position).sqrMagnitude;

        // move
        if (distanceSqrMag > curAttackRange * curAttackRange && distanceSqrMag < 8f * 8f)        // walk towards if curDistance > attackRange & < 8 units
        {
            if (animator.GetFloat("MovementY") < 1f)
                animator.SetFloat("MovementY", animator.GetFloat("MovementY") + 0.05f);
            else if (animator.GetFloat("MovementY") > 1f)
                animator.SetFloat("MovementY", animator.GetFloat("MovementY") - 0.01f);

        }
        else if (distanceSqrMag > 8f * 8f)                                                     // sprint towards if curDistance > 8 units
        {
            if (animator.GetFloat("MovementY") < 2f)
                animator.SetFloat("MovementY", animator.GetFloat("MovementY") + 0.05f);
        }
        else if (distanceSqrMag < (curAttackRange-1.5f)*(curAttackRange-1.5f))  // && has space behind               // walk backward if too close
        {

            // if has space behind, move back, else go towards center?
            if (animator.GetFloat("MovementY") > -1f)
                animator.SetFloat("MovementY", animator.GetFloat("MovementY") - 0.02f);
        }
        else                                                                                   // stop, in proper range
        {
            if (animator.GetFloat("MovementY") > 0f)
            {
                animator.SetFloat("MovementY", animator.GetFloat("MovementY") - 0.04f);
                if (animator.GetFloat("MovementY") < 0.1f) animator.SetFloat("MovementY", 0f);
            }
            if (animator.GetFloat("MovementY") < 0f)
            {
                animator.SetFloat("MovementY", animator.GetFloat("MovementY") + 0.04f);
                if (animator.GetFloat("MovementY") > -0.1f) animator.SetFloat("MovementY", 0f);
            }
            
        }




        // update boolean whether inrange or no
        if (distanceSqrMag <= curAttackRange * curAttackRange)
            //&&
            //distanceSqrMag >= (curAttackRange-1.5f)*(curAttackRange-1.5f)
            //)
            isInRangeForCurAttack = true;
        else
            isInRangeForCurAttack = false;
    }


    #endregion

    #region set attacks from boss behaviors
    public void SetBossAttack(Attack_SO at)
    {

        isInRangeForCurAttack = false;
        curAttackRange = at.rangeNeeded;
        curDamageRange = at.rangeToDamagetarget;
        attackRef = at;
        
        attackSet = true;
        //Debug.Log("Boss attack set");
    }

    public void ClearBossAttack()
    {
        float bossCatchUpTime = attackRef.catchUpTime;

        isInRangeForCurAttack = false;
        attackRef = null;
        curAttackRange = defaultRange;
        attackSet = false;
        isAttacking = false;

        shouldAlign = true;

        // once attack ends, depending on the attack made, the boss will catch breath and not move / align for a while (read value from attack so)
        if (bossCatchUpTime > 0)
        {
            isCatchingBreath = true;
            Invoke("DisableBossCatchUp", bossCatchUpTime*catchBreathMultiplier);

            if (animator.GetFloat("MovementY") != 0) animator.SetFloat("MovementY", 0f);
            if (animator.GetFloat("MovementX") != 0) animator.SetFloat("MovementX", 0f);
        }
        else
            DisableBossCatchUp();



        // turn around to face player if player is behind.
        //if (Vector3.Angle(transform.forward, (targetRef.position- transform.position) )> 35)
        //    animator.SetTrigger("Turn180");
    }

    /// <summary>
    /// Boss will stop catching their breath and get back to fighting
    /// </summary>
    void DisableBossCatchUp()
    {
        isCatchingBreath = false;
        
        // turn around to face player if player is behind.
        if (Vector3.Angle(transform.forward, (targetRef.position- transform.position) )> 35)
            animator.SetTrigger("Turn180");

        // can choose to shield until next attack
        int no = Random.Range(0, 100);
        if (no < chanceToBlock)
        {
            StartBlocking();
        }
    }


    public void StartBlocking() {
        isBlocking = true;
        animator.SetBool("ShieldUp", true);
    }
    public void EndBlocking() {
        isBlocking = false;
        animator.SetBool("ShieldUp", false);
    }




    #endregion

    // anim event for boss attack
    public void AttackTarget()
    {
        // if certain angle and distance

        float dist = (targetRef.position - transform.position).sqrMagnitude;
        float angle = Vector3.Angle(transform.forward, (targetRef.position - transform.position).normalized);

        bool playerKilled=false;

        if (dist <= curDamageRange * curDamageRange && angle < 15)
            playerKilled=targetRef.GetComponent<PlayerAttributes>().DealDamage(attackRef.damage, transform.forward);

        // cur attack can stun, do stun

        // if target dies, outcome, include finisher anim too :P

        // if certain angle --> do finisher

        // Victory anim.
        if (playerKilled)
        {
            playerDefeated = true;
            animator.SetTrigger("PlayerDead");
            //animator.SetFloat("VictoryAnimValue", Random.Range(0, 100f));
            animator.SetFloat("VictoryAnimValue", 100);
            animator.SetLayerWeight(2, 1);
        }


    }



    #region take damage and die section
    public int DealDamage(int dmg, Vector3 dir)
    {
        // if any buffs that reduce damage.
        float dmgMultiplier = 1f;                // if npc has any debuffs that makes it take more damage.

        float angle = Vector3.Angle(dir, transform.forward);
        if (angle<30)
            dmgMultiplier += 1f;
        if (isBlocking && angle > 30)
        {
            dmgMultiplier -= 0.75f;
            // playshield sound
        }


        //total damage to take
        float dmgToTake = dmg * dmgMultiplier;

        health -= (int)(dmgToTake);
        if (health <= 0)
        {
            KillBoss();
            health = 0;
        }
        UpdateHealth_UI();


        // hurt anim
        if (!isAttacking)
        {
            animator.SetTrigger("Hurt");
            if (Vector3.Angle(dir, transform.forward) < 35)       // means a backstab
                animator.SetFloat("HurtValue", 1f);
            else
                animator.SetFloat("HurtValue", 0f);
        }


        return (int)dmgToTake;
    }

    void KillBoss()
    {
        Debug.Log("Boss is dead");
        animator.SetTrigger("isDead");
        isDead = true;
        animator.SetLayerWeight(1, 0);
    }
    #endregion

    // Helper methods
    #region helper methods
    public void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, alignSpeed);
    }


    public void UpdateHealth_UI()
    {
        HealthFG_UI.fillAmount = ((health * 1f) / maxHealth);
        if (HealthFG_CatchUp_UI.fillAmount < HealthFG_UI.fillAmount) HealthFG_CatchUp_UI.fillAmount = HealthFG_UI.fillAmount;
        catchUp_Timer = 0f;
    }


    void DisableAlign()
    {
        shouldAlign = false;
    }

    void EnableAlign()
    {
        shouldAlign = true;
    }

    #endregion


    #region anim events & anim layers

    /// <summary>
    /// depending on states of the boss, this later controls the 2nd layer which is responsible for shield, parry and hurt anims
    /// </summary>
    void UpperBodyLayer_Control()
    {
        // turn on & off upper body layer whenever

        bool upperLayerEnable=true;

        if (isAttacking || isDead || disableUpperBodyLayer)
            upperLayerEnable = false;


        // smoothly enable / disable layers
        if (upperLayerEnable)
        {
            if (animator.GetLayerWeight(1) < 1f)
                animator.SetLayerWeight(1,animator.GetLayerWeight(1) + 0.06f);
        }
        else
        {
            if (animator.GetLayerWeight(1) > 0f)
                animator.SetLayerWeight(1, animator.GetLayerWeight(1) - 0.08f);
        }

    }

    public void  EnableUpperBodyControl() { disableUpperBodyLayer = false; Debug.Log("Called anim event to enable upperbody again"); }
    public void DisableUpperBodyControl() { disableUpperBodyLayer = true; }

    
    #endregion

    #region audio
    AudioSource audioSource;
    public void PlayFootStep()
    {
        audioSource.PlayOneShot(footstepSound);
    }

    #endregion
}
