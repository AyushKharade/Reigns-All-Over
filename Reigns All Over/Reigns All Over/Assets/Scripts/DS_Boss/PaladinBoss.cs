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
/// </summary>
public class PaladinBoss : MonoBehaviour
{

    [Header("List of all attacks")]
    public List<Attack_SO> lst = new List<Attack_SO>();
    
    [Header("Tracking all attacks")]
    public List<Attack> AttackLst = new List<Attack>();



    // Start is called before the first frame update
    void Start()
    {
        InitializeAllAttacks();

        // set a random available attack from the list.
        //Attack_SO at = GetAvailableAttack();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAttacksData();
    }

    // increments cooldown times and checks when cooldown has finished
    void UpdateAttacksData()
    {
        foreach (Attack at in AttackLst)
        {
            if (!at.isReady)         // if ready then dont need to process, that attack is available or doesnt have cooldowns
            {
                if (at.cooldownTimeLimit < at.cooldownTimer)
                {

                }
                else
                {
                    // its ready
                }
            }
        }
    }

    //Attack_SO GetAvailableAttack()
    //{
    //    List<Attack_SO> lst = new List<Attack_SO>();
    //    foreach(atta)
    //}


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
