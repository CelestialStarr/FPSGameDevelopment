using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer instance;

    [Header("Timer Settings")]
    public Text timerText;
    public bool countTime = true;

    [Header("Level Times")]
    public float[] levelTimes = new float[3]; // 存储每关的时间

    private float currentLevelTime = 0f;
    public int currentLevel = 0;
    private bool isTimerActive = true;

    void Awake()
    {
        // 改进的单例模式 - 每个场景重新创建，但保持数据
        if (instance != null && instance != this)
        {
            // 复制之前的数据
            levelTimes = instance.levelTimes;

            // 销毁旧实例
            Destroy(instance.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeLevel();
        RefreshUIReferences();
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
            return;
        }

        // 重置当前关卡时间
        currentLevelTime = 0f;
        isTimerActive = true;
        countTime = true;

        Debug.Log($"Level {currentLevel + 1} timer started");
    }

    private void RefreshUIReferences()
    {
        // 场景切换后重新查找UI元素
        if (timerText == null)
        {
            Text[] texts = FindObjectsOfType<Text>();
            foreach (var text in texts)
            {
                if (text.name.Contains("Timer") || text.name.Contains("Time"))
                {
                    timerText = text;
                    break;
                }
            }
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
        if (currentLevel >= 0 && currentLevel < levelTimes.Length)
        {
            levelTimes[currentLevel] = currentLevelTime;
            Debug.Log($"Level {currentLevel + 1} completed in {FormatTime(currentLevelTime)}");
        }
    }

    public void LoadNextLevel()
    {
        SaveCurrentLevelTime();

        int nextLevel = currentLevel + 1;

        // 加载下一关
        if (nextLevel < 3)
        {
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

        for (int i = 0; i < levelTimes.Length; i++)
        {
            if (levelTimes[i] > 0) // 只显示已完成的关卡
            {
                results += $"Level {i + 1}: {FormatTime(levelTimes[i])}\n";
                totalTime += levelTimes[i];
            }
        }

        results += $"\nTotal Time: {FormatTime(totalTime)}";
        Debug.Log(results);

        // 这里可以触发最终结果UI显示事件
        // 例如：GameEvents.OnGameComplete?.Invoke(levelTimes, totalTime);
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