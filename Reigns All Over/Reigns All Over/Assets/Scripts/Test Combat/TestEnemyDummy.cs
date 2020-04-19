using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// for testing damaging and combo
public class TestEnemyDummy : MonoBehaviour
{
    [Header("Variables")]
    public int health = 50;
    int maxHealth;

    public Image HP;
    public Transform UIParent;

    private void Start()
    {
        maxHealth = health;

    }

    private void Update()
    {
        // make UI be billboard
        UIParent.rotation = Camera.main.transform.rotation;
    }
    void UpdateHealthUI()
    {
        HP.fillAmount = (float)(health*1f / maxHealth*1f);
    }
   

    public void DealDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
            Destroy(this.gameObject);

        UpdateHealthUI();
    }
}
