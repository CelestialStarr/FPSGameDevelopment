using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public float invLength = 1f;

    [Header("Death Settings")]
    public float respawnDelay = 3f; // �����ӳ�ʱ��

    private float invCounter;
    private bool isDead = false;

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
        if (UIController.Instance != null)
        {
            UIController.Instance.healthSlider.maxValue = maxHealth;
            UIController.Instance.healthSlider.value = currentHealth;
            UIController.Instance.healthText.text = "Health: " + currentHealth + "/" + maxHealth;
        }
    }

    public void DamagePlayer(int damage)
    {
        if (invCounter <= 0 && !isDead)
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
        if (isDead) return; // ��ֹ�ظ�����

        isDead = true;
        currentHealth = 0;
        UpdateHealthUI();

        // ������ҿ�����
        PlayerController.instance.enabled = false;

        // ���ý�ɫ������
        CharacterController charController = PlayerController.instance.GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = false;

        // ���������Ⱦ
        DisablePlayerRendering();

        // ��ʾ����UI����ʼ����ʱ
        if (UIController.Instance != null)
        {
            UIController.Instance.ShowDeathUI();
            StartCoroutine(DeathCountdownSequence());
        }

        Debug.Log("Player died!");
    }

    // ��������ʱЭ�� - �޸��˷�����
    System.Collections.IEnumerator DeathCountdownSequence()
    {
        float countdown = respawnDelay;

        while (countdown > 0)
        {
            if (UIController.Instance != null)
            {
                UIController.Instance.UpdateDeathCountdown(countdown);
            }

            countdown -= Time.deltaTime;
            yield return null;
        }

        // ����ʱ��������ʼ����
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateDeathCountdown(0);
        }

        // ��������
        if (CheckpointManager.instance != null)
        {
            CheckpointManager.instance.RespawnPlayer();
        }
    }

    private void DisablePlayerRendering()
    {
        // ��������������е�Renderer���������Ӱ�������
        Renderer[] renderers = PlayerController.instance.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // �������������壨�����ȣ�
        foreach (Transform child in PlayerController.instance.transform)
        {
            // ���������
            if (!child.name.Contains("Camera") && !child.name.Contains("camera"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    // ����ʱ����������Ⱦ
    private void EnablePlayerRendering()
    {
        // ������������������е�Renderer���
        Renderer[] renderers = PlayerController.instance.GetComponentsInChildren<Renderer>(true); // ����inactive��
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        // ������������������
        foreach (Transform child in PlayerController.instance.transform)
        {
            if (!child.name.Contains("Camera") && !child.name.Contains("camera"))
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    // ������������CheckpointManager���ã�
    public void RespawnPlayer()
    {
        // ����Ѫ��
        currentHealth = maxHealth;
        isDead = false;

        // �������������Ⱦ
        EnablePlayerRendering();

        // �������ý�ɫ������
        CharacterController charController = PlayerController.instance.GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = true;

        // ����������ҿ����������Զ�����������ʾ��
        PlayerController.instance.enabled = true;

        // ����UI
        UpdateHealthUI();

        // ��������UI
        if (UIController.Instance != null)
        {
            UIController.Instance.HideDeathUI();
        }

        Debug.Log("Player respawned!");
    }

    public void HealPlayer(int healAmount)
    {
        if (!isDead)
        {
            currentHealth += healAmount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            UpdateHealthUI();
        }
    }

    // ��ȡ��ǰѪ�����������ű�ʹ�ã�
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // ����Ƿ�����
    public bool IsDead()
    {
        return isDead;
    }
}