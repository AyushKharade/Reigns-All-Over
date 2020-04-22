using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    public Image HPBar;
    public Image StaminaBar;

    //references:
    PlayerAttributes PAttributesRef;

    // Start is called before the first frame update
    void Start()
    {
        PAttributesRef = transform.GetChild(1).GetComponent<PlayerAttributes>();
    }

    // Update is called once per frame
    void Update()
    {
        HPBar.fillAmount = (PAttributesRef.health/PAttributesRef.maxHealth);
        StaminaBar.fillAmount = (PAttributesRef.stamina / PAttributesRef.maxStamina);
    }
}
