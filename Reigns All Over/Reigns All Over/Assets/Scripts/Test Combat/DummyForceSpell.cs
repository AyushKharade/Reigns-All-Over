using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyForceSpell : MonoBehaviour
{
    /// <summary>
    /// No. of charges required to cast spell.
    /// </summary>
    public int cost;
    public float force=10f;
    public float mSpeed=3f;
    public Transform Target;
    public int damage;

    public GameObject DMG_popupPrefab;

    void Start()
    {
        AcquireTarget();
        Destroy(this.gameObject, 5f);
    }

    void Update()
    {
        if (Target != null)
        {
            // chase target.
            Vector3 targettingPostion = Target.position;
            targettingPostion.y = transform.position.y;

            transform.Translate((targettingPostion - transform.position).normalized * mSpeed*Time.deltaTime);
        }
        else
        {
            transform.Translate(transform.forward * mSpeed * Time.deltaTime);
        }
    }

    void AcquireTarget()
    {
        Collider[] arr= Physics.OverlapSphere(transform.position,15f);
        foreach (Collider c in arr)
        {
            if (c.tag == "Enemy")
            {
                if (!c.GetComponent<TestEnemyDummy>().isDead)
                {
                    Target = c.transform;
                    break;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Enemy")
        {
            //hurt
            collision.collider.GetComponent<TestEnemyDummy>().DealDamage(damage);
            GameObject dm=Instantiate(DMG_popupPrefab, collision.contacts[0].point, Quaternion.identity);
            dm.GetComponent<TextMesh>().text = damage + "";
            // magic is blue
            dm.GetComponent<TextMesh>().color = Color.blue;
        }

        Destroy(this.gameObject);
    }
    
}
