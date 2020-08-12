using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam2 : MonoBehaviour
{
    [Header("Varaibles")]
    public float camMoveSpeed = 120f;
    public Transform cameraFollowTarget;
    public Transform cameraAimFollowTarget;
    public GameObject cameraObj;
    public GameObject playerObj;
    Vector3 FollowPOS;

    public float clampAngle = 75f;
    public float sensitivity = 150f;
    public bool invert=true;

    float mouseX;
    float mouseY;
    public float smoothX;
    public float smoothY;

    float rotY = 0f;
    float rotX = 0f;

    //references
    Combat combatRef;


    private void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        combatRef = playerObj.GetComponent<Combat>();
    }


    private void Update()
    {
        // get input;
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y")*-1;

        rotY += mouseX * sensitivity * Time.deltaTime;
        rotX += mouseY * sensitivity * Time.deltaTime;
        

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX,rotY,0f);
        transform.rotation = localRotation;


        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale == 0f)
                Time.timeScale = 1f;
            else
                Time.timeScale = 0f;
        }
    }


    private void LateUpdate()
    {
        CamPosUpdate();
    }

    void CamPosUpdate()
    {
        Transform target;
        // follow player
        if (combatRef.archerBowDraw)
            target = cameraAimFollowTarget;
        else
            target = cameraFollowTarget;

        // move towards
        float step = camMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        
    }
}
