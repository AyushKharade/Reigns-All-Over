using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A seperate class to handle the vision cone of each NPC, what they see, i.e. player or other NPCs, whom they attack depending upon
/// distance and angle.
/// 
/// If stealth is implemented, detection can be seperately be implemented in this script.
/// </summary>
public class Vision : MonoBehaviour
{
    [Header("Variables")]
    public float visionMaxRange=40f;                                 // maximmum vision cone
    [Range(0f, 1f)] public float detectionRate;                 // how fast will they detect their target. 0 = never, 1 = instantly once in vision.

    //references
    NPC_CombatBehavior npcCombatRef;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SphereCollider>().radius = visionMaxRange;

        npcCombatRef = transform.parent.GetComponent<NPC_CombatBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // various cases.
    }
}
