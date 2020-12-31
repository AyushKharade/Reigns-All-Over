using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Common Boss mechanics such as movement, etc. 
/// Specific boss behaviors are different scripts
/// </summary>
public class BossScript : MonoBehaviour
{

    public Transform targetRef;
    Animator animator;

    [Header("Dynamic data")]
    public BossProfile profileRef;
    public int health;
    int maxHealth;
    float alignSpeed;

    [Header("States")]
    public bool isDead;
    public bool isAttacking;

    // UI
    [Header("UI References")]
    public Transform bossHP_Parent;
    public Image HealthFG_UI;
    public Image HealthFG_CatchUp_UI;
    public Text BossNameUI;
    public float catchUpUI_Speed = 0.1f;
    float catchUp_Timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        health = profileRef.health;
        maxHealth = health;
        alignSpeed = profileRef.alignSpeed;

        animator = GetComponent<Animator>();

        // UI
        HealthFG_UI.fillAmount = health;
        HealthFG_CatchUp_UI.fillAmount = health;
        BossNameUI.text = profileRef.bName;
    }

    private void Update()
    {
        if (HealthFG_CatchUp_UI.fillAmount > HealthFG_UI.fillAmount && catchUp_Timer>0.8f)
        {
            HealthFG_CatchUp_UI.fillAmount -= catchUpUI_Speed;
            if (HealthFG_CatchUp_UI.fillAmount < HealthFG_UI.fillAmount)
                HealthFG_CatchUp_UI.fillAmount = HealthFG_UI.fillAmount;
        }
        catchUp_Timer += Time.deltaTime;
    }



    #region take damage and die section
    public int DealDamage(int dmg, Vector3 dir)
    {
        // if any buffs that reduce damage.
        float dmgMultiplier = 1f;                // if npc has any debuffs that makes it take more damage.

        if (Vector3.Angle(dir, transform.forward) < 30)
            dmgMultiplier = 1.5f;


        //total damage to take
        float dmgToTake = dmg * dmgMultiplier;

        health -= (int)(dmgToTake);
        if (health <= 0)
        {
            KillBoss();
            health = 0;
        }
        UpdateHealth_UI();



        return (int)dmgToTake;



    }

    void KillBoss()
    {
        Debug.Log("Boss is dead");
    }
    #endregion

    // Helper methods
    #region helper methods
    public void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, alignSpeed);
    }


    void UpdateHealth_UI()
    {
        HealthFG_UI.fillAmount = ((health * 1f) / maxHealth);
        catchUp_Timer = 0f;
    }


    #endregion

}
