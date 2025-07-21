using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer instance;
    private static float[] savedLevelTimes = new float[3]; // 静态存储，跨场景保持

    [Header("Timer Settings")]
    public Text timerText;
    public bool countTime = true;

    private float currentLevelTime = 0f;
    public int currentLevel = 0;
    private bool isTimerActive = true;

    void Awake()
    {
        // 每个场景都允许有新的LevelTimer，但数据持久化
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 延迟初始化，确保场景完全加载
        Invoke("InitializeLevel", 0.1f);
        Invoke("RefreshUIReferences", 0.1f);
    }

    void Update()
    {
        if (countTime && isTimerActive)
        {
            currentLevelTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void InitializeLevel()
    {
        // 获取当前关卡
        string sceneName = SceneManager.GetActiveScene().name.ToLower();
        Debug.Log($"Initializing timer for scene: {sceneName}");

        if (sceneName.Contains("level1"))
            currentLevel = 0;
        else if (sceneName.Contains("level2"))
            currentLevel = 1;
        else if (sceneName.Contains("level3"))
            currentLevel = 2;
        else
        {
            // 非游戏关卡，停止计时
            isTimerActive = false;
            countTime = false;
            return;
        }

        // 重置当前关卡时间为0（每次进入关卡都从0开始）
        currentLevelTime = 0f;
        isTimerActive = true;
        countTime = true;

        Debug.Log($"Level {currentLevel + 1} timer started, time reset to 0");
    }

    private void RefreshUIReferences()
    {
        // 场景切换后重新查找UI元素
        timerText = null; // 先清空引用

        Text[] texts = FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (var text in texts)
        {
            if (text.name.ToLower().Contains("timer") || text.name.ToLower().Contains("time"))
            {
                timerText = text;
                Debug.Log($"Found timer text: {text.name}");
                break;
            }
        }

        if (timerText == null)
        {
            Debug.LogWarning("Timer text not found in scene!");
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentLevelTime / 60f);
            int seconds = Mathf.FloorToInt(currentLevelTime % 60f);
            int milliseconds = Mathf.FloorToInt((currentLevelTime % 1f) * 100f);

            timerText.text = string.Format("Time: {0:00}:{1:00}:{2:00}",
                minutes, seconds, milliseconds);
        }
    }

    public void SaveCurrentLevelTime()
    {
        if (currentLevel >= 0 && currentLevel < savedLevelTimes.Length)
        {
            savedLevelTimes[currentLevel] = currentLevelTime;
            Debug.Log($"Level {currentLevel + 1} completed in {FormatTime(currentLevelTime)}");
        }
    }

    public void LoadNextLevel()
    {
        SaveCurrentLevelTime();

        // 通知场景切换管理器关卡完成
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.OnLevelCompleted();
        }

        int nextLevel = currentLevel + 1;

        // 加载下一关
        if (nextLevel < 3)
        {
            Debug.Log($"Loading next level: Level{nextLevel + 1}");
            SceneManager.LoadScene($"Level{nextLevel + 1}");
        }
        else
        {
            // 游戏结束，显示总时间
            ShowFinalResults();

            // 加载游戏结束场景
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGameOver();
            }
            else
            {
                SceneManager.LoadScene("GameOver");
            }
        }
    }

    void ShowFinalResults()
    {
        float totalTime = 0f;
        string results = "Game Complete!\n\n";

        for (int i = 0; i < savedLevelTimes.Length; i++)
        {
            if (savedLevelTimes[i] > 0) // 只显示已完成的关卡
            {
                results += $"Level {i + 1}: {FormatTime(savedLevelTimes[i])}\n";
                totalTime += savedLevelTimes[i];
            }
        }

        results += $"\nTotal Time: {FormatTime(totalTime)}";
        Debug.Log(results);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 暂停/恢复计时的公共方法
    public void PauseTimer()
    {
        countTime = false;
        Debug.Log("Timer paused");
    }

    public void ResumeTimer()
    {
        if (isTimerActive) // 只有在游戏关卡中才恢复计时
        {
            countTime = true;
            Debug.Log("Timer resumed");
        }
    }

    // 完全停止计时器（用于关卡完成）
    public void StopTimer()
    {
        countTime = false;
        isTimerActive = false;
        SaveCurrentLevelTime();
        Debug.Log("Timer stopped");
    }

    // 获取当前关卡时间（只读）
    public float GetCurrentLevelTime()
    {
        return currentLevelTime;
    }

    // 获取所有关卡时间
    public float[] GetAllLevelTimes()
    {
        return savedLevelTimes;
    }

    // 获取指定关卡时间
    public float GetLevelTime(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < savedLevelTimes.Length)
        {
            return savedLevelTimes[levelIndex];
        }
        return 0f;
    }

    // 为了向后兼容，提供levelTimes属性
    public float[] levelTimes
    {
        get { return savedLevelTimes; }
    }

    // 检查计时器是否正在运行
    public bool IsTimerRunning()
    {
        return countTime && isTimerActive;
    }

    void OnDestroy()
    {
        // 清理时确保数据被保存
        if (isTimerActive && currentLevelTime > 0)
        {
            SaveCurrentLevelTime();
        }
    }
}