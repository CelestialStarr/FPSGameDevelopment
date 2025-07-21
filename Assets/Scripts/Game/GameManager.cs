using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject pausePanel;
    public Slider volumeSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private bool isPaused = false;

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
        if (volumeSlider != null)
            SetVolume(volumeSlider.value);
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
        Debug.Log($"Scene loaded: {scene.name}");

        // ���²���UI���ã������л�����ܶ�ʧ��
        RefreshUIReferences();

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

    private void RefreshUIReferences()
    {
        // ���UI���ö�ʧ�����²���
        if (pausePanel == null)
        {
            GameObject pausePanelObj = GameObject.Find("PausePanel");
            if (pausePanelObj != null)
                pausePanel = pausePanelObj;
        }

        if (volumeSlider == null)
        {
            Slider[] sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
            foreach (var slider in sliders)
            {
                if (slider.name.Contains("Volume"))
                {
                    volumeSlider = slider;
                    break;
                }
            }
        }
    }

    public void PauseGame()
    {
        if (pausePanel == null) return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        // ͬ����ͣ��ʱ��
        if (LevelTimer.instance != null)
            LevelTimer.instance.PauseTimer();

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

        if (pausePanel == null) return;

        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        // ͬ���ָ���ʱ��
        if (LevelTimer.instance != null)
            LevelTimer.instance.ResumeTimer();

        // ���ù��״̬
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ǿ�ƻָ���Ϸ״̬�����ڷ���Ϸ������
    private void ForceResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

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
        Debug.Log($"Loading Level{levelNumber}");
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