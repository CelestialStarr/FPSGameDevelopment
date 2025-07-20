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

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        if (invCounter > 0)
        {
            invCounter -= Time.deltaTime;
        }
    }

    public void UpdateHealthUI()
    {
        UIController.Instance.healthSlider.maxValue = maxHealth;
        UIController.Instance.healthSlider.value = currentHealth;
        UIController.Instance.healthText.text = "Health: " + currentHealth + "/" + maxHealth;
    }

    public void DamagePlayer(int damage)
    {
        if (invCounter <= 0)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                KillPlayer();
            }
            else
            {
                invCounter = invLength;
                UpdateHealthUI();
            }
        }
    }

    public void KillPlayer()
    {
        currentHealth = 0;
        UpdateHealthUI();
        PlayerController.instance.gameObject.SetActive(false);
        //CheckpointManager.instance.RespawnPlayer();
    }


    public void HealPlayer(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateHealthUI();
    }
}
