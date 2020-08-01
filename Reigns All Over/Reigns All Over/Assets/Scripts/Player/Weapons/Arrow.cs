using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Weapon Details")]
    public string Name;
    [TextArea(3, 3)]
    public string desc;

    public int baseDamage;

    [Range(0, 100)] public float criticalChance;

    [Header("References")]
    //Combat CombatRef;
    //public GameObject PlayerRef;
    public GameObject DamagePopUpPrefab;
    public GameObject BloodParticle;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "NPC" && !other.CompareTag("Player"))
        {
            Debug.Log("Hit : " + LayerMask.LayerToName(other.gameObject.layer)+", Name: "+other.name);
            // various cases such as creature or hostile npc, etc. Do not hurt civilians (in future dont let the player swing)
            NPC_Attributes nAttributes = other.GetComponent<NPC_Attributes>();

            bool shouldAttack = false;
            if (nAttributes.Get_NPC_Type() == "Creature" || nAttributes.Get_NPC_Type() == "Bandit")
                shouldAttack = true;
            else if (nAttributes.Get_NPC_Type() == "Guard")
            {
                if (nAttributes.Get_NPC_PlayerRelation() != "Hostile")
                    nAttributes.Set_NPC_PlayerRelation("Hostile");

                shouldAttack = true;
            }


            // attack the target.
            if (!nAttributes.isDead && shouldAttack)
            {
                float dealtDMG = nAttributes.DealDamageNPC(baseDamage, transform.forward);

                //instantiate popup.
                GameObject GB = Instantiate(DamagePopUpPrefab, other.transform.position, Quaternion.identity);
                GB.transform.Translate(Vector3.up * 1.75f);
                GB.GetComponent<TextMesh>().text = baseDamage + "";
                //if (doingCritDMG)
                //    GB.GetComponent<TextMesh>().color = Color.red;

                if (BloodParticle != null)
                {
                    GB = Instantiate(BloodParticle, other.transform.position, Quaternion.identity);
                    GB.transform.Translate(Vector3.up * 1.3f);
                    Destroy(GB, 1f);
                }

                //PlayStabSound();
            }
        }


        //Destroy(this.gameObject);
    }
}
