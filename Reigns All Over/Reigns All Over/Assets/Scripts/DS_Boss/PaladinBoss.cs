using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Boss scripts are unique behaviors, they use any helper methods from the common boss script such as is target visible, move, align, navmesh
/// , UI whenever required. Their behavoir is directed from this script.
/// 
/// Paladin Boss:
/// A sword and shield warrior who has a variety of attacks.
/// Imp things to note:
/// - runs towards you if you are too far
/// - walks in 4 directions to position
/// - can rotate 180 if you stay behind him
/// - certain attacks can orient with you.
/// - try to make a finisher if the attacking blow is a killer.
/// when on 25% HP, knocks the player back and triggers Adrenaline mode, has special VFX, and all animation speeds are faster recovers health back to 50%
/// 
/// Attacks and their definitions:
/// Boss attacks and then has a random cooldown on their next attack, which is short, 4-8 seconds, they have a chance to raise their shields
/// 
/// - 3Combo - whenever available
/// - Hardkick - to break player out of blocking or if they get really close
///            - if player is blocking, standing stun, else knockback
///            
///  - Jump attack - whenever available, at a distance (can also be part of slash1-slash2 combo)
///  - slash 1 - most basic attack which has no cooldown, chance to chain to next attack in combo (slash2)
///  - slash 2 - similar as above, chance to chain into jump attack
///  
///  - quick hit - if player is too close and not blocking, causes standing stun - low damage
///  - spin attack - small cooldown, whevnever avaiable
///  - jumping spin attack - medium cooldown
///  - special attack - available after adrenaline phase
/// </summary>
public class PaladinBoss : MonoBehaviour
{
    [Header("variables & states")]

    public float bossCooldown;
    float bossCooldownTimer;
    public bool isOnAttackCooldown;

    public float randomCooldownLowRange=2f;
    public float randomCooldownHighRange=5f;

    [Header("Second Phase")]
    public float secondPhaseHP_Percent=30;                 // at what percent remaining health will the second phase trigger.
    public float secondPhaseHP_RaiseTo = 60;
    bool startedSecondPhase;
    [Header("List of all attacks")]
    public List<Attack_SO> lst = new List<Attack_SO>();
    
    [Header("Tracking all attacks")]
    public List<Attack> AttackLst = new List<Attack>();

    [Header("Boss Music Ref")]
    public GameObject BossMusicObjRef;
    BossMusic BossMusicRef;

    //referneces
    BossScript bossScriptRef;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        bossScriptRef= GetComponent<BossScript>();
        InitializeAllAttacks();
        SetBossOnCooldown();

        // set a random available attack from the list.
        //Attack_SO at = GetAvailableAttack();
        bossScriptRef.attackRef = lst[0];
        BossMusicRef = BossMusicObjRef.GetComponent<BossMusic>();
        

        animator.SetFloat("AnimSpeed", 1f);
        bossScriptRef.bossHP_Parent.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (bossScriptRef.fightEngaged)
        {
            if (!bossScriptRef.isDead || !bossScriptRef.playerDefeated)
            {
                if (!startedSecondPhase)
                    MonitorForSecondPhase();

                UpdateAttacksData();
                CancelBlockingIfNeeded();

                BossCoolDown();
                if (!isOnAttackCooldown && !bossScriptRef.attackSet && !bossScriptRef.isAttacking)
                    AttackDecisions();

                // if attacks are set
                if (bossScriptRef.attackSet && bossScriptRef.isInRangeForCurAttack && !bossScriptRef.isAttacking && !isOnAttackCooldown)
                {
                    float angle = Vector3.Angle(transform.forward, (bossScriptRef.targetRef.position - transform.position).normalized);
                    if (angle < 20)
                        DoAttack();
                }
            }
        }













        // debug
        if (Input.GetKeyDown(KeyCode.O))
        {
            foreach (Attack at in AttackLst)
            {
                if (at.attackSO.hasCooldown)
                    at.ExpendAttack();
            }
        }

        if (Input.GetKeyDown(KeyCode.L) && !bossScriptRef.fightEngaged)
        {
            EngageBossFight();
        }
    }

    public void EngageBossFight()
    {
        bossScriptRef.fightEngaged = true;
        bossScriptRef.bossHP_Parent.gameObject.SetActive(true);
        BossMusicRef.SetAudioClip(BossMusicRef.paladinBossMusic1, true, 1f);
        BossMusicRef.PlayCurrentClip();

    }

    /// <summary>
    /// Looks at how much time there is left for cooldown to finish, in the last second, disable it.
    /// </summary>
    void CancelBlockingIfNeeded()
    {
        if (bossScriptRef.isBlocking)
        {
            if (bossCooldownTimer >= (bossCooldown - 1f))
                bossScriptRef.EndBlocking();
        }
    }


    // there will be a small chance that everytime the player attacks the boss can do an attack immedietely, given they werent attacking
    void BossCoolDown()
    {
        if (isOnAttackCooldown)
        {
            bossCooldownTimer += Time.deltaTime;
            bossScriptRef.bossCooldownTimeRemaining = bossCooldown - bossCooldownTimer;
            if (bossCooldownTimer >= bossCooldown)
            {
                bossCooldownTimer = 0f;
                isOnAttackCooldown = false;
            }
        }
    }

    void SetBossOnCooldown()
    {
        isOnAttackCooldown = true;
        bossCooldown = Random.Range(randomCooldownLowRange, randomCooldownHighRange);
    }

    void SetBossOnCooldown(float time)
    {
        isOnAttackCooldown = true;
        bossCooldown = time;
    }

    // increments cooldown times and checks when cooldown has finished
    void UpdateAttacksData()
    {
        foreach (Attack at in AttackLst)
        {
            if (!at.isReady)         // if ready then dont need to process, that attack is available or doesnt have cooldowns
            {
                at.cooldownTimer += Time.deltaTime;
                if (at.cooldownTimer < at.cooldownTimeLimit)
                {
                    // nothing
                }
                else
                {
                    at.ReadyAttack();
                }
            }
        }
    }

    

    void InitializeAllAttacks()
    {
        foreach (Attack_SO at in lst)
        {
            Attack newAttack=new Attack();
            newAttack.attackSO = at;
            newAttack.cooldownTimeLimit = at.cooldown;
            AttackLst.Add(newAttack);
        }
    }


    bool isAttackAvailable(string attackName)
    {
        foreach (Attack at in AttackLst)
        {
            if (at.attackSO.attackname.CompareTo(attackName) == 0)
            {
                if (at.attackSO.isSecondPhaseAttack && !startedSecondPhase)
                    return false;

                if (at.isReady)
                    return true;
                else
                    return false;
            }
        }
        Debug.Log("requested attack wasnt in the attacklist");
        return false;
    }

    Attack GetAttack(string attackName)
    {
        foreach (Attack at in AttackLst)
        {
            if (attackName.CompareTo(at.attackSO.attackname) == 0)
            {
                return at;
            }
        }
        Debug.Log("Specified attack not found in inventory");
        return null;
    }


    bool stunAttackOnCooldown;

    void RemoveStunAttackCooldown() { stunAttackOnCooldown = false; }
    //
    void AttackDecisions()
    {
        // if this function is called means can attack
        float distance = (bossScriptRef.targetRef.position - transform.position).sqrMagnitude;
        // make decisions based on available attacks and distance to player.

        // if really close do stun attacks if available
        bool attackSet=false;
        if (distance < (3.2f * 3.2f) && Random.Range(0,100)<80)
        {
            if (stunAttackOnCooldown && isAttackAvailable("HardKick") && isAttackAvailable("QuickHit"))
            {
                bool trueOrFalse = (Random.value > 0.5f);
                if (trueOrFalse) bossScriptRef.SetBossAttack(GetAttack("HardKick").attackSO);
                else bossScriptRef.SetBossAttack(GetAttack("QuickHit").attackSO);
                attackSet = true;
                stunAttackOnCooldown = true;
                Invoke("RemoveStunAttackCooldown", 5f);
            }
            else if (isAttackAvailable("HardKick")) { bossScriptRef.SetBossAttack(GetAttack("HardKick").attackSO); attackSet = true; }
            else if (isAttackAvailable("QuickHit")) { bossScriptRef.SetBossAttack(GetAttack("QuickHit").attackSO); attackSet = true; }

            if (attackSet) {
                stunAttackOnCooldown = true;
                Invoke("RemoveStunAttackCooldown", 5f);
            }
        }

        else if (!attackSet)
        {
            // check if can do combo
            if (isAttackAvailable("3Combo") || isAttackAvailable("4Combo"))
            {
                if (isAttackAvailable("3Combo") && isAttackAvailable("4Combo"))
                {
                    bool trueOrFalse = (Random.value > 0.5f);

                    if (trueOrFalse)
                        bossScriptRef.SetBossAttack(GetAttack("3Combo").attackSO);
                    else
                        bossScriptRef.SetBossAttack(GetAttack("4Combo").attackSO);
                }
                else if(isAttackAvailable("4Combo"))
                    bossScriptRef.SetBossAttack(GetAttack("4Combo").attackSO);
                else if(isAttackAvailable("3Combo"))
                    bossScriptRef.SetBossAttack(GetAttack("3Combo").attackSO);


            }
            // check jump attack
            else if (isAttackAvailable("JumpAttack"))
            {
                bossScriptRef.SetBossAttack(GetAttack("JumpAttack").attackSO);
            }
            else
            {
                bool trueOrFalse = (Random.value > 0.5f);

                if (trueOrFalse)
                    bossScriptRef.SetBossAttack(GetAttack("Slash1").attackSO);
                else
                    bossScriptRef.SetBossAttack(GetAttack("Slash2").attackSO);

            }

        }

    }


    /// <summary>
    /// Perform the current attack set in boss script
    /// </summary>
    void DoAttack()
    {
        animator.SetTrigger(bossScriptRef.attackRef.animatorStateName);
        bossScriptRef.isAttacking=true;


        if (!GetAttack(bossScriptRef.attackRef.name).attackSO.overrideBossCooldown)
            SetBossOnCooldown();
        else
            SetBossOnCooldown(GetAttack(bossScriptRef.attackRef.name).attackSO.overrideBossCooldownTime);


        if (GetAttack(bossScriptRef.attackRef.name).attackSO.hasCooldown)
        {
            GetAttack(bossScriptRef.attackRef.name).ExpendAttack();
        }
    }


    #region Phase 2

    void MonitorForSecondPhase()
    {
        if (bossScriptRef.health <= bossScriptRef.profileRef.health * (secondPhaseHP_Percent / 100f))
            StartSecondPhase();
    }

    void StartSecondPhase()
    {
        if (bossScriptRef.isAttacking)
            bossScriptRef.ClearBossAttack();


        // PlaySound and start anim, etc.
        animator.SetTrigger("StartPhase2");
        startedSecondPhase = true;
        animator.SetFloat("AnimSpeed", 1.2f);
        bossScriptRef.DisableUpperBodyControl();

        BossMusicRef.SetAudioClip(BossMusicRef.paladinBossMusic2, true, 1f);
        BossMusicRef.PlayCurrentClip();

        // reduce times he can block
        bossScriptRef.chanceToBlock = 40f;

        // boost catchbreath Speed by x percent
        bossScriptRef.catchBreathMultiplier = 0.65f;

        // reset HP:
        bossScriptRef.health = (int)(bossScriptRef.profileRef.health * (secondPhaseHP_RaiseTo/100f));
        bossScriptRef.UpdateHealth_UI();
    }
    #endregion
}




//######################################################### class
[System.Serializable]
public class Attack
{
    public Attack_SO attackSO;
    public float cooldownTimeLimit;
    public float cooldownTimer;
    public bool isReady;

    public void ExpendAttack()
    {
        if (attackSO.hasCooldown)
        {
            cooldownTimer = 0f;
            isReady = false;
        }
    }

    public void ReadyAttack()
    {
        isReady = true;
        cooldownTimer = 0f;
    }
}
