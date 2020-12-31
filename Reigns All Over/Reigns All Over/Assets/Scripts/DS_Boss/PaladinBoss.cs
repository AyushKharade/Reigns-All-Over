using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Boss scripts are unique behaviors, they use any helper methods from the common boss script such as is target visible, move, align, navmesh
/// , UI whenever required. Their behavoir is directed from this script.
/// 
/// Paladin Boss:
/// A sword and shield warrior who has a variety of attacks.
/// Imp things to note:
/// - runs towards you if you are too far
/// - walks in 4 directions to position
/// - can rotate 180 if you stay behind him
/// - certain attacks can orient with you.
/// - try to make a finisher if the attacking blow is a killer.
/// when on 25% HP, knocks the player back and triggers Adrenaline mode, has special VFX, and all animation speeds are faster recovers health back to 50%
/// </summary>
public class PaladinBoss : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
