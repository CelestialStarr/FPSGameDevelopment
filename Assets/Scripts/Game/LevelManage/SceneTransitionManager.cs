using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ע�᳡���л��¼�
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene scene)
    {
        // �ڳ���ж��ʱ������������
        SaveAllGameData();
        Debug.Log($"Scene {scene.name} unloaded, data saved");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded");

        // ���³������غ�ָ�����
        if (scene.name.StartsWith("Level"))
        {
            // �ӳٻָ����ݣ�ȷ������������ѳ�ʼ��
            Invoke("RestoreGameData", 0.5f);
        }
    }

    void SaveAllGameData()
    {
        // ����Ѫ������
        if (PlayerHealthController.instance != null)
        {
            GameDataManager.SaveHealthData(
                PlayerHealthController.instance.GetCurrentHealth(),
                PlayerHealthController.instance.maxHealth
            );
        }

        // ������������
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.SaveCurrentWeaponData();
        }

        Debug.Log("All game data saved before scene transition");
    }

    void RestoreGameData()
    {
        // �ָ�Ѫ������
        GameDataManager.GetHealthData(out int currentHealth, out int maxHealth);
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.SetHealth(currentHealth, maxHealth);
        }

        // �������ݻ��� WeaponManager �� Start() �������Զ�����
        Debug.Log("Game data restored in new scene");
    }

    // �ֶ��������ݣ����Դ������ű����ã�
    public void ManualSave()
    {
        SaveAllGameData();
    }

    // �ؿ����ʱ����
    public void OnLevelCompleted()
    {
        SaveAllGameData();

        // ������һ��
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level1")
        {
            GameDataManager.UnlockLevel(2);
        }
        else if (currentScene == "Level2")
        {
            GameDataManager.UnlockLevel(3);
        }

        // ֪ͨ�����������ؿ����
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.OnLevelComplete();
        }

        Debug.Log("Level completed, data saved and next level unlocked");
    }

    // �������ʱ���ã����õ��ؿ���ʼ״̬�������ֽ��ȣ�
    public void OnPlayerDeath()
    {
        // ����ʱ�����浱ǰ״̬������Ҵӹؿ���ʼ��״̬����
        Debug.Log("Player died, maintaining level start state");
    }

    // ����Ϸ��ʼʱ����
    public void StartNewGame()
    {
        GameDataManager.ResetGameData();
        Debug.Log("New game started, all data reset");
    }

    void OnDestroy()
    {
        // �����¼�����
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllGameData();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAllGameData();
        }
    }
}