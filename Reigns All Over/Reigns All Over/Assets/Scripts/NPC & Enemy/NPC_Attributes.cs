using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    int maxHealth;
    public bool isDead;




    // enums
    public enum NPC_Type { Villager, Guard, Creature, Bandit};
    public NPC_Type npcType = new NPC_Type();

    public enum combat_Type { Flee, OneHanded, Shield, TwoHanded,Magic, Creature};
    public combat_Type combatType = new combat_Type();

    public enum stateTowardsPlayer { Neutral, Ally, Vendor, Hostile};
    public stateTowardsPlayer playerRelation = new stateTowardsPlayer();


    [Header("NPC_HP_UI")]

    public Image HP;
    public Transform UIParent;
    public Transform CameraRef;

    private void Start()
    {
        maxHealth = health;
        CameraRef = Camera.main.transform;
    }

    private void Update()
    {
        // billboard the ui
        if (!isDead)
            UIParent.rotation = CameraRef.rotation;
        
    }


    /// <summary>
    /// deal damage to an npc if applicable. That is wont hurt non-enemies.
    /// </summary>
    /// <param name="dmg">How much damage to inflict</param>
    /// <param name="dir">Your direction. Skill can make attacks from behind do more damage.</param>
    /// <returns>returns amount of damage dealt. 0 = blocked</returns>
    public int DealDamageNPC(int dmg, Vector3 dir)
    {
        // if any buffs that reduce damage.
        float dmgMultiplier = 1f;                // if npc has any debuffs that makes it take more damage.

        if (Vector3.Angle(dir, transform.forward) <30)
            dmgMultiplier = 1.5f;


        //total damage to take
        float dmgToTake = dmg * dmgMultiplier;

        health -= (int)(dmgToTake);
        if (health <= 0)
        {
            KillNPC();
        }
        UpdateHealthUI();


        return (int)dmgToTake;    
    }


    void KillNPC()
    {
        health = 0;
        isDead = true;

        GetComponent<Animator>().SetBool("isDead", true);
        GetComponent<Animator>().SetLayerWeight(1,0);
        GetComponent<Animator>().SetLayerWeight(2,0);

        UIParent.gameObject.SetActive(false);

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    void UpdateHealthUI()
    {
        HP.fillAmount = (float)(health * 1f / maxHealth * 1f);
    }



    // getters for enum:
    public string Get_NPC_Type()
    {
        return npcType + "";
    }
    public string Get_NPC_CombatType()
    {
        return combatType + "";
    }
    public string Get_NPC_PlayerRelation()
    {
        return playerRelation + "";
    }

    //setters
    public void Set_NPC_PlayerRelation(string relation)
    {
        if (relation == "Hostile")
            playerRelation = stateTowardsPlayer.Hostile;
        else
            Debug.Log("Assigned incorrect player relation on NPC: " + transform.name);
        // other ones
    }

}
