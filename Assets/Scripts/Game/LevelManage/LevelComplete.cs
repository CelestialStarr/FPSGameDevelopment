using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelComplete: MonoBehaviour
{
    public GameObject levelCompletePanel;
    public Text missionCompleteText;
    public Text timeText;
    public Button nextLevelButton;

    void Start()
    {
        levelCompletePanel.SetActive(false);
        nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    public void ShowLevelCompleteUI()
    {
        levelCompletePanel.SetActive(true);

        if (missionCompleteText != null)
            missionCompleteText.text = "Mission Complete!";

        if (timeText != null)
        {
            float levelTime = LevelTimer.instance.levelTimes[LevelTimer.instance.currentLevel];
            timeText.text = "Time: " + FormatTime(levelTime);
        }

        Time.timeScale = 0f; // ‘›Õ£”Œœ∑
    }

    void OnNextLevelClicked()
    {
        Time.timeScale = 1f; // ª÷∏¥ ±º‰
        LevelTimer.instance.LoadNextLevel();
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
