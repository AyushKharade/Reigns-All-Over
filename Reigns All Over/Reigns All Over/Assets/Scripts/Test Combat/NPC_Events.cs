using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Events : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public void StopEnemyAttack()
    {
        GetComponent<NPC_Combat>().animator.SetBool("Attacking",false);
        GetComponent<NPC_Combat>().commitedAttack = false;
        GetComponent<NPC_Combat>().RandomizeAttackFrequency();

    }

    /// <summary>
    /// temporary shitting damage mechanics
    /// </summary>
    public void HurtPlayer()
    {
        if (GetComponent<NPC_Combat>().distance < GetComponent<NPC_Combat>().distanceToAttack)
        {
            if(Vector3.Angle(transform.forward,(GetComponent<NPC_Combat>().Target.position-transform.position))<15)
                GetComponent<NPC_Combat>().Target.GetComponent<PlayerAttributes>().DealDamage(GetComponent<NPC_Combat>().damage);
        }
    }
}
