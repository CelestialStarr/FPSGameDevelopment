using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public static LevelComplete Instance;

    public GameObject levelCompletePanel;
    public Text missionCompleteText;
    public Text timeText;
    public Button nextLevelButton;

    private bool isShowing = false;

    void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 可选：如果需要跨场景保持，取消注释下一行
         //DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }
    }

    void Update()
    {
        // 按L键触发结算界面
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShowLevelCompleteUI();
        }
    }

    public void ShowLevelCompleteUI()
    {
        if (isShowing) return;

        isShowing = true;

        // 先停止计时器并保存时间
        if (LevelTimer.instance != null)
        {
            LevelTimer.instance.StopTimer();
            LevelTimer.instance.SaveCurrentLevelTime();
        }

        // 显示UI
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (missionCompleteText != null)
            missionCompleteText.text = "Mission Accomplished";

        // 显示时间
        UpdateTimeDisplay();

        // 确保按钮可交互
        if (nextLevelButton != null)
            nextLevelButton.interactable = true;

        // 设置游戏状态
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null || LevelTimer.instance == null)
        {
            if (timeText != null)
                timeText.text = "Time: --:--";
            return;
        }

        // 获取当前关卡时间
        float levelTime = 0f;
        int currentLevel = LevelTimer.instance.currentLevel;

        if (currentLevel >= 0 && currentLevel < LevelTimer.instance.levelTimes.Length)
        {
            levelTime = LevelTimer.instance.levelTimes[currentLevel];
        }

        // 如果保存的时间为0，使用当前计时
        if (levelTime <= 0f)
        {
            levelTime = LevelTimer.instance.GetCurrentLevelTime();
        }

        timeText.text = "Time: " + FormatTime(levelTime);
    }

    public void OnNextLevelClicked()
    {
        if (!isShowing) return;

        isShowing = false;
        Time.timeScale = 1f;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        // 直接用GameManager，删除LevelTimer的调用
        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log($"当前场景: {currentScene}"); // 调试用

        if (currentScene == "Level1")
        {
            Debug.Log("跳转到Level2");
            GameManager.Instance.LoadLevel2();
        }
        else if (currentScene == "Level2")
        {
            Debug.Log("跳转到Level3");
            GameManager.Instance.LoadLevel3();
        }
        else if (currentScene == "Level3")
        {
            Debug.Log("跳转到CutScene");
            GameManager.Instance.LoadCutScene();
        }
        else
        {
            Debug.LogError($"未知场景: {currentScene}");
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}