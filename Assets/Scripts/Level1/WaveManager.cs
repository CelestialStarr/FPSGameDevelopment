using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public float levelDuration = 420f;          // 7分钟 = 420秒
    public int totalWaves = 5;                  // 总波次数
    public float waveDuration = 84f;            // 每波持续时间 (420/5)

    [Header("Enemy Spawn Control")]
    public int baseEnemiesPerWave = 6;          // 每波基础敌人数
    public float difficultyIncrease = 1.2f;     // 难度递增倍数
    public int maxSimultaneousEnemies = 12;     // 同时最多敌人数

    [Header("Time Control")]
    public float baseSpawnInterval = 25f;       // 基础生成间隔
    public float minSpawnInterval = 15f;        // 最小生成间隔
    public float intensePeriodDuration = 30f;   // 紧张期持续时间
    public float intensePeriodInterval = 120f;  // 紧张期间隔

    [Header("Enemy Type Ratios")]
    public AnimationCurve cornRatioOverTime;    // 玉米敌人比例随时间变化
    public AnimationCurve cauliflowerRatioOverTime; // 花菜敌人比例随时间变化

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
        // 获取所有生成器
        allSpawners = FindObjectsOfType<EnemySpawner>();

        // 初始化动画曲线（如果未设置）
        if (cornRatioOverTime == null || cornRatioOverTime.keys.Length == 0)
        {
            SetupDefaultCurves();
        }

        // 开始关卡
        StartLevel();
    }

    void SetupDefaultCurves()
    {
        // 玉米敌人：开始多，后期少
        cornRatioOverTime = new AnimationCurve();
        cornRatioOverTime.AddKey(0f, 0.8f);     // 开始80%
        cornRatioOverTime.AddKey(0.5f, 0.5f);   // 中期50%
        cornRatioOverTime.AddKey(1f, 0.3f);     // 结束30%

        // 花菜敌人：开始少，后期多
        cauliflowerRatioOverTime = new AnimationCurve();
        cauliflowerRatioOverTime.AddKey(0f, 0.2f);     // 开始20%
        cauliflowerRatioOverTime.AddKey(0.5f, 0.5f);   // 中期50%
        cauliflowerRatioOverTime.AddKey(1f, 0.7f);     // 结束70%
    }

    void StartLevel()
    {
        Debug.Log("Level started! Duration: " + levelDuration + " seconds");

        // 开始时间循环
        StartCoroutine(GameTimeLoop());

        // 开始紧张期循环
        StartCoroutine(IntensePeriodLoop());

        // 触发第一波
        TriggerWave(0);
    }

    IEnumerator GameTimeLoop()
    {
        while (gameTime < levelDuration)
        {
            gameTime += Time.deltaTime;

            // 更新当前波次
            int newWave = Mathf.FloorToInt(gameTime / waveDuration);
            if (newWave != currentWave && newWave < totalWaves)
            {
                TriggerWave(newWave);
            }

            // 更新生成器设置
            UpdateSpawnerSettings();

            yield return null;
        }

        // 关卡结束
        EndLevel();
    }

    IEnumerator IntensePeriodLoop()
    {
        yield return new WaitForSeconds(intensePeriodInterval);

        while (gameTime < levelDuration)
        {
            // 开始紧张期
            StartIntensePeriod();
            yield return new WaitForSeconds(intensePeriodDuration);

            // 结束紧张期
            EndIntensePeriod();
            yield return new WaitForSeconds(intensePeriodInterval - intensePeriodDuration);
        }
    }

    void TriggerWave(int waveIndex)
    {
        currentWave = waveIndex;

        Debug.Log($"Wave {currentWave + 1} started!");

        // 触发事件
        OnWaveChanged?.Invoke(currentWave);

        // 调整生成器设置
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

        // 获取当前敌人类型比例
        float cornRatio = cornRatioOverTime.Evaluate(timeProgress);
        float cauliflowerRatio = cauliflowerRatioOverTime.Evaluate(timeProgress);

        // 更新所有生成器
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

        // 加快生成速度
        foreach (EnemySpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.SetSpawnInterval(spawner.spawnInterval * 0.5f); // 生成速度加倍
            }
        }
    }

    void EndIntensePeriod()
    {
        isIntensePeriod = false;

        Debug.Log("Intense period ended!");

        // 恢复正常生成速度
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

        // 检查是否达到胜利条件
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        // 简单的胜利条件：击杀足够多的敌人
        int requiredKills = baseEnemiesPerWave * totalWaves;

        if (totalEnemiesKilled >= requiredKills)
        {
            EndLevel();
        }
    }

    void EndLevel()
    {
        Debug.Log("Level completed!");

        // 停止所有生成器
        foreach (EnemySpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.SetActive(false);
            }
        }

        // 触发关卡完成事件
        OnLevelComplete?.Invoke();
    }

    // 公共方法：获取当前状态
    public float GetTimeProgress()
    {
        return gameTime / levelDuration;
    }

    public int GetCurrentWave()
    {
        return currentWave + 1; // 返回1-based的波次
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

    // 强制结束关卡（调试用）
    [ContextMenu("Force End Level")]
    public void ForceEndLevel()
    {
        EndLevel();
    }

    // 强制开始紧张期（调试用）
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