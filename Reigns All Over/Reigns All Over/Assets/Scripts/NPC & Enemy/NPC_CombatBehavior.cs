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
    public float runSpeed;               // run speed maybe less if using root motion.
    public int attackDMG;
    public Transform attackTarget;
    

    // how close the target must be to attack
    public float distanceToTarget;

    [Header("States")]
    public bool equipped;          // whether npc has equipped his weapon
    public bool inCombat;          // whether npc is in direct combat
    public bool targetInSight;     // if target is close enough or direct raycast is possible.
    public bool usingNavmesh;


    [Header("Weapon References")]
    public GameObject HandWeapon;
    public GameObject SheathWeapon;

    [Header("testing vars")]
    public float velocityNavmesh;
    
    // private
    Animator animator;
    NPC_Attributes NAttributesRef;
    NavMeshAgent navmeshRef;
    Transform Holder;

    /// <summary>
    /// to track NPC's navmesh destination and so to check if this destination is too far from intended destination if so update it
    /// </summary>
    Vector3 navmeshTarget;
    

    void Start()
    {
        HandWeapon.SetActive(false);

        animator = GetComponent<Animator>();
        NAttributesRef = GetComponent<NPC_Attributes>();
        navmeshRef = GetComponent<NavMeshAgent>();
        Holder = transform.parent;

    }

    void Update()
    {
        if (attackTarget != null)
        {
            // check if they are dead already, then move on.
            EngageCombat();
        }



        // testing stuff
        velocityNavmesh = navmeshRef.velocity.magnitude;
    }


    /// <summary>
    /// Inits combat approach towards a new target, this is equip weapon if not already did so & chase after them.
    /// </summary>
    void EngageCombat()
    {
        if (!equipped)
            EquipWeapon();

        // determine distance.
        float dist = Vector3.Distance(transform.position, attackTarget.position);          // if its really close, you could probably seek it.

        if (dist > 10)
        {
            ChaseTargetNavmesh();
        }
        else if (GetTargetInSight())
        {
            //seek ?
            if (usingNavmesh)
            {
                usingNavmesh = false;
                navmeshRef.ResetPath();
            }
            //Debug.Log("Should attack");
            CombatBehavior();
        }
        else
        {
            ChaseTargetNavmesh();
        }
    }


    /// <summary>
    /// takes care of attacking target depending upon style specified in NAttributes
    /// </summary>
    void CombatBehavior()
    {

    }


    /// <summary>
    /// pursue target using navmesh
    /// </summary>
    void ChaseTargetNavmesh()
    {
        //Use navmesh
        if (!usingNavmesh)
        {
            navmeshTarget = attackTarget.position;
            navmeshRef.SetDestination(attackTarget.position);
            usingNavmesh = true;
        }
        else
        {
            // check if target moved to far away from the navmesh target, if yes, update pathfinding
            if (Vector3.Distance(navmeshTarget, attackTarget.position) > 5)
            {
                navmeshRef.ResetPath();
                navmeshRef.SetDestination(attackTarget.position);
                navmeshTarget = attackTarget.position;
                Debug.Log("Updated chasing location");
            }

            NavmeshAnimationUpdate();

        }
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


    void NavmeshAnimationUpdate()
    {
        float animValue = navmeshRef.velocity.magnitude/navmeshRef.speed;
        animator.SetFloat("Locomotion",animValue);


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




    private void OnTriggerEnter(Collider other)
    {
        // detect if this trigger is a hostile target, engage it, if theres a condition assigned to not stray away from actual location. Do not stray away.
        // for now, this is only player
        if (other.tag == "Player")
        {
            attackTarget = other.transform;
        }
    }
}
