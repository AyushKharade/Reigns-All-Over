using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to take care of player interaction, using raycasts in the middle of the screen, prompting buttons for interaction.
/// </summary>
public class Interaction : MonoBehaviour
{

    [Header("Raycast Target")]
    public GameObject Target;

    //References
    PlayerMovement MovementRef;

    void Start()
    {
        MovementRef = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!MovementRef.isDead)
        {
            RaycastInteractables();
        }
    }

    void RaycastInteractables()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out hit, Mathf.Infinity))
        {
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
                Target = hit.collider.gameObject;
            else if(hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC"))
                Target = hit.collider.gameObject;

        }
        else
            Target = null;
    }
}
