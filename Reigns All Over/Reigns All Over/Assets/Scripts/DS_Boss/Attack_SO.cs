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
    public string animatorStateName;
    public bool isSecondPhaseAttack;

    [Header("Stun ")]
    public bool canStun;
    public float stunValue;
    public float blockStunValue;               // if player was blocking

    [Header("Cooldowns")]
    public bool hasCooldown;
    public float cooldown;
    public bool overrideBossCooldown;
    public float overrideBossCooldownTime;
    public float catchUpTime;               // leave at zero if no catch up;


    [Header("Hit angle override")]
    public bool overrideHitAngle;
    public float overrideHitAngleValue;

    [Header("Range")]
    public float rangeNeeded; // to initiate attack
    public float rangeToDamagetarget;       // attacks can be done from further away but damage only happens on this range


    [Header("Chain attacks")]
    public Attack_SO chainAttack;
    public bool canChain;
    public float chainChance;

    [Header("References")]
    public GameObject VFX_Prefab;

}
