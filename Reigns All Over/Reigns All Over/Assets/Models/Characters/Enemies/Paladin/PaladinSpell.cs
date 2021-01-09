using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaladinSpell : MonoBehaviour
{
    // Start is called before the first frame update

    [HideInInspector]public Transform target;
    [HideInInspector]public Transform camRef;
    [HideInInspector] public Transform Paladinref;

    public float moveSpeed=7.5f;
    public float moveSpeedIncrRate=3f;
    public float alignSpeed=3.5f;
    public float distanceToAllowAlign=5f;
    public bool shipSailed;

    public float detonationRadius=5f;

    public float targetY_Offset = 0.3f;

    public float damage;

    [Header("Audio")]
    public AudioClip castSound;
    public AudioClip hitSound;
    public AudioClip detonateSound;
    public float volume = 0.6f;
    AudioSource audioSource;


    void Start()
    {
        Destroy(transform.parent.gameObject, 10f);
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(castSound,volume);
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            float dist = (transform.position - target.position).sqrMagnitude;
            if (dist > distanceToAllowAlign * distanceToAllowAlign && !shipSailed)
            {
                Vector3 adjustedDir = (target.position - transform.position).normalized;
                adjustedDir.y += targetY_Offset;
                AlignOrientation(adjustedDir);
            }
            else
                shipSailed = true;


            transform.parent.Translate(transform.forward * moveSpeed * Time.deltaTime);
            moveSpeed += Time.deltaTime*moveSpeedIncrRate;
            RandomRotation();

            
        }
    }

   


    void RandomRotation()
    {
        float xSpin = Random.Range(0, 360);
        float ySpin = Random.Range(0, 360);
        float zSpin = Random.Range(0, 360);


        transform.GetChild(0).rotation = Quaternion.Euler(xSpin, ySpin,zSpin );
    }

    public void AlignOrientation(Vector3 dir)
    {

        Quaternion lookDirection;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, alignSpeed);
    }

    // happens if ball misses player and hit something else it will do a detonation, will damage if player is in range
    void Detonate(bool doDamage, Transform targetRef)
    {
        Collider[] arr = Physics.OverlapSphere(transform.position,detonationRadius);

        bool playerInRadius = false;
        if (!doDamage)    // means the spell didnt hit the player directly, need to see if player is in radius
        {
            foreach (Collider c in arr)
            {
                if (c.CompareTag("Player"))
                {
                    playerInRadius = true;
                    break;
                }
                playerInRadius = false;
            }
        }
        if (playerInRadius && doDamage)
        {
            Vector3 towardsDir = (targetRef.position - transform.position).normalized;
            bool killed=targetRef.GetComponent<PlayerAttributes>().DealDamage(damage,towardsDir );
            if (killed)
            {
                Paladinref.GetComponent<BossScript>().DefeatPlayer();
                return;
            }


            Vector3 dir = towardsDir;
            dir.y += 2f;

            if (!target.GetComponent<PlayerMovement>().isDodging)
                target.GetComponent<Rigidbody>().AddForce(dir * 25f, ForceMode.Impulse);

        }



        //shake camera
    }

    
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Spell collided with "+other.name);
        audioSource.PlayOneShot(hitSound,volume);
        bool detonate = true;
        if (other.CompareTag("Player"))
        {
            detonate = false;
            bool killed=other.GetComponent<PlayerAttributes>().DealDamage(damage, transform.forward);
            if (killed)
            {
                Paladinref.GetComponent<BossScript>().DefeatPlayer();
            }
            else
            {
                bool throwPlayer = false;
                if (Vector3.Angle(transform.forward, target.forward) > 50 && other.GetComponent<Combat>().isBlocking)
                    other.GetComponent<Combat>().StunPlayer(0f, transform.forward);
                else
                {
                    other.GetComponent<Combat>().StunPlayer(2f, transform.forward);
                    throwPlayer = true;
                }
                // joke
                Vector3 dir = transform.forward;
                dir.y += 2f;

                if (!other.GetComponent<PlayerMovement>().isDodging && throwPlayer)
                    other.GetComponent<Rigidbody>().AddForce(dir * 25f, ForceMode.Impulse);
            }
        }

        if (!other.CompareTag("Boss") && !other.CompareTag("Weapon"))
        {
            Detonate(detonate,other.transform);
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            Destroy(transform.parent.gameObject,2f);
        }
    }
    
    /*
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Spell collisioned with " + other.collider.name);
        bool detonate = true;
        if (other.collider.CompareTag("Player"))
        {
            detonate = false;
            other.collider.GetComponent<PlayerAttributes>().DealDamage(damage, transform.forward);

            bool throwPlayer = false;
            if (Vector3.Angle(transform.forward, target.forward) > 50 && other.collider.GetComponent<Combat>().isBlocking)
                other.collider.GetComponent<Combat>().StunPlayer(0f, transform.forward);
            else
            {
                other.collider.GetComponent<Combat>().StunPlayer(2f, transform.forward);
                throwPlayer = true;
            }
            // joke
            Vector3 dir = transform.forward;
            dir.y += 2f;

            if (!other.collider.GetComponent<PlayerMovement>().isDodging && throwPlayer)
                other.collider.GetComponent<Rigidbody>().AddForce(dir * 25f, ForceMode.Impulse);
        }

        if (!other.collider.CompareTag("Boss") && !other.collider.CompareTag("Weapon"))
        {
            Detonate(detonate, other.transform);
            Destroy(transform.parent.gameObject);
        }
    }
    */

    void ForcePushBack()
    {

    }
}
