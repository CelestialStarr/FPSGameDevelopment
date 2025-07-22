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

    // ɾ��UI���ã���Ϊͨ��UIController����
    // public GameObject pausePanel;
    // public Slider volumeSlider;

    void Awake()
    {
        // ����ģʽ��ȷ��Ψһ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ע�᳡�������¼�
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
            // �ǹؿ�������ȷ������ͣ
            ForceResumeGame();
        }
        else
        {
            // �ǹؿ����ָ�״̬
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // ͬ����ͣ��ʱ��
        if (LevelTimer.instance != null)
            LevelTimer.instance.PauseTimer();

        // ֪ͨUIController��ʾ��ͣUI
        if (UIController.Instance != null)
        {
            UIController.Instance.ShowPauseUI();
        }

        // ���ù��״̬
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

        // ͬ���ָ���ʱ��
        if (LevelTimer.instance != null)
            LevelTimer.instance.ResumeTimer();

        // ֪ͨUIController������ͣUI
        if (UIController.Instance != null)
        {
            UIController.Instance.HidePauseUI();
        }

        // ���ù��״̬
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ǿ�ƻָ���Ϸ״̬�����ڷ���Ϸ������
    private void ForceResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // ����Ϸ�����Ĺ��״̬
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ��Ϸ���ʱ��ͣ����ͬ��ESC��ͣ��
    public void PauseForLevelComplete()
    {
        Time.timeScale = 0f;

        // ֹͣ��ʱ����������ͣ״̬
        if (LevelTimer.instance != null)
            LevelTimer.instance.PauseTimer();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnToMainMenu()
    {
        ForceResumeGame(); // ȷ��״̬����
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
        // �����¼�����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // �������ԣ��������ű���ѯ״̬
    public bool IsPaused => isPaused;
}