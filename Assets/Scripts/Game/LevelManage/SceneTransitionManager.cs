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

        // 注册场景切换事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene scene)
    {
        // 在场景卸载时保存所有数据
        SaveAllGameData();
        Debug.Log($"Scene {scene.name} unloaded, data saved");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded");

        // 在新场景加载后恢复数据
        if (scene.name.StartsWith("Level"))
        {
            // 延迟恢复数据，确保所有组件都已初始化
            Invoke("RestoreGameData", 0.5f);
        }
    }

    void SaveAllGameData()
    {
        // 保存血量数据
        if (PlayerHealthController.instance != null)
        {
            GameDataManager.SaveHealthData(
                PlayerHealthController.instance.GetCurrentHealth(),
                PlayerHealthController.instance.maxHealth
            );
        }

        // 保存武器数据
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.SaveCurrentWeaponData();
        }

        Debug.Log("All game data saved before scene transition");
    }

    void RestoreGameData()
    {
        // 恢复血量数据
        GameDataManager.GetHealthData(out int currentHealth, out int maxHealth);
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.SetHealth(currentHealth, maxHealth);
        }

        // 武器数据会在 WeaponManager 的 Start() 方法中自动加载
        Debug.Log("Game data restored in new scene");
    }

    // 手动保存数据（可以从其他脚本调用）
    public void ManualSave()
    {
        SaveAllGameData();
    }

    // 关卡完成时调用
    public void OnLevelCompleted()
    {
        SaveAllGameData();

        // 解锁下一关
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level1")
        {
            GameDataManager.UnlockLevel(2);
        }
        else if (currentScene == "Level2")
        {
            GameDataManager.UnlockLevel(3);
        }

        // 通知武器管理器关卡完成
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.OnLevelComplete();
        }

        Debug.Log("Level completed, data saved and next level unlocked");
    }

    // 玩家死亡时调用（重置到关卡开始状态，但保持进度）
    public void OnPlayerDeath()
    {
        // 死亡时不保存当前状态，让玩家从关卡开始的状态重生
        Debug.Log("Player died, maintaining level start state");
    }

    // 新游戏开始时调用
    public void StartNewGame()
    {
        GameDataManager.ResetGameData();
        Debug.Log("New game started, all data reset");
    }

    void OnDestroy()
    {
        // 清理事件订阅
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