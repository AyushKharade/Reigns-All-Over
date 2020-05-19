using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// stores data like value, damage, upgrades, runes, repair, critical chance, type (one handed / two handed), used by player and enemies.
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Weapon Details")]
    public string Name;
    [TextArea(3,3)]
    public string desc;
    public enum weaponClass{OneHanded, TwoHanded};
    public weaponClass WeaponType=new weaponClass();

    public enum WeaponUpgrade { Regular, Enhanced, Pristine ,Exquisite,Legendary };
    /// <summary>
    /// Effects how quickly it deteriotes
    /// </summary>
    public WeaponUpgrade upgrade=new WeaponUpgrade();

    public float baseDamage;
    
    [Range(0,100)]public float criticalChance;
    /// <summary>
    /// Weapons in bad condition will deal low damage (condition <65 (10% penalty), condition < 50 (25% penalty), condition < 30 (45% penalth)
    /// </summary>
    [Range(0,100)] public float condition;
    [Range(0, 1)] public float deteriorationRate;

    //private
    bool usedByPlayer;       // if yes, weapon will deteriorate and deal combo damage
    bool doingCritDMG;
    /// <summary>
    /// turned on from Combat, turned off on every collision so same weapon doesnt do additional damage.
    /// Can be upgraded as a skill, where it can do damage to multiple enemies but not the same enemy twice.
    /// </summary>
    [HideInInspector]public bool doDMG; 

    // references
    [Header("References")]
    Combat CombatRef;
    public GameObject PlayerRef;
    public GameObject DamagePopUpPrefab;
    public GameObject BloodParticle;

    [Header("Weapon Sounds")]
    AudioSource AudioSource;
    public List<AudioClip> audioList = new List<AudioClip>();


    //################################################################ start

    void Start()
    {
        CombatRef = PlayerRef.GetComponent<Combat>();
        AudioSource = GetComponent<AudioSource>();
    }

    bool RollCritical()
    {
        int r = Random.Range(0, 101);
        if (r < criticalChance)
        {
            doingCritDMG = true;
            return true;
        }
        doingCritDMG = false;
        return false;
    }
    /// <summary>
    /// factors: condition, current combo, base damage & crit chance (only for player), combos may apply to enemies.
    /// </summary>
    /// <returns> How much damage will be output</returns>
    float DealHowMuchDMG()
    {
        float dmg = baseDamage;
        // do condition check and apply penalty

        // light heavy
        if (CombatRef.attackAnimValue == 1)
            dmg += baseDamage * 0.6f;

        // critical
        if (RollCritical())
            dmg += baseDamage * 0.3f;

        // combo
        int c = CombatRef.combo;
        if (c > 1)
        {
            dmg += baseDamage * ((c * 15f) / 100f);
        }

        WeaponDeteriorate();
        return dmg;
    }

    /// <summary>
    /// reduces weapon condition on every use.
    /// </summary>
    void WeaponDeteriorate()
    {
        float f = 0;
        if (upgrade + "" == "Enhanced")
            f = 0.1f;
        else if ((upgrade + "" == "Pristine"))
            f = 0.25f;
        else if ((upgrade + "" == "Exquisite"))
            f = 0.4f;
        else if ((upgrade + "" == "Legendary"))
            f = 0.55f;


            condition -= (deteriorationRate-deteriorationRate*f);
    }



    
    private void OnTriggerEnter(Collider other)
    {
        /*
        if (other.tag == "Enemy" && doDMG && CombatRef.attacking)
        {
            if (!other.GetComponent<TestEnemyDummy>().isDead)
            {
                // calculate damage
                int dmg = (int)(DealHowMuchDMG());
                other.GetComponent<TestEnemyDummy>().DealDamage(dmg);
                doDMG = false;

                // instantiate popup.
                GameObject GB = Instantiate(DamagePopUpPrefab, other.transform.position, Quaternion.identity);
                GB.transform.Translate(Vector3.up * 1.75f);
                GB.GetComponent<TextMesh>().text = dmg + "";
                if (doingCritDMG)
                    GB.GetComponent<TextMesh>().color = Color.red;

            }
        }
        //*/

        if (LayerMask.LayerToName(other.gameObject.layer) == "NPC")
        {
            // various cases such as creature or hostile npc, etc. Do not hurt civilians (in future dont let the player swing)
            NPC_Attributes nAttributes = other.GetComponent<NPC_Attributes>();

            bool shouldAttack;
            if (nAttributes.Get_NPC_Type() == "Creature" || nAttributes.Get_NPC_Type() == "Bandit")
                shouldAttack = true;

            else if (nAttributes.Get_NPC_PlayeRelation() == "Hostile")
                shouldAttack = true;

            // attack the target.
            if (!nAttributes.isDead && doDMG && CombatRef.attacking)
            {
                int dmg = (int)(DealHowMuchDMG());
                float dealtDMG=nAttributes.DealDamageNPC(dmg,PlayerRef.transform.forward);
                doDMG = false;

                //instantiate popup.
                if (dealtDMG > 0)
                {
                    GameObject GB = Instantiate(DamagePopUpPrefab, other.transform.position, Quaternion.identity);
                    GB.transform.Translate(Vector3.up * 1.75f);
                    GB.GetComponent<TextMesh>().text = dealtDMG + "";
                    if (doingCritDMG)
                        GB.GetComponent<TextMesh>().color = Color.red;

                    if (BloodParticle != null)
                    {
                        GB = Instantiate(BloodParticle, other.transform.position, Quaternion.identity);
                        GB.transform.Translate(Vector3.up * 1.3f);
                        Destroy(GB, 1f);
                    }

                    PlayStabSound();
                }
            }

        }
    }
    
    

    void PlayStabSound()
    {
        int r = Random.Range(0, audioList.Count);


        AudioSource.clip = audioList[r];
        AudioSource.Play();

    }

}
