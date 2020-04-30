using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores information about spells and casts them.
/// </summary>
public class Spell : MonoBehaviour
{


    [Header("Spell Details")]
    public string SpellName;
    [TextArea(3,3)]
    public string desc;
    public enum spellSchool { Destructive, Summoner, Healer };
    public spellSchool spellClass = new spellSchool();

    public int cost; // change this to charges.

    // Ref
    [HideInInspector] public GameObject Target; // for self cast

    void Start()
    {
        
    }

    void Update()
    {
        string type = GetSpellClass();

        if (type.CompareTo("Healer") == 0)
        {
            HealerSpellCast();
        }
    }



    void HealerSpellCast()
    {
        // player spell for now
        Target.GetComponent<PlayerAttributes>().HealPlayer(15);
        Destroy(this.gameObject);
    }

    public string GetSpellClass()
    {
        return spellClass + "";
    }
}
