using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;

    public int maxHealth, currentHealth;

    public float invLength = 1f;
    public float invCounter;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;

        UIController.Instance.healthSlider.maxValue = maxHealth;
        UIController.Instance.healthSlider.value = currentHealth;
        UIController.Instance.healthText.text = "Health: " + currentHealth + "/" + maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(invCounter > 0)
        {
            invCounter -= Time.deltaTime;
        }
       
    }

    public void DamagePlayer(int damage)
    {
        if(invCounter <=0)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                transform.parent.gameObject.SetActive(false); //hide the player
            }

            invCounter = invLength;

            UIController.Instance.healthSlider.value = currentHealth;
            UIController.Instance.healthText.text = "Health: " + currentHealth + "/" + maxHealth;

        }

       
    }

  

    public void HealPlayer(int healAmount)
    {
        currentHealth += healAmount;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UIController.Instance.healthSlider.value = currentHealth;
        UIController.Instance.healthText.text = "Health: " + currentHealth + "/" + maxHealth;

    }
}
