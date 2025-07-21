using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public GameObject levelCompletePanel;
    public Text missionCompleteText;
    public Text timeText;
    public Button nextLevelButton;

    private bool isShowing = false;

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
        // ��L�������������
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShowLevelCompleteUI();
        }
    }

    public void ShowLevelCompleteUI()
    {
        if (isShowing) return;

        isShowing = true;

        // ��ֹͣ��ʱ��������ʱ��
        if (LevelTimer.instance != null)
        {
            LevelTimer.instance.StopTimer();
            LevelTimer.instance.SaveCurrentLevelTime();
        }

        // ��ʾUI
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (missionCompleteText != null)
            missionCompleteText.text = "Mission Accomplished";

        // ��ʾʱ��
        UpdateTimeDisplay();

        // ȷ����ť�ɽ���
        if (nextLevelButton != null)
            nextLevelButton.interactable = true;

        // ������Ϸ״̬
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

        // ��ȡ��ǰ�ؿ�ʱ��
        float levelTime = 0f;
        int currentLevel = LevelTimer.instance.currentLevel;

        if (currentLevel >= 0 && currentLevel < LevelTimer.instance.levelTimes.Length)
        {
            levelTime = LevelTimer.instance.levelTimes[currentLevel];
        }

        // ��������ʱ��Ϊ0��ʹ�õ�ǰ��ʱ
        if (levelTime <= 0f)
        {
            levelTime = LevelTimer.instance.GetCurrentLevelTime();
        }

        timeText.text = "Time: " + FormatTime(levelTime);
    }

    void OnNextLevelClicked()
    {
        if (!isShowing) return;

        isShowing = false;
        Time.timeScale = 1f;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        // ������һ��
        if (LevelTimer.instance != null)
        {
            LevelTimer.instance.LoadNextLevel();
        }
        else
        {
            LoadNextLevelDirectly();
        }
    }

    private void LoadNextLevelDirectly()
    {
        string currentScene = SceneManager.GetActiveScene().name.ToLower();

        if (currentScene.Contains("level1"))
        {
            SceneManager.LoadScene("Level2");
        }
        else if (currentScene.Contains("level2"))
        {
            SceneManager.LoadScene("Level3");
        }
        else if (currentScene.Contains("level3"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LoadGameOver();
            else
                SceneManager.LoadScene("GameOver");
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