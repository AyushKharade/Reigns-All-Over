using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    // Follow Player on X-Z
    public Transform Target;
    public Transform Player;

    public float rotationSpeed = 1;

    [Header("Adjustable Camera")]
    public float HeightOffset;
    public float XOffset;

    float mouseX;
    float mouseY;
    
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        CamControl();    
    }


    void CamControl()
    {
        mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;     // non inverted controls thats why -=

        mouseY = Mathf.Clamp(mouseY, -35f, 60f);

        Vector3 LookAtTarget = Target.position;
        LookAtTarget.y += HeightOffset;
        LookAtTarget.x += XOffset;
        //transform.LookAt(Target);
        transform.LookAt(LookAtTarget);

        Target.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        //Player.rotation = Quaternion.Euler(0, mouseX, 0);
    }
    
}
