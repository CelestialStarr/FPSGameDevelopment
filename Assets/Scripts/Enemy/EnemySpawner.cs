using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;           // �����ɵĵ���Ԥ��������
    public Transform[] spawnPoints;             // ���������λ�ã��������������������壩
    public float spawnInterval = 3f;            // ���ɼ��
    public int maxEnemiesAtOnce = 3;            // ���������ͬʱ��༸������

    [Header("Enemy Types")]
    [Range(0f, 1f)]
    public float cornEnemyChance = 0.7f;        // ���׵��˸���
    [Range(0f, 1f)]
    public float cauliflowerChance = 0.3f;      // ���˵��˸���

    [Header("Spawn Control")]
    public bool isActive = true;                // �Ƿ񼤻�
    public bool spawnOnStart = true;            // ��Ϸ��ʼʱ������
    public float firstSpawnDelay = 2f;          // ��һ�����ɵ��ӳ�

    [Header("Advanced Settings")]
    public float spawnHeightOffset = 0.5f;      // ���ɸ߶�ƫ��
    public GameObject spawnEffect;              // ������Ч
    public AudioClip spawnSound;                // ������Ч

    // Private variables
    private int currentEnemyCount = 0;          // ��ǰ���ŵĵ�������
    private bool isSpawning = false;            // �Ƿ��������ɹ�����
    private AudioSource audioSource;
    private WaveManager waveManager;            // ���ò��ι�����

    void Start()
    {
        // ��ȡ��Ƶ���
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ��ȡ���ι�����
        waveManager = FindObjectOfType<WaveManager>();

        // ���û���������ɵ㣬ʹ���Լ���λ��
        if (spawnPoints.Length == 0)
        {
            spawnPoints = new Transform[] { transform };
        }

        // ��ʼ����
        if (spawnOnStart && isActive)
        {
            StartCoroutine(StartSpawning());
        }
    }

    IEnumerator StartSpawning()
    {
        // �ȴ���ʼ�ӳ�
        yield return new WaitForSeconds(firstSpawnDelay);

        // ��ʼ��������ѭ��
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (isActive)
        {
            // ����Ƿ���Ҫ���ɵ���
            if (currentEnemyCount < maxEnemiesAtOnce && enemyPrefabs.Length > 0)
            {
                SpawnEnemy();
            }

            // �ȴ��´�����
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

        // ѡ���������
        GameObject enemyToSpawn = SelectEnemyType();

        // ѡ������λ��
        Transform spawnPoint = SelectSpawnPoint();
        Vector3 spawnPosition = spawnPoint.position + Vector3.up * spawnHeightOffset;

        // ����������Ч
        if (spawnEffect != null)
        {
            GameObject effect = Instantiate(spawnEffect, spawnPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // ����������Ч
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        // �����ӳ٣���Чʱ�䣩
        yield return new WaitForSeconds(0.5f);

        // ���ɵ���
        GameObject newEnemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);

        // Ϊ���������������
        AddDeathListener(newEnemy);

        // ���Ӽ���
        currentEnemyCount++;

        // ֪ͨ���ι�����
        if (waveManager != null)
        {
            waveManager.OnEnemySpawned();
        }

        isSpawning = false;

        Debug.Log($"Spawned {enemyToSpawn.name} at {spawnPoint.name}. Current count: {currentEnemyCount}");
    }

    GameObject SelectEnemyType()
    {
        // ���ݸ���ѡ���������
        float random = Random.value;

        if (random <= cornEnemyChance)
        {
            // ѡ����������ˣ���������ǰ�벿�������ף�
            int cornIndex = Random.Range(0, Mathf.Min(enemyPrefabs.Length, enemyPrefabs.Length / 2));
            return enemyPrefabs[cornIndex];
        }
        else
        {
            // ѡ�񻨲�����ˣ����������벿���ǻ��ˣ�
            int cauliflowerIndex = Random.Range(enemyPrefabs.Length / 2, enemyPrefabs.Length);
            return enemyPrefabs[cauliflowerIndex];
        }
    }

    Transform SelectSpawnPoint()
    {
        // ���ѡ��һ�����ɵ�
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    void AddDeathListener(GameObject enemy)
    {
        // Ϊ�������ó���������
        EnemyHealthController healthController = enemy.GetComponent<EnemyHealthController>();
        if (healthController != null)
        {
            healthController.SetParentSpawner(this);
        }

        // Ҳ���Լ�ص���������˫�ر��գ�
        StartCoroutine(MonitorEnemyLife(enemy));
    }

    IEnumerator MonitorEnemyLife(GameObject enemy)
    {
        // ��ص����Ƿ񻹻���
        while (enemy != null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // �������������ټ���
        OnEnemyDeath();
    }

    public void OnEnemyDeath()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount);

        // ֪ͨ���ι�����
        if (waveManager != null)
        {
            waveManager.OnEnemyDeath();
        }

        Debug.Log($"Enemy died. Current count: {currentEnemyCount}");
    }

    // �����������ֶ����ɵ���
    public void ForceSpawn()
    {
        if (enemyPrefabs.Length > 0)
        {
            SpawnEnemy();
        }
    }

    // �������������ü���״̬
    public void SetActive(bool active)
    {
        isActive = active;

        if (active && !isSpawning)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    // �����������������ɼ��
    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = newInterval;
    }

    // ���������������������͸���
    public void SetEnemyTypeRatio(float cornChance, float cauliflowerChance)
    {
        cornEnemyChance = cornChance;
        cauliflowerChance = cauliflowerChance;
    }

    // �ڱ༭������ʾ���ɷ�Χ
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