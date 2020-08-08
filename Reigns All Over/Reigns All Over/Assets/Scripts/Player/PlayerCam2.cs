using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam2 : MonoBehaviour
{
    [Header("Varaibles")]
    public float camMoveSpeed = 120f;
    public GameObject cameraFollowObj;
    public GameObject cameraObj;
    public GameObject playerObj;
    Vector3 FollowPOS;

    public float clampAngle = 75f;
    public float sensitivity = 150f;

    [Header("Inputs")]
    public float mouseX;
    public float mouseY;
    public float smoothX;
    public float smoothY;

    public float rotY = 0f;
    public float rotX = 0f;


    private void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        // get input;
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        rotY += mouseX * sensitivity * Time.deltaTime;
        rotX += mouseY * sensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX,rotY,0f);
        transform.rotation = localRotation;
    }


    private void LateUpdate()
    {
        CamPosUpdate();
    }

    void CamPosUpdate()
    {
        // follow player
        Transform target = cameraFollowObj.transform;

        // move towards
        float step = camMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        
    }
}
