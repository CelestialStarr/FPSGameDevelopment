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
    public float[] levelTimes = new float[3]; // �洢ÿ�ص�ʱ��

    private float currentLevelTime = 0f;
    public int currentLevel = 0;
    private bool isTimerActive = true;

    void Awake()
    {
        // �Ľ��ĵ���ģʽ - ÿ���������´���������������
        if (instance != null && instance != this)
        {
            // ����֮ǰ������
            levelTimes = instance.levelTimes;

            // ���پ�ʵ��
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
        // ��ȡ��ǰ�ؿ�
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

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
            return;
        }

        // ���õ�ǰ�ؿ�ʱ��
        currentLevelTime = 0f;
        isTimerActive = true;
        countTime = true;

        Debug.Log($"Level {currentLevel + 1} timer started");
    }

    private void RefreshUIReferences()
    {
        // �����л������²���UIԪ��
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

        // ������һ��
        if (nextLevel < 3)
        {
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

        for (int i = 0; i < levelTimes.Length; i++)
        {
            if (levelTimes[i] > 0) // ֻ��ʾ����ɵĹؿ�
            {
                results += $"Level {i + 1}: {FormatTime(levelTimes[i])}\n";
                totalTime += levelTimes[i];
            }
        }

        results += $"\nTotal Time: {FormatTime(totalTime)}";
        Debug.Log(results);

        // ������Դ������ս��UI��ʾ�¼�
        // ���磺GameEvents.OnGameComplete?.Invoke(levelTimes, totalTime);
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