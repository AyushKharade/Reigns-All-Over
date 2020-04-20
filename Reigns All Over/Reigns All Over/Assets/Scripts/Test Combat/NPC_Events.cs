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
    }

    /// <summary>
    /// temporary shitting damage mechanics
    /// </summary>
    public void HurtPlayer()
    {
        if (GetComponent<NPC_Combat>().distance < 1.35f)
        {
            GetComponent<NPC_Combat>().Target.GetComponent<PlayerAttributes>().DealDamage(22);
        }
    }
}
