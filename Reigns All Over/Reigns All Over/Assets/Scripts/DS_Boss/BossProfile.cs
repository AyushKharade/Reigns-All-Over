using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Create Boss", menuName = "Create Boss")]
public class BossProfile : ScriptableObject
{
    [Header("Boss Stats")]
    public string bName;
    public int health;
    public float moveSpeed;
    public float runSpeed;
    public float alignSpeed;

    public float fastAttackDmg;
    public float heavyAttackDmg;
    public float specialAttackDmg;

}
