using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Variables")]
    public float runSpeed;
    public float sprintSpeed;
    public float alignSpeed;

    [Header("Character States")]
    public bool isGrounded;
    public bool isRunning;
    public bool isDodging;

    // Private stuff
    Vector3 FaceDirection;                             // Input Direction, match forward vector with this

    // camera reference:
    [Header("Object References")]
    public Transform CamRef;
    public Transform PlayerMesh;
    Transform TargetRef;

    //animator ref
    Animator animator;

    void Start()
    {
        TargetRef = CamRef.parent;
        //animator = PlayerMesh.GetComponent<Animator>();
    }

    void Update()
    {
        transform.rotation = Quaternion.identity;                 // Make sure that PlayerHolder Never rotates.
        PMovement();
    }

    /// <summary>
    /// Upon directonal keypress, move in direction, as well as align. Uses 360 movement when not in combat.
    /// </summary>
    void PMovement()
    {
        Vector3 PlayerDirection = Vector3.zero;

        // Front & Back.
        if(Input.GetKey(KeyCode.W))
            PlayerDirection = TargetRef.forward;
        else if(Input.GetKey(KeyCode.S))
            PlayerDirection = TargetRef.forward * -1;
        // Sideways
        if (Input.GetKey(KeyCode.A))
            PlayerDirection = TargetRef.right * -1;
        else if(Input.GetKey(KeyCode.D))
            PlayerDirection = TargetRef.right;


        PlayerDirection = PlayerDirection.normalized;
        PlayerDirection.y = 0;

        transform.Translate(PlayerDirection * runSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            // animation stuff
            AlignOrientation(PlayerDirection);
            isRunning = true;
            Debug.DrawRay(transform.position, PlayerDirection, Color.green);
        }
        else
        {
            isRunning = false;
        }


        // testing alignment
        Vector3 faceDir = TargetRef.forward;
        faceDir.y = 0;
        //AlignOrientation(TargetRef.forward);
    }
    
 
    void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        PlayerMesh.transform.localRotation = Quaternion.RotateTowards(PlayerMesh.localRotation, lookDirection, alignSpeed);
    }
}
