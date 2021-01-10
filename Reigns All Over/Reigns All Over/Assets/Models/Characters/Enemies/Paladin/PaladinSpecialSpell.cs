using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaladinSpecialSpell : MonoBehaviour
{
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform camRef;
    [HideInInspector] public Transform Paladinref;

    Vector3 direction;
    bool aligned;

    public float moveSpeed = 7.5f;
    public float moveSpeedIncrRate = 3f;

    public float damage;
    [HideInInspector] public bool dirSet;
    [Header("Audio")]
    public AudioClip castSound;
    public AudioClip hitSound;
    public AudioClip detonateSound;
    public float volume = 0.6f;
    AudioSource audioSource;


    void Start()
    {
        Destroy(transform.parent.gameObject, 5f);
        //audioSource = GetComponent<AudioSource>();
        //audioSource.PlayOneShot(castSound, volume);

    }

    // Update is called once per frame
    void Update()
    {
        if (dirSet)
        {

            if (Vector3.Angle(transform.forward, direction) > 5 && !aligned)
            {
                AlignOrientation(direction);
            }
            else aligned = true;

            if (aligned)
            {
                transform.parent.Translate(transform.forward * moveSpeed * Time.deltaTime);
                moveSpeed += moveSpeedIncrRate * Time.deltaTime;
            }
        }
    }


    public void SetDirection(Vector3 dir)
    {
        direction = dir;
        dirSet = true;
    }

    public void AlignOrientation(Vector3 dir)
    {
        Quaternion lookDirection;

        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool killed=other.GetComponent<PlayerAttributes>().DealDamage(damage,transform.forward);
            if (!killed)
            {
                other.GetComponent<Combat>().StunPlayer(2f, transform.forward);
                Vector3 dir = transform.forward;
                dir.y += 2f;
                other.GetComponent<Rigidbody>().AddForce(dir * 25f, ForceMode.Impulse);
            }
            else
                Paladinref.GetComponent<BossScript>().DefeatPlayer();

        }
    }
}
