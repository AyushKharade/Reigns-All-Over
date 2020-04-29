using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Combat Behavior
/// </summary>
public class NPC_CombatBehavior : MonoBehaviour
{
    [Header("Variables")]
    public float walkSpeed;
    public float runSpeed;
    public int attackDMG;
    public Transform attackTarget;

    // how close the target must be to attack
    public float distanceToTarget;

    [Header("States")]
    public bool equipped;          // whether npc has equipped his weapon
    public bool inCombat;          // whether npc is in direct combat
    public bool targetInSight;     // if target is close enough or direct raycast is possible.


    [Header("Weapon References")]
    public GameObject HandWeapon;
    public GameObject SheathWeapon;

    [Header("Test")]
    public bool engageTarget;

    // private
    Animator animator;
    NPC_Attributes NAttributesRef;
    NavMeshAgent navmeshRef;
    Transform Holder;

    

    void Start()
    {
        HandWeapon.SetActive(false);

        animator = GetComponent<Animator>();
        NAttributesRef = GetComponent<NPC_Attributes>();
        navmeshRef = GetComponent<NavMeshAgent>();
        Holder = transform.parent;

        //mSpeed = navmeshRef.speed;
    }

    void Update()
    {
        if (!NAttributesRef.isDead)
            EngageCombat();
    }

    void EngageCombat()
    {
        if (engageTarget)
        {
            // equip weapon
            if (!equipped)
                EquipWeapon();

            //if (Vector3.Distance(transform.position, attackTarget.position) < 10 || GetTargetInSight())
            if (GetTargetInSight())
                targetInSight = true;
            else
                targetInSight = false;
        }

        // if target is in sight, seek and attack, else use navmesh towards player.
        if (targetInSight )
        {
            navmeshRef.isStopped = true;
            EngageTarget();   
        }
        else // navemesh
        { }
    }


    /// <summary>
    /// move towards target and attack
    /// </summary>
    void EngageTarget()
    {
        if (Vector3.Distance(attackTarget.position, transform.position) > distanceToTarget)
        {
            //seek
            float movementFactor = 1f;
            Vector3 direction = (attackTarget.position - transform.position);
            if (Vector3.Angle(attackTarget.forward, transform.forward) > 15)
            {
                AlignOrientation(direction);
                movementFactor = 0.4f;
            }
            if (Vector3.Distance(attackTarget.position, transform.position) > distanceToTarget + 5)
            {
                Seek(direction, runSpeed * movementFactor);

                if (animator.GetFloat("Locomotion") < 1)
                    animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
            }
            else
            {
                Seek(direction, walkSpeed * movementFactor);

                if (animator.GetFloat("Locomotion") >0.52f)
                    animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);

                if (animator.GetFloat("Locomotion") <0.5f)
                        animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
            }
        }
        else
        {
            //attack
            if (animator.GetFloat("Locomotion") > 0)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);
        }
    }


    /// <summary>
    /// Return true if target is visible without any obstacles.
    /// </summary>
    /// <returns></returns>
    bool GetTargetInSight()
    {
        // do raycast towards enemy if enemy 
        Vector3 targetPos = attackTarget.position;
        targetPos.y += 1f;
        Vector3 castPos = transform.position;
        castPos.y += 1f;

        Vector3 raycastDir = (targetPos - castPos);


        Debug.DrawRay(castPos,raycastDir,Color.red);
        RaycastHit hit;
        Physics.Raycast(castPos,raycastDir,out hit);
        if (hit.collider.CompareTag("Player"))
            return true;
        return false;
            
    }


    void EquipWeapon()
    {
        equipped = true;

        animator.SetLayerWeight(1, 1);
        animator.SetBool("EquipWeapon", true);
        animator.SetBool("InCombat",true);

    }


    // animation event
    void EnterCombat()
    {
        HandWeapon.SetActive(true);
        SheathWeapon.SetActive(true);
        animator.SetBool("EquipWeapon",false);
    }

    // seek and align methods
    void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, 4f);
    }

    //void Seek(Vector3 pos,Vector3 direction)
    void Seek(Vector3 direction, float speed)
    {
        direction.y = 0f;
        Holder.Translate(speed*direction*Time.deltaTime);
    }
}
