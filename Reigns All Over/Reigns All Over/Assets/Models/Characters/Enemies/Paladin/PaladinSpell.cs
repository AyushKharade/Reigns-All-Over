using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaladinSpell : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform target;
    public float moveSpeed=7.5f;
    public float moveSpeedIncrRate=3f;
    public float alignSpeed=3.5f;
    public float distanceToAllowAlign=5f;
    public bool shipSailed;

    public float targetY_Offset = 0.3f;

    public float damage;
    public bool canStun;
    public float stunValue;
    public float stunValueBlocking;

    void Start()
    {
        Destroy(this.gameObject, 10f);   
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerAttributes>().DealDamage(damage,transform.forward);

            bool throwPlayer=false;
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

            if(!other.GetComponent<PlayerMovement>().isDodging && throwPlayer)
                other.GetComponent<Rigidbody>().AddForce(dir * 25f, ForceMode.Impulse);
        }

        if(!other.CompareTag("Boss") && !other.CompareTag("Weapon"))
            Destroy(this.gameObject);
    }
}
