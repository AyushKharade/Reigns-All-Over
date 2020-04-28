using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Stores data & general functions.
/// </summary>
public class NPC_Attributes : MonoBehaviour
{
    [Header("Details")]
    public string npcName;
    [TextArea(3, 3)]
    public string desc;

    [Header("Variables")]
    public int health;

    public enum NPC_Type { Villager, Guard, Creature, Bandit};
    public NPC_Type npcType = new NPC_Type();

    public enum combat_Type { Flee, Melee, Magic, Creature};
    public combat_Type combatType = new combat_Type();

    [Header("Navmesh Test")]
    public Transform destination;
    bool destinationSet;
    public float velocity;


    // references
    private NavMeshAgent navmeshRef;
    private Animator animator;

    private void Start()
    {
        navmeshRef = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!destinationSet)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                destinationSet = true;
                GoToDestination();
            }
        }
        velocity = navmeshRef.velocity.magnitude;
    }

    void GoToDestination()
    {
        navmeshRef.Warp(transform.position);
        navmeshRef.SetDestination(destination.position);
        animator.SetFloat("Locomotion", 1f);

        
    }
}
