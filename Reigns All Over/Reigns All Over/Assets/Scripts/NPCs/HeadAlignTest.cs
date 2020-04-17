using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAlignTest : MonoBehaviour
{
    // just for testing NPC's look at player while in convo

    public Transform HeadRef;
    public Transform Target;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AlignHead();
    }

    void AlignHead()
    {
        HeadRef.LookAt(Target);
    }
}
