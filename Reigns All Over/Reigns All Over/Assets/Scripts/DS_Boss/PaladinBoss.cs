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

    [Header("List of all attacks")]
    public List<Attack_SO> lst = new List<Attack_SO>();
    
    [Header("Tracking all attacks")]
    public List<Attack> AttackLst = new List<Attack>();

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
    }

    // Update is called once per frame
    void Update()
    {
        if (!bossScriptRef.isDead)
        {
            UpdateAttacksData();
            BossCoolDown();
            if(!isOnAttackCooldown && !bossScriptRef.attackSet)
                AttackDecisions();

            // if attacks are set
            if (bossScriptRef.attackSet && bossScriptRef.isInRangeForCurAttack)
            {
                Debug.Log("Called do attack.");
                DoAttack();
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
    }


    // there will be a small chance that everytime the player attacks the boss can do an attack immedietely, given they werent attacking
    void BossCoolDown()
    {
        if (isOnAttackCooldown)
        {
            bossCooldownTimer += Time.deltaTime;
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



    //
    void AttackDecisions()
    {
        // if this function is called means can attack
        float distance = (bossScriptRef.targetRef.position - transform.position).sqrMagnitude;
        // make decisions based on available attacks and distance to player.

        if (isAttackAvailable("3Combo"))
        {
            // set it in boss script, it will be done as soon as player is in range.
            bossScriptRef.SetBossAttack(GetAttack("3Combo").attackSO);
        }
        else if (isAttackAvailable("JumpAttack"))
        {
            bossScriptRef.SetBossAttack(GetAttack("JumpAttack").attackSO);
        }
        else
        {
            bool trueOrFalse = (Random.value > 0.5f);

            if(trueOrFalse)
                bossScriptRef.SetBossAttack(GetAttack("Slash1").attackSO);
            else
                bossScriptRef.SetBossAttack(GetAttack("Slash2").attackSO);

        }



    }


    /// <summary>
    /// Perform the current attack set in boss script
    /// </summary>
    void DoAttack()
    {
        animator.SetTrigger(bossScriptRef.attackRef.animatorStateName);
        bossScriptRef.isAttacking=true;
        SetBossOnCooldown();
        if (GetAttack(bossScriptRef.attackRef.name).attackSO.hasCooldown)
        {
            GetAttack(bossScriptRef.attackRef.name).ExpendAttack();
        }
        //bossScriptRef.ClearBossAttack();
    }
}

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
