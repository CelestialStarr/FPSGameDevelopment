using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;

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
        if (isDead) return; // 防止重复死亡

        isDead = true;
        currentHealth = 0;
        UpdateHealthUI();

        // 禁用玩家控制器
        PlayerController.instance.enabled = false;

        // 禁用角色控制器
        CharacterController charController = PlayerController.instance.GetComponent<CharacterController>();
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

    // 死亡倒计时协程 - 修复了方法名
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
        // 重置血量
        currentHealth = maxHealth;
        isDead = false;

        // 重新启用玩家渲染
        EnablePlayerRendering();

        // 重新启用角色控制器
        CharacterController charController = PlayerController.instance.GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = true;

        // 重新启用玩家控制器（会自动处理武器显示）
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
}