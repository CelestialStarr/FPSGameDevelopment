using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject endPanel;
    public GameObject mainMenuPanel;

    [Header("Timer")]
    public Text timerText;
    private float currentTime = 0f;
    private bool isCounting = true;

    [Header("Level Info")]
    public float[] levelTimes = new float[3];
    private int currentLevelIndex = 0;

    [Header("Video (演出关卡)")]
    public VideoPlayer cutscenePlayer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Level"))
        {
            InitLevel();
        }

        if (scene.name == "Cutscene")
        {
            if (cutscenePlayer != null)
            {
                cutscenePlayer.loopPointReached += OnVideoFinished;
                cutscenePlayer.Play();
            }
        }
    }

    void Update()
    {
        if (isCounting && SceneManager.GetActiveScene().name.Contains("Level"))
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }

        // 测试暂停功能用（也可用 UI 按钮）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null)
            {
                bool isPaused = pausePanel.activeSelf;
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }
    }

    void InitLevel()
    {
        currentTime = 0f;
        isCounting = true;
        pausePanel?.SetActive(false);
        endPanel?.SetActive(false);
        mainMenuPanel?.SetActive(false);

        string name = SceneManager.GetActiveScene().name;
        if (name.Contains("Level1")) currentLevelIndex = 0;
        else if (name.Contains("Level2")) currentLevelIndex = 1;
        else if (name.Contains("Level3")) currentLevelIndex = 2;
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int min = Mathf.FloorToInt(currentTime / 60f);
            int sec = Mathf.FloorToInt(currentTime % 60f);
            int ms = Mathf.FloorToInt((currentTime % 1f) * 100f);
            timerText.text = $"Time: {min:00}:{sec:00}:{ms:00}";
        }
    }

    public void PauseGame()
    {
        isCounting = false;
        Time.timeScale = 0f;
        pausePanel?.SetActive(true);
    }

    public void ResumeGame()
    {
        isCounting = true;
        Time.timeScale = 1f;
        pausePanel?.SetActive(false);
    }

    public void FinishLevel()
    {
        isCounting = false;
        if (currentLevelIndex >= 0 && currentLevelIndex < levelTimes.Length)
        {
            levelTimes[currentLevelIndex] = currentTime;
        }

        if (currentLevelIndex < 2)
        {
            currentLevelIndex++;
            SceneManager.LoadScene("Level" + (currentLevelIndex + 1));
        }
        else
        {
            // 完成三关后进入演出关
            SceneManager.LoadScene("Cutscene");
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene("EndScene");
    }

    public void ShowFinalResults(Text endText)
    {
        float total = 0f;
        string result = "Game Completed!\n\n";
        for (int i = 0; i < levelTimes.Length; i++)
        {
            result += $"Level {i + 1}: {FormatTime(levelTimes[i])}\n";
            total += levelTimes[i];
        }
        result += $"\nTotal Time: {FormatTime(total)}";
        endText.text = result;
        endPanel?.SetActive(true);
    }

    string FormatTime(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        return $"{m:00}:{s:00}";
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
