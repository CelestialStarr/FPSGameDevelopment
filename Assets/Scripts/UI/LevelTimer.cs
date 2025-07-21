using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer instance;
    private static float[] savedLevelTimes = new float[3]; // ��̬�洢���糡������

    [Header("Timer Settings")]
    public Text timerText;
    public bool countTime = true;

    private float currentLevelTime = 0f;
    public int currentLevel = 0;
    private bool isTimerActive = true;

    void Awake()
    {
        // ÿ���������������µ�LevelTimer�������ݳ־û�
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // �ӳٳ�ʼ����ȷ��������ȫ����
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
        // ��ȡ��ǰ�ؿ�
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
            // ����Ϸ�ؿ���ֹͣ��ʱ
            isTimerActive = false;
            countTime = false;
            return;
        }

        // ���õ�ǰ�ؿ�ʱ��Ϊ0��ÿ�ν���ؿ�����0��ʼ��
        currentLevelTime = 0f;
        isTimerActive = true;
        countTime = true;

        Debug.Log($"Level {currentLevel + 1} timer started, time reset to 0");
    }

    private void RefreshUIReferences()
    {
        // �����л������²���UIԪ��
        timerText = null; // ���������

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

        // ֪ͨ�����л��������ؿ����
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.OnLevelCompleted();
        }

        int nextLevel = currentLevel + 1;

        // ������һ��
        if (nextLevel < 3)
        {
            Debug.Log($"Loading next level: Level{nextLevel + 1}");
            SceneManager.LoadScene($"Level{nextLevel + 1}");
        }
        else
        {
            // ��Ϸ��������ʾ��ʱ��
            ShowFinalResults();

            // ������Ϸ��������
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
            if (savedLevelTimes[i] > 0) // ֻ��ʾ����ɵĹؿ�
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

    // ��ͣ/�ָ���ʱ�Ĺ�������
    public void PauseTimer()
    {
        countTime = false;
        Debug.Log("Timer paused");
    }

    public void ResumeTimer()
    {
        if (isTimerActive) // ֻ������Ϸ�ؿ��вŻָ���ʱ
        {
            countTime = true;
            Debug.Log("Timer resumed");
        }
    }

    // ��ȫֹͣ��ʱ�������ڹؿ���ɣ�
    public void StopTimer()
    {
        countTime = false;
        isTimerActive = false;
        SaveCurrentLevelTime();
        Debug.Log("Timer stopped");
    }

    // ��ȡ��ǰ�ؿ�ʱ�䣨ֻ����
    public float GetCurrentLevelTime()
    {
        return currentLevelTime;
    }

    // ��ȡ���йؿ�ʱ��
    public float[] GetAllLevelTimes()
    {
        return savedLevelTimes;
    }

    // ��ȡָ���ؿ�ʱ��
    public float GetLevelTime(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < savedLevelTimes.Length)
        {
            return savedLevelTimes[levelIndex];
        }
        return 0f;
    }

    // Ϊ�������ݣ��ṩlevelTimes����
    public float[] levelTimes
    {
        get { return savedLevelTimes; }
    }

    // ����ʱ���Ƿ���������
    public bool IsTimerRunning()
    {
        return countTime && isTimerActive;
    }

    void OnDestroy()
    {
        // ����ʱȷ�����ݱ�����
        if (isTimerActive && currentLevelTime > 0)
        {
            SaveCurrentLevelTime();
        }
    }
}