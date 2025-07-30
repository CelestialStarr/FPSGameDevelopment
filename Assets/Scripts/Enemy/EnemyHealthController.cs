using UnityEngine;
using System.Collections;

public class EnemyHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 50;
    public int currentHealth;
    public float invulnerabilityTime = 0.1f;

    [Header("Death Settings")]
    public GameObject[] possibleDrops;
    public float dropChance = 0.7f;
    public float dropHeight = 0.2f;
    public GameObject deathEffect;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Visual Feedback")]
    public float hurtFlashDuration = 0.1f;
    public Color hurtColor = Color.red;

    [Header("Boss Settings")]
    public bool isBoss = false;

    [Header("Boss Special Features (Only if isBoss = true)")]
    public int bossExtraDrops = 2; // Boss额外掉落数量
    public GameObject[] bossSpecialDrops; // Boss专属掉落
    public float bossDeathEffectDuration = 5f; // Boss死亡特效持续时间
    public AudioClip bossDeathSound; // Boss专属死亡音效
    public float bossEnrageThreshold = 0.3f; // 30%血量时狂暴
    public Color bossEnrageColor = Color.red; // 狂暴时颜色

    [Header("Boss Summon Attack")]
    public GameObject minionPrefab; // 小饺子预制体
    public int summonCount = 2; // 每次召唤数量
    public float summonCooldown = 12f; // 召唤冷却时间
    public float summonRange = 5f; // 召唤范围
    public GameObject summonEffect; // 召唤特效
    public AudioClip summonSound; // 召唤音效

    // Private variables
    private float invulnerabilityCounter = 0f;
    private bool isDead = false;
    private bool isBossEnraged = false;
    private AudioSource audioSource;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Material originalMaterial;

    // Boss召唤相关
    private float lastSummonTime = 0f;
    private bool isPerformingSummon = false;

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

        // Boss初始化提示
        if (isBoss)
        {
            Debug.Log($"[Boss] {gameObject.name} Boss initialized with {maxHealth} health!");
        }
    }

    void Update()
    {
        // 更新无敌时间
        if (invulnerabilityCounter > 0)
        {
            invulnerabilityCounter -= Time.deltaTime;
        }

        // Boss狂暴检查
        if (isBoss && !isBossEnraged && !isDead)
        {
            CheckBossEnrage();
        }

        // Boss召唤检查
        if (isBoss && !isDead && !isPerformingSummon)
        {
            CheckBossSummon();
        }
    }

    void CheckBossSummon()
    {
        // 召唤攻击：冷却时间到了就召唤
        if (Time.time >= lastSummonTime + summonCooldown)
        {
            StartCoroutine(PerformSummonAttack());
        }
    }

    IEnumerator PerformSummonAttack()
    {
        isPerformingSummon = true;
        lastSummonTime = Time.time;

        Debug.Log("[Boss] Boss is summoning minions!");

        // 播放召唤音效
        if (summonSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(summonSound);
        }

        // 召唤延迟
        yield return new WaitForSeconds(1f);

        // 召唤小饺子
        for (int i = 0; i < summonCount; i++)
        {
            if (minionPrefab != null)
            {
                // 在Boss周围随机位置召唤
                float angle = (360f / summonCount) * i + Random.Range(-30f, 30f);
                float radian = angle * Mathf.Deg2Rad;
                Vector3 spawnOffset = new Vector3(
                    Mathf.Cos(radian) * summonRange,
                    0.5f,
                    Mathf.Sin(radian) * summonRange
                );
                Vector3 spawnPosition = transform.position + spawnOffset;

                // 召唤特效
                if (summonEffect != null)
                {
                    GameObject effect = Instantiate(summonEffect, spawnPosition, Quaternion.identity);
                    Destroy(effect, 2f);
                }

                // 召唤小怪
                GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"[Boss] Summoned minion #{i + 1} at {spawnPosition}");

                // 间隔召唤
                if (i < summonCount - 1)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        Debug.Log("[Boss] Summoning complete!");
        isPerformingSummon = false;
    }

    void CheckBossEnrage()
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        if (healthPercentage <= bossEnrageThreshold)
        {
            EnterBossEnrageMode();
        }
    }

    void EnterBossEnrageMode()
    {
        isBossEnraged = true;
        Debug.Log("[Boss] Boss is now ENRAGED!");

        // 改变颜色表示狂暴
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.Lerp(originalColor, bossEnrageColor, 0.6f);
        }

        // 增加移动速度（如果有NavMeshAgent）
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.speed *= 1.5f;
            Debug.Log("[Boss] Boss speed increased!");
        }

        // 狂暴时召唤冷却时间减少
        summonCooldown *= 0.7f; // 减少30%冷却时间
        Debug.Log("[Boss] Summon cooldown reduced!");
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

        // 不同的伤害日志
        if (isBoss)
        {
            Debug.Log($"[Boss] {gameObject.name} took {bulletDamage} damage. Health: {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.Log($"{gameObject.name} took {bulletDamage} damage. Health: {currentHealth}/{maxHealth}");
        }

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

            // 恢复颜色
            if (enemyRenderer != null)
            {
                if (isBoss && isBossEnraged)
                {
                    // 如果是狂暴Boss，恢复狂暴颜色
                    enemyRenderer.material.color = Color.Lerp(originalColor, bossEnrageColor, 0.6f);
                }
                else
                {
                    // 普通颜色
                    enemyRenderer.material.color = originalColor;
                }
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (isBoss)
        {
            Debug.Log($"[Boss] {gameObject.name} Boss has been defeated!");
        }
        else
        {
            Debug.Log($"{gameObject.name} died!");
        }

        // 播放死亡音效 - Boss有专属音效
        AudioClip soundToPlay = (isBoss && bossDeathSound != null) ? bossDeathSound : deathSound;
        if (soundToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }

        // 生成死亡特效
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            float effectDuration = isBoss ? bossDeathEffectDuration : 3f;
            Destroy(effect, effectDuration);
        }

        // 处理掉落物品
        HandleDrops();

        // 通知出生系统
        NotifySpawnSystem();

        // 禁用敌人组件
        DisableEnemyComponents();

        // DumplingEnemy脚本处理
        DumplingEnemy dumplingScript = GetComponent<DumplingEnemy>();
        if (dumplingScript != null)
        {
            dumplingScript.OnEnemyDeath();
        }

        // 通知任务系统
        if (isBoss && MissionSystem.Instance != null)
        {
            MissionSystem.Instance.OnBossDefeated();
            Debug.Log("[Boss] Boss defeated! Mission system notified.");
        }
        else if (MissionSystem.Instance != null)
        {
            MissionSystem.Instance.OnEnemyKilled();
            Debug.Log("[Enemy] Enemy killed! Mission system notified.");
        }

        // Boss延迟销毁时间更长
        float destroyDelay = isBoss ? 3f : 1f;
        Destroy(gameObject, destroyDelay);
    }

    void HandleDrops()
    {
        if (isBoss)
        {
            HandleBossDrops();
        }
        else
        {
            HandleNormalDrops();
        }
    }

    void HandleNormalDrops()
    {
        // 原来的普通掉落逻辑
        if (possibleDrops.Length > 0 && Random.value < dropChance)
        {
            int randomIndex = Random.Range(0, possibleDrops.Length);
            Vector3 dropPosition = transform.position + Vector3.up * dropHeight;

            GameObject droppedItem = Instantiate(possibleDrops[randomIndex], dropPosition, Quaternion.identity);

            Rigidbody dropRb = droppedItem.GetComponent<Rigidbody>();
            if (dropRb != null)
            {
                Vector3 randomForce = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1.5f),
                    Random.Range(-1f, 1f)
                );
                dropRb.AddForce(randomForce, ForceMode.Impulse);
            }

            Debug.Log($"Dropped {droppedItem.name}");
        }
    }

    void HandleBossDrops()
    {
        Debug.Log("[Boss] Handling boss drops...");

        // Boss掉落多个普通物品
        if (possibleDrops.Length > 0)
        {
            int dropCount = bossExtraDrops + 1; // 至少掉落1个，加上额外数量
            for (int i = 0; i < dropCount; i++)
            {
                if (Random.value < dropChance)
                {
                    int randomIndex = Random.Range(0, possibleDrops.Length);
                    Vector3 dropPosition = transform.position + Vector3.up * dropHeight +
                                         new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));

                    GameObject droppedItem = Instantiate(possibleDrops[randomIndex], dropPosition, Quaternion.identity);

                    Rigidbody dropRb = droppedItem.GetComponent<Rigidbody>();
                    if (dropRb != null)
                    {
                        Vector3 randomForce = new Vector3(
                            Random.Range(-2f, 2f),
                            Random.Range(1f, 2f),
                            Random.Range(-2f, 2f)
                        );
                        dropRb.AddForce(randomForce, ForceMode.Impulse);
                    }

                    Debug.Log($"[Boss] Dropped {droppedItem.name}");
                }
            }
        }

        // Boss专属掉落
        if (bossSpecialDrops.Length > 0)
        {
            foreach (GameObject specialDrop in bossSpecialDrops)
            {
                if (specialDrop != null)
                {
                    Vector3 dropPosition = transform.position + Vector3.up * dropHeight +
                                         new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                    GameObject dropped = Instantiate(specialDrop, dropPosition, Quaternion.identity);
                    Debug.Log($"[Boss] Dropped special item: {dropped.name}");
                }
            }
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

            string logPrefix = isBoss ? "[Boss]" : "";
            Debug.Log($"{logPrefix} {gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
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

    // Boss专用方法
    public bool IsBoss()
    {
        return isBoss;
    }

    public bool IsBossEnraged()
    {
        return isBoss && isBossEnraged;
    }

    // 在Scene视图中显示召唤范围
    void OnDrawGizmosSelected()
    {
        if (isBoss)
        {
            // 召唤范围
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, summonRange);
        }
    }
}