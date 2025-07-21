using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;
    private static int savedMaxHealth = 100; // ��̬��������Ѫ������
    private static int savedCurrentHealth = 100; // ��̬�������浱ǰѪ��

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
        // ÿ���������´�����������Ѫ������
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;

        // ���糡������GameObject������������
        // DontDestroyOnLoad(gameObject); // ע�͵�
    }

    void Start()
    {
        // �Ӿ�̬�����ָ�Ѫ��״̬
        maxHealth = savedMaxHealth;
        currentHealth = savedCurrentHealth;

        // �ӳٸ���UI��ȷ��UIController�Ѿ�׼����
        Invoke("UpdateHealthUI", 0.3f);
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
        // ���浱ǰѪ������̬����
        savedMaxHealth = maxHealth;
        savedCurrentHealth = currentHealth;

        if (UIController.Instance != null)
        {
            if (UIController.Instance.healthSlider != null)
            {
                UIController.Instance.healthSlider.maxValue = maxHealth;
                UIController.Instance.healthSlider.value = currentHealth;
            }

            if (UIController.Instance.healthText != null)
            {
                UIController.Instance.healthText.text = "Health: " + currentHealth + "/" + maxHealth;
            }
        }
        else
        {
            Debug.LogWarning("UIController.Instance is null when trying to update health UI");
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
        if (PlayerController.instance != null)
            PlayerController.instance.enabled = false;

        // ���ý�ɫ������
        CharacterController charController = null;
        if (PlayerController.instance != null)
            charController = PlayerController.instance.GetComponent<CharacterController>();
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

    // ��������ʱЭ��
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
        if (PlayerController.instance == null) return;

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
        if (PlayerController.instance == null) return;

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
        // ����Ѫ��Ϊ��Ѫ
        currentHealth = maxHealth;
        isDead = false;

        // �������������Ⱦ
        EnablePlayerRendering();

        // �������ý�ɫ������
        CharacterController charController = null;
        if (PlayerController.instance != null)
            charController = PlayerController.instance.GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = true;

        // ����������ҿ����������Զ�����������ʾ��
        if (PlayerController.instance != null)
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

    // ����Ѫ��Ϊ��Ѫ�����ڹؿ���ʼʱ�������Ҫ�Ļ���
    public void ResetToFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthUI();
    }

    // ����Ѫ�������ֵ�ǰѪ��״̬�����ڹؿ���̳У�
    public void SetHealth(int newCurrentHealth, int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(newCurrentHealth, 0, maxHealth);
        savedMaxHealth = maxHealth;
        savedCurrentHealth = currentHealth;
        UpdateHealthUI();
    }
}
