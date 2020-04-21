using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DMGPopUp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.up * 0.6f * Time.deltaTime);
        transform.rotation = Camera.main.transform.rotation;
    }
}
