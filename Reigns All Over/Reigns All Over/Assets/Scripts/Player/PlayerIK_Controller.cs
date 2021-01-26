using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIK_Controller : MonoBehaviour
{

    public bool useIK;
    public float lookWeight;
    [HideInInspector] public Vector3 lookTarget;
    [HideInInspector] public Vector3 spineLookAtForward; 
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    private void OnAnimatorIK()
    {
        if (animator)
        {
            if (useIK)
            {
                //animator.SetLookAtWeight(lookWeight,0.1f,lookWeight);
                animator.SetLookAtWeight(lookWeight);
                animator.SetLookAtPosition(lookTarget);

                Transform spineBone = animator.GetBoneTransform(HumanBodyBones.Spine);

                //Vector3 lookDirection = (lookTarget - spineBone.position).normalized;

                Quaternion lookRot = Quaternion.LookRotation(spineLookAtForward, Vector3.up);
                Vector3 lookRotvec3 = lookRot.eulerAngles;
                lookRotvec3.y = 0f;
                lookRotvec3.z = 0f;

                //animator.SetBoneLocalRotation(HumanBodyBones.UpperChest, animator.GetBoneTransform(HumanBodyBones.Head).rotation );
                //animator.SetBoneLocalRotation(HumanBodyBones.UpperChest, Quaternion.Euler(lookRotvec3) );
            }
            else
            {
                animator.SetLookAtWeight(0f);
            }
        }
    }
}
