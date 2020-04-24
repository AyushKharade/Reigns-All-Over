using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Player UI References")]
    public Image HPBar;
    public Image StaminaBar;

    [Header("Mana References")]
    public Transform manaSlotParent;
    List<Image> manaSlotList = new List<Image>();

    //references:
    PlayerAttributes PAttributesRef;

    // Start is called before the first frame update
    void Start()
    {
        PAttributesRef = transform.GetChild(1).GetComponent<PlayerAttributes>();

        // store all images in respective mana image slots.
        foreach (Transform T in manaSlotParent)
        {
            manaSlotList.Add(T.GetChild(1).GetComponent<Image>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        HealthUI_Update();
        StaminaUI_Update();
        ManaUI_Update();
    }

    void HealthUI_Update()
    {
        HPBar.fillAmount = (PAttributesRef.health / PAttributesRef.maxHealth);

    }

    void StaminaUI_Update()
    {
        StaminaBar.fillAmount = (PAttributesRef.stamina / PAttributesRef.maxStamina);

    }

    void ManaUI_Update()
    {
        // read current index from attributes
        int slotIndex=0;
        foreach (Image i in manaSlotList)
        {
            i.fillAmount = PAttributesRef.manaSlots[slotIndex]/10f;
            slotIndex++;
        }
    }

    /// <summary>
    /// flash stamina bar to indicate no more stamina for action.
    /// </summary>
    public void WarnNoStaminaUI()
    {
        StaminaBar.transform.parent.GetChild(0).GetComponent<Animation>().Play();
    }
}
