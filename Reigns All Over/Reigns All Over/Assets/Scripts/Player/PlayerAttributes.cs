using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// saves data such as health, stamina, gold, inventory, skills, experience etc.
/// </summary>
public class PlayerAttributes : MonoBehaviour
{
    [Header("Player Information")]
    public float health=100;
    public float stamina=100;
    public float staminaRegenRate;
    public float staminaRegenDelay;
    public bool onStaminaRegenDelay;
    float staminaTimer;

    [Header("Character")]
    public int level;
    public int currentXP;

    //references
    PlayerMovement MovementRef;

    private void Start()
    {
        MovementRef = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if(!MovementRef.isDead)
            RegenStamina();
    }

    void RegenStamina()
    {
        if (stamina < 100)
        {
            if (onStaminaRegenDelay)
            {
                staminaTimer += Time.deltaTime;
                if (staminaTimer > staminaRegenDelay)
                {
                    onStaminaRegenDelay = false;
                    staminaTimer = 0;
                }
            }
            else
            {
                stamina += staminaRegenRate * Time.deltaTime;
            }
        }
    }
}
