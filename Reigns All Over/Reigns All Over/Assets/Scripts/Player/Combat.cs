using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class deals with combat system and animation
/// </summary>
public class Combat : MonoBehaviour
{
    public bool inCombat;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inCombat)
            {
                inCombat = false;
                animator.SetBool("inCombat",false);
            }
            else
            {
                inCombat = true;
                animator.SetBool("inCombat", true);
            }
        }
    }
}
