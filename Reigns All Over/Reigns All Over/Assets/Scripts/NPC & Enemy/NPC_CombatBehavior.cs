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

    public List<AttackDetails> attacks = new List<AttackDetails>();
    int noOfAttacks;
    

    // how close the target must be to attack
    [Header("Distance Parameters")]
    public float attackDistance;
    /// <summary>
    /// how close does the npc need to be before they can request to attack
    /// </summary>
    public float distanceToRequestAttack;

    [Header("Approach States")]
    public bool equipped;          // whether npc has equipped his weapon
    public bool inCombat;          // whether npc is in direct combat
    public bool targetInSight;     // if target is close enough or direct raycast is possible.
    public bool usingNavmesh;

    [Header("Combat States")]
    public bool attacking;
    // can he attack player
    public bool hasAttackPermission;
    public float attackFrequency=1f;
    float ogAttackFrequency;


    [Header("Weapon References")]
    public GameObject HandWeapon;
    public GameObject SheathWeapon;

    [Header("testing vars")]
    public float distanceToTarget;

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

        noOfAttacks = attacks.Count;
        ogAttackFrequency = attackFrequency;

        HandWeapon.SetActive(false);
    }

    void Update()
    {
        if (attackTarget != null && !NAttributesRef.isDead)
        {
            // check if they are dead already, then move on.
            EngageCombat();
        }


        if (!attacking && animator.GetLayerWeight(2) > 0)
            animator.SetLayerWeight(2, animator.GetLayerWeight(2) - 0.02f);

        // testing stuff
        distanceToTarget = Vector3.Distance(attackTarget.position,transform.position);
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
            //seek 
            if (usingNavmesh)
            {
                usingNavmesh = false;
                navmeshRef.ResetPath();
            }
            CombatBehavior(dist);

        }
        else
        {
            ChaseTargetNavmesh();
        }
    }


    /// <summary>
    /// takes care of attacking target depending upon style specified in NAttributes
    /// </summary>
    void CombatBehavior(float distance)
    {
        /*
         * Strategy
         * Once this method is called, NPCs are close enough with sight with their target.
         * if target is any npc, just attack regularly
         * if its a player, its trick, we should make sure that not a lot of enemies attack at once, and that they surround the player.
         * 
         * PAttributes has variable 'maxattackers'.
         * Once NPC has sight of player, they move closer by walking until they reach distance to attack.
         * 
         * Once they reach this distance, they will request attack. if they get back true, they can go ahead and attack.
         * if they get false, means enough enemies are already attacking. in this case do flanking behavior. or simply they can stand and taunt.
         * 
         * Cooldown on requesting attack again if you got back false or you finished your attack.
         * heavy attacks should retract so that npc can recover, for normal atttacks, NPCs may not retract attack permission (free it to someone else)
         * 
         */

        Vector3 directionTowardsTarget = (attackTarget.position - transform.position);
        if (attackTarget.CompareTag("Player"))
        {
            /*
            // move until request distance.
            if (distance > distanceToRequestAttack)
            {
                animator.SetFloat("Locomotion", 0.5f);  // walk
                AlignOrientation(directionTowardsTarget);
                Seek(directionTowardsTarget, walkSpeed);
            }
            else if (distance > attackDistance)
            {

                //request attack
                hasAttackPermission = attackTarget.GetComponent<PlayerAttributes>().RequestAttack();

                if (hasAttackPermission)
                {
                    // move in closer and attack
                    animator.SetFloat("Locomotion", 0.5f);  // walk
                    AlignOrientation(directionTowardsTarget);
                    Seek(directionTowardsTarget, walkSpeed);
                }
                else
                    animator.SetFloat("Locomotion", 0f);  // stop


            }

            // if has perimission and close enough to player, attack
            if (hasAttackPermission && distance <= attackDistance)
            {

            }
            */
            //--------------------------------------------------------------------
            // npc vs npc code just to see
            // get close enough and attack.
            if (distance > attackDistance)
            {
                animator.SetFloat("Locomotion", 0.5f);  // walk
                AlignOrientation(directionTowardsTarget);
                Seek(directionTowardsTarget, walkSpeed);
            }
            else // attack
            {
                animator.SetFloat("Locomotion", 0f);  // walk
                AttackingBehavior();

            }
        }




        // targets are other npcs.
        else if (!attackTarget.GetComponent<NPC_Attributes>().isDead)// npc, dont request attacks
        {
            // get close enough and attack.
            if (distance > attackDistance && !attacking)
            {
                animator.SetFloat("Locomotion", 0.5f);  // walk
                AlignOrientation(directionTowardsTarget);
                Seek(directionTowardsTarget, walkSpeed);
            }
            else if(!attacking)// attack
            {
                animator.SetFloat("Locomotion", 0f);  // walk
                AttackingBehavior();

            }
        }

        else
        {
            Debug.Log("Target is dead");
            attackTarget = null;
            canAttack = false;
            attackTimer = 0f;
        }
    }


    float attackTimer = 0f;
    bool canAttack;
    int selectedAttackDMG;
    /// <summary>
    /// Do different types of attack based on behavior
    /// </summary>
    void AttackingBehavior()
    {
        string attackType = NAttributesRef.Get_NPC_CombatType();
        Vector3 targetDirection = (attackTarget.position - transform.position);

        if (attackTimer > attackFrequency)
        {
            canAttack = true;
            attackTimer = 0f;
        }
        else
        {
            attackTimer += Time.deltaTime;
        }



        // diferent behaviors
        if (attackType == "OneHanded")
        {

            if (Vector3.Angle(transform.forward, targetDirection ) < 15 && canAttack && !attacking)
            {

                // roll and choose which attack out of many given, randomize attack speed (anim speed)

                float r = Random.Range(0f, 1f);
                int whichAttack = 0;
                for (int i = 0; i < attacks.Count; i++)
                {
                    whichAttack = i;
                    if (r <= attacks[i].frequency)
                        break;
                }


                // got which attack is about to happen.
                animator.SetFloat("AttackAnim",attacks[whichAttack].blendValue);
                selectedAttackDMG = attacks[whichAttack].attackDamage;


                animator.SetLayerWeight(2,1);
                animator.SetBool("Attacking",true);
                attacking = true;
            }
            else if(!attacking)
                AlignOrientation(targetDirection,2f);


        }
    }

    void AttackTarget()
    {
        if (attackTarget.tag != "Player")
        {
            if (Vector3.Angle(transform.forward, (attackTarget.position - transform.position)) < 30)
                attackTarget.GetComponent<NPC_Attributes>().DealDamageNPC(selectedAttackDMG, transform.forward);
        }
        else
        {
            if (Vector3.Angle(transform.forward, (attackTarget.position - transform.position)) < 20 && Vector3.Distance(transform.position,attackTarget.position)<attackDistance)
                attackTarget.GetComponent<PlayerAttributes>().DealDamage(selectedAttackDMG,transform.forward);
        }
    }

    
    public void EndNPCAttack()
    {
        RandomizeAttackFrequency();
        animator.SetBool("Attacking",false);
        attacking = false;
        attackTimer = 0f;
        canAttack = false;
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
    /*
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
    */


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

        if (hit.collider.CompareTag("Player") || hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC"))
            return true;
       
        return false;
            
    }


    void EquipWeapon()
    {
        equipped = true;

        animator.SetLayerWeight(1, 1);
        animator.SetBool("EquipWeapon", true);
        animator.SetBool("InCombat",true);

        SheathWeapon.SetActive(false);

    }


    // animation event
    void EnterCombat()
    {
        HandWeapon.SetActive(true);
        SheathWeapon.SetActive(true);
        animator.SetBool("EquipWeapon",false);
    }

   

    void RandomizeAttackFrequency()
    {
        float r = Random.Range(-0.3f, 0.3f);
        attackFrequency += r;

        if (attackFrequency < ogAttackFrequency - 0.75f || attackFrequency > ogAttackFrequency + 0.75f)
        {
            //Debug.Log("Attack Frequency was reset.");
            attackFrequency = ogAttackFrequency;
        }
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

    void AlignOrientation(Vector3 dir, float speed )
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, speed);
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


[System.Serializable]
public struct AttackDetails
{
    [Range(0,1)]
    public float frequency;
    public int attackDamage;
    public float blendValue;
    /// <summary>
    /// should enemy give up its retract permission after attack? useful if attack is heavy and npc needs to recover
    /// </summary>
    public bool retracts;
}
