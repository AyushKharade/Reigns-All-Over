using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Idle Behavior
/// </summary>
public class NPC_Behavior : MonoBehaviour
{
    // behaviors like wander, guard --> uses a list of transforms. Guard stationary, villager idle at random points, shopkeeper.
    // behavior scripts will not execute if incombat.

    public enum IdleBehaviorType { Guard, Villager_Wander, Follower};
    public IdleBehaviorType npcIdleBehavior = new IdleBehaviorType();

    //references
    NPC_CombatBehavior npcCombatRef;


    void Start()
    {
        npcCombatRef = GetComponent<NPC_CombatBehavior>();
    }

    void Update()
    {
        
    }
}
