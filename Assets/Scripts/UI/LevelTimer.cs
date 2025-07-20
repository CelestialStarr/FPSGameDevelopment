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
    private int currentLevel = 0;

    void Awake()
    {
        // ����ģʽ����ÿ������
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ��ȡ��ǰ�ؿ�
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level1") || sceneName.Contains("level1"))
            currentLevel = 0;
        else if (sceneName.Contains("Level2") || sceneName.Contains("level2"))
            currentLevel = 1;
        else if (sceneName.Contains("Level3") || sceneName.Contains("level3"))
            currentLevel = 2;

        // ���õ�ǰ�ؿ�ʱ��
        currentLevelTime = 0f;
    }

    void Update()
    {
        if (countTime)
        {
            currentLevelTime += Time.deltaTime;
            UpdateTimerDisplay();
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
            Debug.Log($"Level {currentLevel + 1} completed in {currentLevelTime:F2} seconds");
        }
    }

    public void LoadNextLevel()
    {
        SaveCurrentLevelTime();
        currentLevel++;

        // ������һ��
        if (currentLevel < 3)
        {
            SceneManager.LoadScene("Level" + (currentLevel + 1));
        }
        else
        {
            // ��Ϸ��������ʾ��ʱ��
            ShowFinalResults();
        }
    }

    void ShowFinalResults()
    {
        float totalTime = 0f;
        string results = "Game Complete!\n\n";

        for (int i = 0; i < levelTimes.Length; i++)
        {
            results += $"Level {i + 1}: {FormatTime(levelTimes[i])}\n";
            totalTime += levelTimes[i];
        }

        results += $"\nTotal Time: {FormatTime(totalTime)}";
        Debug.Log(results);

        // ���������ʾ���UI
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // ��ͣ/�ָ���ʱ
    public void PauseTimer()
    {
        countTime = false;
    }

    public void ResumeTimer()
    {
        countTime = true;
    }
}
