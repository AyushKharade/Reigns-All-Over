using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Combat : MonoBehaviour
{

    public Transform Target;
    public GameObject Weapon;
    public float mSpeed;
    public float alignSpeed;

    // references
    Transform NPCHolder;
    [HideInInspector]public Animator animator;

    //states
    public bool targetInRange;
    public bool commitedAttack;
    public bool isRunning;

    public float distance;

    float attackFrequency=1f;
    float attackTimer=0f;

    void Start()
    {
        NPCHolder = transform.parent;
        animator = GetComponent<Animator>();

        float r = Random.Range(-0.35f, 0.35f);
        attackFrequency += r;
    }

    void Update()
    {
        if (Target != null && !GetComponent<TestEnemyDummy>().isDead && !Target.GetComponent<PlayerMovement>().isDead)
        {
            CombatBehavior();
            distance = Vector3.Distance(transform.position, Target.position);
        }
        else if (Target != null && Target.GetComponent<PlayerMovement>().isDead)
            Target = null;
    }

    void CombatBehavior()
    {
        // seek the attack.
        if (!commitedAttack)
            AlignOrientation((Target.position-transform.position), alignSpeed);

        if (Vector3.Distance(transform.position, Target.position) > 1.35f && !commitedAttack)
        {
            targetInRange = false;
            Seek(Target.position, (Target.position - transform.position));
        }
        else
        {
            targetInRange = true;
            isRunning = false;
            if (animator.GetFloat("Locomotion") > 0)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);
        }



        if (targetInRange)
        {
            // new attack function
            AttackTarget();
        }
        else
        {
            attackTimer = 0f;
            commitedAttack = false;
        }
    }



    void AttackTarget()
    {
        //commitedAttack = true;
        attackTimer += Time.deltaTime;
        if (attackTimer > attackFrequency)// && Vector3.Angle(transform.forward, (Target.position - transform.position)) < 20)
        {
            commitedAttack = true;
            animator.SetBool("Attacking", true);
        }
        else
            AlignOrientation((Target.position - transform.position), alignSpeed);
    }
    


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" )
        {
            if(!other.GetComponent<PlayerMovement>().isDead)
                Target = other.transform;
        }
    }





    // functions for seek / align
    public void AlignOrientation(Vector3 dir, float newAlignSpeed)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, newAlignSpeed);
    }

    public void Seek(Vector3 pos, Vector3 direction)
    {
        if (Vector3.Angle(transform.forward, direction) < 20)
        {
            NPCHolder.Translate(mSpeed * transform.forward * Time.deltaTime);
            isRunning = true;

            if (animator.GetFloat("Locomotion") < 1)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") + 0.04f);
        }
        else
        {
            isRunning = false;
            if (animator.GetFloat("Locomotion") > 0)
                animator.SetFloat("Locomotion", animator.GetFloat("Locomotion") - 0.04f);
        }
    }
}
