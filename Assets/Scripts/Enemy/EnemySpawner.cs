using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;           // 可生成的敌人预制体数组
    public Transform[] spawnPoints;             // 具体的生成位置（可以是这个物体的子物体）
    public float spawnInterval = 3f;            // 生成间隔
    public int maxEnemiesAtOnce = 3;            // 这个生成器同时最多几个敌人

    [Header("Enemy Types")]
    [Range(0f, 1f)]
    public float cornEnemyChance = 0.7f;        // 玉米敌人概率
    [Range(0f, 1f)]
    public float cauliflowerChance = 0.3f;      // 花菜敌人概率

    [Header("Spawn Control")]
    public bool isActive = true;                // 是否激活
    public bool spawnOnStart = true;            // 游戏开始时就生成
    public float firstSpawnDelay = 2f;          // 第一次生成的延迟

    [Header("Advanced Settings")]
    public float spawnHeightOffset = 0.5f;      // 生成高度偏移
    public GameObject spawnEffect;              // 生成特效
    public AudioClip spawnSound;                // 生成音效

    // Private variables
    private int currentEnemyCount = 0;          // 当前活着的敌人数量
    private bool isSpawning = false;            // 是否正在生成过程中
    private AudioSource audioSource;
    private WaveManager waveManager;            // 引用波次管理器

    void Start()
    {
        // 获取音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 获取波次管理器
        waveManager = FindObjectOfType<WaveManager>();

        // 如果没有设置生成点，使用自己的位置
        if (spawnPoints.Length == 0)
        {
            spawnPoints = new Transform[] { transform };
        }

        // 开始生成
        if (spawnOnStart && isActive)
        {
            StartCoroutine(StartSpawning());
        }
    }

    IEnumerator StartSpawning()
    {
        // 等待初始延迟
        yield return new WaitForSeconds(firstSpawnDelay);

        // 开始持续生成循环
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (isActive)
        {
            // 检查是否需要生成敌人
            if (currentEnemyCount < maxEnemiesAtOnce && enemyPrefabs.Length > 0)
            {
                SpawnEnemy();
            }

            // 等待下次生成
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnEnemy()
    {
        if (isSpawning || enemyPrefabs.Length == 0) return;

        StartCoroutine(SpawnEnemyCoroutine());
    }

    IEnumerator SpawnEnemyCoroutine()
    {
        isSpawning = true;

        // 选择敌人类型
        GameObject enemyToSpawn = SelectEnemyType();

        // 选择生成位置
        Transform spawnPoint = SelectSpawnPoint();
        Vector3 spawnPosition = spawnPoint.position + Vector3.up * spawnHeightOffset;

        // 播放生成特效
        if (spawnEffect != null)
        {
            GameObject effect = Instantiate(spawnEffect, spawnPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 播放生成音效
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        // 短暂延迟（特效时间）
        yield return new WaitForSeconds(0.5f);

        // 生成敌人
        GameObject newEnemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);

        // 为敌人添加死亡监听
        AddDeathListener(newEnemy);

        // 增加计数
        currentEnemyCount++;

        // 通知波次管理器
        if (waveManager != null)
        {
            waveManager.OnEnemySpawned();
        }

        isSpawning = false;

        Debug.Log($"Spawned {enemyToSpawn.name} at {spawnPoint.name}. Current count: {currentEnemyCount}");
    }

    GameObject SelectEnemyType()
    {
        // 根据概率选择敌人类型
        float random = Random.value;

        if (random <= cornEnemyChance)
        {
            // 选择玉米类敌人（假设数组前半部分是玉米）
            int cornIndex = Random.Range(0, Mathf.Min(enemyPrefabs.Length, enemyPrefabs.Length / 2));
            return enemyPrefabs[cornIndex];
        }
        else
        {
            // 选择花菜类敌人（假设数组后半部分是花菜）
            int cauliflowerIndex = Random.Range(enemyPrefabs.Length / 2, enemyPrefabs.Length);
            return enemyPrefabs[cauliflowerIndex];
        }
    }

    Transform SelectSpawnPoint()
    {
        // 随机选择一个生成点
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    void AddDeathListener(GameObject enemy)
    {
        // 为敌人设置出生器引用
        EnemyHealthController healthController = enemy.GetComponent<EnemyHealthController>();
        if (healthController != null)
        {
            healthController.SetParentSpawner(this);
        }

        // 也可以监控敌人生命（双重保险）
        StartCoroutine(MonitorEnemyLife(enemy));
    }

    IEnumerator MonitorEnemyLife(GameObject enemy)
    {
        // 监控敌人是否还活着
        while (enemy != null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 敌人死亡，减少计数
        OnEnemyDeath();
    }

    public void OnEnemyDeath()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount);

        // 通知波次管理器
        if (waveManager != null)
        {
            waveManager.OnEnemyDeath();
        }

        Debug.Log($"Enemy died. Current count: {currentEnemyCount}");
    }

    // 公共方法：手动生成敌人
    public void ForceSpawn()
    {
        if (enemyPrefabs.Length > 0)
        {
            SpawnEnemy();
        }
    }

    // 公共方法：设置激活状态
    public void SetActive(bool active)
    {
        isActive = active;

        if (active && !isSpawning)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    // 公共方法：调整生成间隔
    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = newInterval;
    }

    // 公共方法：调整敌人类型概率
    public void SetEnemyTypeRatio(float cornChance, float cauliflowerChance)
    {
        cornEnemyChance = cornChance;
        cauliflowerChance = cauliflowerChance;
    }

    // 在编辑器中显示生成范围
    void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                    Gizmos.DrawLine(point.position, point.position + Vector3.up * 2f);
                }
            }
        }
    }
}