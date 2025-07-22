using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private bool isPaused = false;

    // 删除UI引用，改为通过UIController管理
    // public GameObject pausePanel;
    // public Slider volumeSlider;

    void Awake()
    {
        // 单例模式，确保唯一
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 注册场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        ResumeGame();
    }

    void Update()
    {
        if (!IsInGameLevel()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private bool IsInGameLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName.StartsWith("Level");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.StartsWith("Level"))
        {
            // 非关卡场景，确保不暂停
            ForceResumeGame();
        }
        else
        {
            // 是关卡，恢复状态
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // 同步暂停计时器
        if (LevelTimer.instance != null)
            LevelTimer.instance.PauseTimer();

        // 通知UIController显示暂停UI
        if (UIController.Instance != null)
        {
            UIController.Instance.ShowPauseUI();
        }

        // 设置光标状态
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!IsInGameLevel())
        {
            ForceResumeGame();
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;

        // 同步恢复计时器
        if (LevelTimer.instance != null)
            LevelTimer.instance.ResumeTimer();

        // 通知UIController隐藏暂停UI
        if (UIController.Instance != null)
        {
            UIController.Instance.HidePauseUI();
        }

        // 设置光标状态
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 强制恢复游戏状态（用于非游戏场景）
    private void ForceResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // 非游戏场景的光标状态
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 游戏完成时暂停（不同于ESC暂停）
    public void PauseForLevelComplete()
    {
        Time.timeScale = 0f;

        // 停止计时但不设置暂停状态
        if (LevelTimer.instance != null)
            LevelTimer.instance.PauseTimer();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnToMainMenu()
    {
        ForceResumeGame(); // 确保状态正常
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game.");
        Application.Quit();
    }

    public void SetVolume(float value)
    {
        if (audioMixer == null) return;

        if (value <= 0.0001f)
            audioMixer.SetFloat("MasterVolume", -80f);
        else
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20f);
    }

    public void LoadLevel(int levelNumber)
    {
        ForceResumeGame();
        SceneManager.LoadScene($"Level{levelNumber}");
    }

    public void LoadLevel2()
    {
        LoadLevel(2);
    }

    public void LoadLevel3()
    {
        LoadLevel(3);
    }

    public void LoadCutscene()
    {
        ForceResumeGame();
        SceneManager.LoadScene("Cutscene");
    }

    public void LoadGameOver()
    {
        ForceResumeGame();
        SceneManager.LoadScene("GameOver");
    }

    void OnDestroy()
    {
        // 清理事件订阅
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 公共属性，供其他脚本查询状态
    public bool IsPaused => isPaused;
}