using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 50;
    public int currentHealth;
    public float invulnerabilityTime = 0.1f; // 受伤后的无敌时间

    [Header("Death Settings")]
    public GameObject[] possibleDrops;      // 可能掉落的物品（子弹包等）
    public float dropChance = 0.7f;         // 掉落概率
    public GameObject deathEffect;          // 死亡特效
    public AudioClip hurtSound;             // 受伤音效
    public AudioClip deathSound;            // 死亡音效

    [Header("Visual Feedback")]
    public float hurtFlashDuration = 0.1f;  // 受伤闪烁时间
    public Color hurtColor = Color.red;     // 受伤时的颜色

    // Private variables
    private float invulnerabilityCounter = 0f;
    private bool isDead = false;
    private AudioSource audioSource;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Material originalMaterial;

    // 引用出生系统
    private EnemySpawner parentSpawner;
    private WaveManager waveManager;

    void Start()
    {
        // 初始化血量
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        // 获取组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
            originalColor = originalMaterial.color;
        }

        // 查找出生系统引用
        waveManager = FindObjectOfType<WaveManager>();
        // parentSpawner 可以在出生时设置，或者通过其他方式获取
    }

    void Update()
    {
        // 更新无敌时间
        if (invulnerabilityCounter > 0)
        {
            invulnerabilityCounter -= Time.deltaTime;
        }
    }

    public void DamageEnemy(int bulletDamage)
    {
        // 检查是否在无敌时间内或已死亡
        if (invulnerabilityCounter > 0 || isDead)
        {
            return;
        }

        // 扣除血量
        currentHealth -= bulletDamage;

        // 设置无敌时间
        invulnerabilityCounter = invulnerabilityTime;

        // 播放受伤音效
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // 受伤视觉反馈
        StartCoroutine(HurtFlash());

        Debug.Log($"{gameObject.name} took {bulletDamage} damage. Health: {currentHealth}/{maxHealth}");

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator HurtFlash()
    {
        if (enemyRenderer != null)
        {
            // 变红
            enemyRenderer.material.color = hurtColor;

            // 等待
            yield return new WaitForSeconds(hurtFlashDuration);

            // 恢复原色
            if (enemyRenderer != null) // 确保敌人还没被销毁
            {
                enemyRenderer.material.color = originalColor;
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        // 播放死亡音效
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 生成死亡特效
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // 处理掉落物品
        HandleDrops();

        // 通知出生系统
        NotifySpawnSystem();

        // 禁用敌人组件但保留物体一小段时间（用于音效播放）
        DisableEnemyComponents();

        // 延迟销毁
        Destroy(gameObject, 1f);
    }

    void HandleDrops()
    {
        if (possibleDrops.Length > 0 && Random.value < dropChance)
        {
            // 随机选择掉落物品
            int randomIndex = Random.Range(0, possibleDrops.Length);
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;

            GameObject droppedItem = Instantiate(possibleDrops[randomIndex], dropPosition, Quaternion.identity);

            // 给掉落物品一个小的随机力
            Rigidbody dropRb = droppedItem.GetComponent<Rigidbody>();
            if (dropRb != null)
            {
                Vector3 randomForce = new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(1f, 3f),
                    Random.Range(-2f, 2f)
                );
                dropRb.AddForce(randomForce, ForceMode.Impulse);
            }

            Debug.Log($"Dropped {droppedItem.name}");
        }
    }

    void NotifySpawnSystem()
    {
        // 通知波次管理器敌人死亡
        if (waveManager != null)
        {
            waveManager.OnEnemyDeath();
        }

        // 通知出生器敌人死亡
        if (parentSpawner != null)
        {
            parentSpawner.OnEnemyDeath();
        }
        else
        {
            // 如果没有直接引用，尝试找到最近的出生器
            EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
            if (spawners.Length > 0)
            {
                // 简单起见，通知第一个找到的出生器
                spawners[0].OnEnemyDeath();
            }
        }
    }

    void DisableEnemyComponents()
    {
        // 禁用AI和移动
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // 禁用导航
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // 禁用碰撞（但保留Trigger用于掉落物品检测等）
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.enabled = false;
        }
    }

    // 公共方法：设置出生器引用
    public void SetParentSpawner(EnemySpawner spawner)
    {
        parentSpawner = spawner;
    }

    // 公共方法：治疗敌人（如果需要）
    public void HealEnemy(int healAmount)
    {
        if (!isDead)
        {
            currentHealth += healAmount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            Debug.Log($"{gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
        }
    }

    // 公共方法：获取血量信息
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
