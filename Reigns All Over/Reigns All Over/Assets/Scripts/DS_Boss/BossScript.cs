using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Common Boss mechanics such as movement, etc. 
/// Specific boss behaviors are different scripts
/// </summary>
public class BossScript : MonoBehaviour
{

    public Transform targetRef;
    Animator animator;

    [Header("Dynamic data")]
    public BossProfile profileRef;
    public int health;
    int maxHealth;
    float alignSpeed;
    public bool shouldAlign;           // controlled by animation events and conditions, etc.

    [Header("States")]
    public bool isDead;
    public bool isAttacking;

    public Attack_SO attackRef;
    public bool isInRangeForCurAttack;
    public float curAttackRange=4f;
    float defaultRange = 4f;
    public bool attackSet;
    

    //[Header("Debug")]

    // UI
    [Header("UI References")]
    public Transform bossHP_Parent;
    public Image HealthFG_UI;
    public Image HealthFG_CatchUp_UI;
    public Text BossNameUI;
    public float catchUpUI_Speed = 0.1f;
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

        if (!isDead)
        {
            if(shouldAlign)
                AlignOrientation((targetRef.position-transform.position).normalized);

            float distanceSqrMag = (targetRef.position - transform.position).sqrMagnitude;

            if (!isAttacking)
            {
                // move

                if (distanceSqrMag > curAttackRange * curAttackRange && distanceSqrMag < 8f * 8f)        // walk
                {
                    if (animator.GetFloat("MovementY") < 1f)
                        animator.SetFloat("MovementY", animator.GetFloat("MovementY") + 0.05f);
                    else if (animator.GetFloat("MovementY") > 1f)
                        animator.SetFloat("MovementY", animator.GetFloat("MovementY") - 0.01f);

                }
                else if (distanceSqrMag > 8f * 8f)                                                     // sprint
                {
                    if (animator.GetFloat("MovementY") < 2f)
                        animator.SetFloat("MovementY", animator.GetFloat("MovementY") + 0.05f);
                }
                else                                                                                   // stop
                {
                    if (animator.GetFloat("MovementY") > 0f)
                        animator.SetFloat("MovementY", animator.GetFloat("MovementY") - 0.04f);
                }
            }

            if (distanceSqrMag <= curAttackRange * curAttackRange)
                isInRangeForCurAttack = true;
            else
                isInRangeForCurAttack = false;

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



    #region Movement Script




    #endregion

    #region set attacks from boss behaviors
    public void SetBossAttack(Attack_SO at)
    {

        isInRangeForCurAttack = false;
        curAttackRange = at.rangeNeeded;
        attackRef = at;
        if (attackRef == null)
            Debug.Log("fuck");

        attackSet = true;
        //Debug.Log("Boss attack set");
    }

    public void ClearBossAttack()
    {
        isInRangeForCurAttack = false;
        attackRef = null;
        curAttackRange = defaultRange;
        attackSet = false;
        isAttacking = false;

        shouldAlign = true;
        //Debug.Log("BossAttack Cleared");
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

    // anim event for boss attack
    public void AttackTarget()
    {
        // if certain angle and distance

        float dist = (targetRef.position - transform.position).sqrMagnitude;
        float angle = Vector3.Angle(transform.forward, (targetRef.position - transform.position).normalized);

        if (dist <= curAttackRange*curAttackRange && angle < 15)
            targetRef.GetComponent<PlayerAttributes>().DealDamage(attackRef.damage, transform.forward);
        else
            Debug.Log("Attaacking failed");
        // cur attack can stun, do stun


        // clear old attack
        //ClearBossAttack();
    }



    #region take damage and die section
    public int DealDamage(int dmg, Vector3 dir)
    {
        // if any buffs that reduce damage.
        float dmgMultiplier = 1f;                // if npc has any debuffs that makes it take more damage.

        if (Vector3.Angle(dir, transform.forward) < 30)
            dmgMultiplier = 1.5f;


        //total damage to take
        float dmgToTake = dmg * dmgMultiplier;

        health -= (int)(dmgToTake);
        if (health <= 0)
        {
            KillBoss();
            health = 0;
        }
        UpdateHealth_UI();



        return (int)dmgToTake;



    }

    void KillBoss()
    {
        Debug.Log("Boss is dead");
        animator.SetTrigger("isDead");
        isDead = true;
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


    void UpdateHealth_UI()
    {
        HealthFG_UI.fillAmount = ((health * 1f) / maxHealth);
        catchUp_Timer = 0f;
    }


    #endregion


    #region audio
    AudioSource audioSource;
    public void PlayFootStep()
    {
        audioSource.PlayOneShot(footstepSound);
    }

    #endregion
}
