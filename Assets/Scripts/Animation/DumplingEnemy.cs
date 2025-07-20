using UnityEngine;
using UnityEngine.UI;

public class DumplingEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;                // 玩家对象
    public GameObject doughBall;            // 敌人本体
    public CanvasGroup wrapOverlayUI;       // UI 显示用

    [Header("Movement Settings")]
    public float detectRange = 15f;         // 触发追击范围
    public float wrapRange = 2f;            // 包裹触发范围
    public float moveSpeed = 1.5f;

    [Header("Health Settings")]
    public int maxHealth = 50;              // 最大血量
    public GameObject healthBarPrefab;      // 血条预制体
    public Transform healthBarPosition;     // 血条位置（头顶）

    [Header("Death Settings")]
    public GameObject[] possibleDrops;      // 可能掉落的物品
    public float dropChance = 0.7f;         // 掉落概率
    public GameObject deathEffect;          // 死亡特效

    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;

    // Private variables
    private int currentHealth;
    private bool isChasing = false;
    private bool hasWrapped = false;
    private bool isDead = false;
    private AudioSource audioSource;
    private GameObject healthBarInstance;
    private Slider healthSlider;

    void Start()
    {
        // 初始化血量
        currentHealth = maxHealth;

        // 获取音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 自动找到玩家
        if (player == null && PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }

        // 设置碰撞检测
        SetupColliders();

        // 创建血条
        CreateHealthBar();
    }

    void SetupColliders()
    {
        // 确保主物体有一个合理大小的实体Collider用于子弹碰撞
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider == null)
        {
            // 添加一个合理大小的碰撞体
            SphereCollider bodyCollider = gameObject.AddComponent<SphereCollider>();
            bodyCollider.radius = 0.5f; // 只包围敌人本体
            bodyCollider.isTrigger = false; // 可以被子弹击中
        }
        else
        {
            // 如果已有Collider，确保它不是巨大的检测范围
            if (mainCollider is SphereCollider sphere && sphere.radius > 3f)
            {
                sphere.radius = 0.5f; // 重置为合理大小
            }
        }

        // 创建专门的检测范围（不影响子弹）
        GameObject detectionZone = new GameObject("DetectionZone");
        detectionZone.transform.SetParent(transform);
        detectionZone.transform.localPosition = Vector3.zero;
        detectionZone.layer = gameObject.layer;

        SphereCollider detector = detectionZone.AddComponent<SphereCollider>();
        detector.radius = detectRange;
        detector.isTrigger = true; // 设为Trigger，不阻挡子弹

        // 添加检测脚本
        EnemyDetectionZone detectionScript = detectionZone.AddComponent<EnemyDetectionZone>();
        detectionScript.parentEnemy = this;
    }

    void CreateHealthBar()
    {
        if (healthBarPrefab != null)
        {
            // 创建血条实例
            healthBarInstance = Instantiate(healthBarPrefab);
            healthBarInstance.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

            // 获取血条组件
            healthSlider = healthBarInstance.GetComponent<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
        }
        else
        {
            // 如果没有预制体，创建简单的血条
            CreateSimpleHealthBar();
        }
    }

    void CreateSimpleHealthBar()
    {
        // 创建简单的世界空间血条
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = Vector3.up * 2f; // 头顶上方

        Canvas canvas = healthBarObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform rectTransform = healthBarObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(2f, 0.3f);

        // 创建血条背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 创建血条
        GameObject healthBar = new GameObject("HealthBar");
        healthBar.transform.SetParent(healthBarObj.transform, false);
        healthSlider = healthBar.AddComponent<Slider>();
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        // 血条填充
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBar.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.red;
        healthSlider.fillRect = fill.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (isDead) return;

        if (player == null) return;

        // 更新血条位置（如果是世界空间血条）
        UpdateHealthBarPosition();

        // 计算XZ平面距离
        Vector2 enemyXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        float distance = Vector2.Distance(enemyXZ, playerXZ);

        // 开始追击
        if (isChasing && !hasWrapped)
        {
            if (distance > wrapRange)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0; // 保持在地面移动
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;

                // 面向玩家
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            }
            else
            {
                hasWrapped = true;
                ShowWrapOverlay();
            }
        }
    }

    void UpdateHealthBarPosition()
    {
        if (healthBarInstance != null && Camera.main != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBarInstance.transform.position = screenPos;
        }
    }

    // 公共方法：开始追击（由检测区域调用）
    public void StartChasing()
    {
        if (!isDead)
        {
            isChasing = true;
        }
    }

    // 受伤方法
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // 更新血条
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // 播放受伤音效
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // 受伤反馈（可选：颜色闪烁等）
        StartCoroutine(HurtFeedback());

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator HurtFeedback()
    {
        // 简单的红色闪烁效果
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        // 播放死亡音效
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 生成死亡特效
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 掉落物品
        HandleDrops();

        // 销毁血条
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        // 延迟销毁敌人（让音效和特效播放完）
        Destroy(gameObject, 1f);
    }

    void HandleDrops()
    {
        if (possibleDrops.Length > 0 && Random.value < dropChance)
        {
            int randomIndex = Random.Range(0, possibleDrops.Length);
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            Instantiate(possibleDrops[randomIndex], dropPosition, Quaternion.identity);
        }
    }

    void ShowWrapOverlay()
    {
        if (wrapOverlayUI != null)
        {
            wrapOverlayUI.alpha = 1f;
            wrapOverlayUI.blocksRaycasts = true;
            wrapOverlayUI.interactable = true;
        }
    }

    // 当敌人被销毁时清理
    void OnDestroy()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }
}

// 检测区域脚本（分离的组件）
public class EnemyDetectionZone : MonoBehaviour
{
    [HideInInspector]
    public DumplingEnemy parentEnemy;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && parentEnemy != null)
        {
            parentEnemy.StartChasing();
        }
    }
}