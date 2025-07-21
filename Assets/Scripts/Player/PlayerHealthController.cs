using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;
    private static int savedMaxHealth = 100; // 静态变量保存血量上限
    private static int savedCurrentHealth = 100; // 静态变量保存当前血量

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public float invLength = 1f;

    [Header("Death Settings")]
    public float respawnDelay = 3f; // 重生延迟时间

    private float invCounter;
    private bool isDead = false;

    private void Awake()
    {
        // 每个场景重新创建，但保持血量数据
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;

        // 不跨场景保持GameObject，但保持数据
        // DontDestroyOnLoad(gameObject); // 注释掉
    }

    void Start()
    {
        // 从静态变量恢复血量状态
        maxHealth = savedMaxHealth;
        currentHealth = savedCurrentHealth;

        // 延迟更新UI，确保UIController已经准备好
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
        // 保存当前血量到静态变量
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
        if (isDead) return; // 防止重复死亡

        isDead = true;
        currentHealth = 0;
        UpdateHealthUI();

        // 禁用玩家控制器
        if (PlayerController.instance != null)
            PlayerController.instance.enabled = false;

        // 禁用角色控制器
        CharacterController charController = null;
        if (PlayerController.instance != null)
            charController = PlayerController.instance.GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = false;

        // 禁用玩家渲染
        DisablePlayerRendering();

        // 显示死亡UI并开始倒计时
        if (UIController.Instance != null)
        {
            UIController.Instance.ShowDeathUI();
            StartCoroutine(DeathCountdownSequence());
        }

        Debug.Log("Player died!");
    }

    // 死亡倒计时协程
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

        // 倒计时结束，开始重生
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateDeathCountdown(0);
        }

        // 调用重生
        if (CheckpointManager.instance != null)
        {
            CheckpointManager.instance.RespawnPlayer();
        }
    }

    private void DisablePlayerRendering()
    {
        if (PlayerController.instance == null) return;

        // 禁用玩家身上所有的Renderer组件，但不影响摄像机
        Renderer[] renderers = PlayerController.instance.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // 禁用所有子物体（武器等）
        foreach (Transform child in PlayerController.instance.transform)
        {
            // 跳过摄像机
            if (!child.name.Contains("Camera") && !child.name.Contains("camera"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    // 重生时重新启用渲染
    private void EnablePlayerRendering()
    {
        if (PlayerController.instance == null) return;

        // 重新启用玩家身上所有的Renderer组件
        Renderer[] renderers = PlayerController.instance.GetComponentsInChildren<Renderer>(true); // 包括inactive的
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        // 重新启用所有子物体
        foreach (Transform child in PlayerController.instance.transform)
        {
            if (!child.name.Contains("Camera") && !child.name.Contains("camera"))
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    // 重生方法（由CheckpointManager调用）
    public void RespawnPlayer()
    {
        // 重置血量为满血
        currentHealth = maxHealth;
        isDead = false;

        // 重新启用玩家渲染
        EnablePlayerRendering();

        // 重新启用角色控制器
        CharacterController charController = null;
        if (PlayerController.instance != null)
            charController = PlayerController.instance.GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = true;

        // 重新启用玩家控制器（会自动处理武器显示）
        if (PlayerController.instance != null)
            PlayerController.instance.enabled = true;

        // 更新UI
        UpdateHealthUI();

        // 隐藏死亡UI
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

    // 获取当前血量（给其他脚本使用）
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // 检查是否死亡
    public bool IsDead()
    {
        return isDead;
    }

    // 重置血量为满血（用于关卡开始时，如果需要的话）
    public void ResetToFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthUI();
    }

    // 设置血量（保持当前血量状态，用于关卡间继承）
    public void SetHealth(int newCurrentHealth, int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(newCurrentHealth, 0, maxHealth);
        savedMaxHealth = maxHealth;
        savedCurrentHealth = currentHealth;
        UpdateHealthUI();
    }
}
