using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public float levelDuration = 420f;          // 7���� = 420��
    public int totalWaves = 5;                  // �ܲ�����
    public float waveDuration = 84f;            // ÿ������ʱ�� (420/5)

    [Header("Enemy Spawn Control")]
    public int baseEnemiesPerWave = 6;          // ÿ������������
    public float difficultyIncrease = 1.2f;     // �Ѷȵ�������
    public int maxSimultaneousEnemies = 12;     // ͬʱ��������

    [Header("Time Control")]
    public float baseSpawnInterval = 25f;       // �������ɼ��
    public float minSpawnInterval = 15f;        // ��С���ɼ��
    public float intensePeriodDuration = 30f;   // �����ڳ���ʱ��
    public float intensePeriodInterval = 120f;  // �����ڼ��

    [Header("Enemy Type Ratios")]
    public AnimationCurve cornRatioOverTime;    // ���׵��˱�����ʱ��仯
    public AnimationCurve cauliflowerRatioOverTime; // ���˵��˱�����ʱ��仯

    // Private variables
    private int currentWave = 0;
    private float gameTime = 0f;
    private int totalEnemiesSpawned = 0;
    private int totalEnemiesKilled = 0;
    private bool isIntensePeriod = false;
    private EnemySpawner[] allSpawners;

    // Events
    public System.Action<int> OnWaveChanged;
    public System.Action OnLevelComplete;
    public System.Action<int, int> OnEnemyCountChanged; // (killed, spawned)

    void Start()
    {
        // ��ȡ����������
        allSpawners = FindObjectsOfType<EnemySpawner>();

        // ��ʼ���������ߣ����δ���ã�
        if (cornRatioOverTime == null || cornRatioOverTime.keys.Length == 0)
        {
            SetupDefaultCurves();
        }

        // ��ʼ�ؿ�
        StartLevel();
    }

    void SetupDefaultCurves()
    {
        // ���׵��ˣ���ʼ�࣬������
        cornRatioOverTime = new AnimationCurve();
        cornRatioOverTime.AddKey(0f, 0.8f);     // ��ʼ80%
        cornRatioOverTime.AddKey(0.5f, 0.5f);   // ����50%
        cornRatioOverTime.AddKey(1f, 0.3f);     // ����30%

        // ���˵��ˣ���ʼ�٣����ڶ�
        cauliflowerRatioOverTime = new AnimationCurve();
        cauliflowerRatioOverTime.AddKey(0f, 0.2f);     // ��ʼ20%
        cauliflowerRatioOverTime.AddKey(0.5f, 0.5f);   // ����50%
        cauliflowerRatioOverTime.AddKey(1f, 0.7f);     // ����70%
    }

    void StartLevel()
    {
        Debug.Log("Level started! Duration: " + levelDuration + " seconds");

        // ��ʼʱ��ѭ��
        StartCoroutine(GameTimeLoop());

        // ��ʼ������ѭ��
        StartCoroutine(IntensePeriodLoop());

        // ������һ��
        TriggerWave(0);
    }

    IEnumerator GameTimeLoop()
    {
        while (gameTime < levelDuration)
        {
            gameTime += Time.deltaTime;

            // ���µ�ǰ����
            int newWave = Mathf.FloorToInt(gameTime / waveDuration);
            if (newWave != currentWave && newWave < totalWaves)
            {
                TriggerWave(newWave);
            }

            // ��������������
            UpdateSpawnerSettings();

            yield return null;
        }

        // �ؿ�����
        EndLevel();
    }

    IEnumerator IntensePeriodLoop()
    {
        yield return new WaitForSeconds(intensePeriodInterval);

        while (gameTime < levelDuration)
        {
            // ��ʼ������
            StartIntensePeriod();
            yield return new WaitForSeconds(intensePeriodDuration);

            // ����������
            EndIntensePeriod();
            yield return new WaitForSeconds(intensePeriodInterval - intensePeriodDuration);
        }
    }

    void TriggerWave(int waveIndex)
    {
        currentWave = waveIndex;

        Debug.Log($"Wave {currentWave + 1} started!");

        // �����¼�
        OnWaveChanged?.Invoke(currentWave);

        // ��������������
        AdjustSpawnersForWave();
    }

    void AdjustSpawnersForWave()
    {
        float waveMultiplier = Mathf.Pow(difficultyIncrease, currentWave);
        float newInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval / waveMultiplier);

        foreach (EnemySpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.SetSpawnInterval(newInterval);
                spawner.maxEnemiesAtOnce = Mathf.RoundToInt(3 * waveMultiplier);
            }
        }

        Debug.Log($"Wave {currentWave + 1}: Spawn interval = {newInterval:F1}s, Max enemies per spawner = {Mathf.RoundToInt(3 * waveMultiplier)}");
    }

    void UpdateSpawnerSettings()
    {
        float timeProgress = gameTime / levelDuration;

        // ��ȡ��ǰ�������ͱ���
        float cornRatio = cornRatioOverTime.Evaluate(timeProgress);
        float cauliflowerRatio = cauliflowerRatioOverTime.Evaluate(timeProgress);

        // ��������������
        foreach (EnemySpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.SetEnemyTypeRatio(cornRatio, cauliflowerRatio);
            }
        }
    }

    void StartIntensePeriod()
    {
        isIntensePeriod = true;

        Debug.Log("Intense period started!");

        // �ӿ������ٶ�
        foreach (EnemySpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.SetSpawnInterval(spawner.spawnInterval * 0.5f); // �����ٶȼӱ�
            }
        }
    }

    void EndIntensePeriod()
    {
        isIntensePeriod = false;

        Debug.Log("Intense period ended!");

        // �ָ����������ٶ�
        AdjustSpawnersForWave();
    }

    public void OnEnemySpawned()
    {
        totalEnemiesSpawned++;
        OnEnemyCountChanged?.Invoke(totalEnemiesKilled, totalEnemiesSpawned);

        Debug.Log($"Enemy spawned. Total: {totalEnemiesSpawned}");
    }

    public void OnEnemyDeath()
    {
        totalEnemiesKilled++;
        OnEnemyCountChanged?.Invoke(totalEnemiesKilled, totalEnemiesSpawned);

        Debug.Log($"Enemy killed. Total killed: {totalEnemiesKilled}/{totalEnemiesSpawned}");

        // ����Ƿ�ﵽʤ������
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        // �򵥵�ʤ����������ɱ�㹻��ĵ���
        int requiredKills = baseEnemiesPerWave * totalWaves;

        if (totalEnemiesKilled >= requiredKills)
        {
            EndLevel();
        }
    }

    void EndLevel()
    {
        Debug.Log("Level completed!");

        // ֹͣ����������
        foreach (EnemySpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.SetActive(false);
            }
        }

        // �����ؿ�����¼�
        OnLevelComplete?.Invoke();
    }

    // ������������ȡ��ǰ״̬
    public float GetTimeProgress()
    {
        return gameTime / levelDuration;
    }

    public int GetCurrentWave()
    {
        return currentWave + 1; // ����1-based�Ĳ���
    }

    public string GetTimeRemaining()
    {
        float remaining = levelDuration - gameTime;
        int minutes = Mathf.FloorToInt(remaining / 60);
        int seconds = Mathf.FloorToInt(remaining % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public bool IsIntensePeriod()
    {
        return isIntensePeriod;
    }

    // ǿ�ƽ����ؿ��������ã�
    [ContextMenu("Force End Level")]
    public void ForceEndLevel()
    {
        EndLevel();
    }

    // ǿ�ƿ�ʼ�����ڣ������ã�
    [ContextMenu("Force Intense Period")]
    public void ForceIntensePeriod()
    {
        StartCoroutine(ForceIntenseCoroutine());
    }

    IEnumerator ForceIntenseCoroutine()
    {
        StartIntensePeriod();
        yield return new WaitForSeconds(10f);
        EndIntensePeriod();
    }
}