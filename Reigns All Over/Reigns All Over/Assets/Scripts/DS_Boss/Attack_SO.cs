using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Scritable objects for Boss Attacks, each with its damage, cooldown, special effects / VFX, etc.
[CreateAssetMenu(fileName = "Create Boss Attack", menuName = "Create Boss Attack")]
public class Attack_SO : ScriptableObject
{

    [Header("Attack details")]
    public string attackname;
    public int damage;
    public bool canStun;
    public string animatorStateName;
    public bool hasCooldown;
    public float cooldown;


    public float rangeNeeded; // to initiate attack
    public float rangeToDamagetarget;       // attacks can be done from further away but damage only happens on this range

    public float catchUpTime;               // leave at zero if no catch up;

    [Header("References")]
    public GameObject VFX_Prefab;

}
