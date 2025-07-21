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
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    public void ShowLevelCompleteUI()
    {
        if (isShowing) return; // ��ֹ�ظ�����
        isShowing = true;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (missionCompleteText != null)
            missionCompleteText.text = "Mission Complete!";

        if (timeText != null && LevelTimer.instance != null)
        {
            // �ȱ��浱ǰ�ؿ�ʱ��
            LevelTimer.instance.SaveCurrentLevelTime();

            float levelTime = LevelTimer.instance.levelTimes[LevelTimer.instance.currentLevel];
            timeText.text = "Time: " + FormatTime(levelTime);
        }

        // ʹ��GameManager��ר����ͣ����
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseForLevelComplete();
        }
        else
        {
            // ���÷���
            Time.timeScale = 0f;
            if (LevelTimer.instance != null)
                LevelTimer.instance.PauseTimer();
        }
    }

    void OnNextLevelClicked()
    {
        if (!isShowing) return;

        isShowing = false;

        // �ָ���Ϸ״̬
        Time.timeScale = 1f;

        if (LevelTimer.instance != null)
        {
            LevelTimer.instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("LevelTimer instance not found!");
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // �����Ҫֱ�ӷ������˵�
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