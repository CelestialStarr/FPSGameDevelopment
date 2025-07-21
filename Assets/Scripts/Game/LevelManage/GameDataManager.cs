using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("Persistent Data")]
    // 玩家血量数据
    public static int persistentMaxHealth = 100;
    public static int persistentCurrentHealth = 100;

    // 武器弹药数据 - 对应你的武器系统：0胡萝卜枪 1肉枪 2胡椒枪 3刀
    public static int[] persistentAmmo = new int[4]; // 4种武器的弹药
    public static int[] persistentMaxAmmo = new int[4]; // 4种武器的最大弹药
    public static int persistentCurrentWeapon = 0;
    public static bool[] persistentWeaponUnlocked = new bool[4] { true, false, false, false }; // 第一把武器默认解锁

    // 关卡进度
    public static int highestLevelUnlocked = 1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化默认数据
        InitializeDefaultData();
    }

    void InitializeDefaultData()
    {
        // 初始化弹药（如果还没有初始化过）
        if (persistentAmmo[0] == 0 && persistentAmmo[1] == 0 && persistentAmmo[2] == 0 && persistentAmmo[3] == 0)
        {
            // 根据你的武器系统设置默认值
            persistentAmmo[0] = 50;   // 胡萝卜枪初始弹药
            persistentAmmo[1] = 0;    // 肉枪
            persistentAmmo[2] = 0;    // 胡椒枪
            persistentAmmo[3] = 1;    // 刀（近战武器）

            // 设置最大弹药
            persistentMaxAmmo[0] = 150; // 胡萝卜枪最大弹药
            persistentMaxAmmo[1] = 40;  // 肉枪最大弹药
            persistentMaxAmmo[2] = 30;  // 胡椒枪最大弹药
            persistentMaxAmmo[3] = 1;   // 刀（近战武器）
        }
    }

    // 保存玩家血量状态
    public static void SaveHealthData(int currentHealth, int maxHealth)
    {
        persistentCurrentHealth = currentHealth;
        persistentMaxHealth = maxHealth;
        Debug.Log($"Health data saved: {currentHealth}/{maxHealth}");
    }

    // 获取玩家血量状态
    public static void GetHealthData(out int currentHealth, out int maxHealth)
    {
        currentHealth = persistentCurrentHealth;
        maxHealth = persistentMaxHealth;
    }

    // 保存武器弹药状态
    public static void SaveWeaponData(int[] ammo, int[] maxAmmo, int currentWeapon, bool[] weaponUnlocked)
    {
        for (int i = 0; i < persistentAmmo.Length && i < ammo.Length; i++)
        {
            persistentAmmo[i] = ammo[i];
        }

        for (int i = 0; i < persistentMaxAmmo.Length && i < maxAmmo.Length; i++)
        {
            persistentMaxAmmo[i] = maxAmmo[i];
        }

        persistentCurrentWeapon = currentWeapon;

        for (int i = 0; i < persistentWeaponUnlocked.Length && i < weaponUnlocked.Length; i++)
        {
            persistentWeaponUnlocked[i] = weaponUnlocked[i];
        }

        Debug.Log($"Weapon data saved: Ammo[{string.Join(",", ammo)}], Current: {currentWeapon}");
    }

    // 获取武器弹药状态
    public static void GetWeaponData(out int[] ammo, out int[] maxAmmo, out int currentWeapon, out bool[] weaponUnlocked)
    {
        ammo = new int[persistentAmmo.Length];
        for (int i = 0; i < persistentAmmo.Length; i++)
        {
            ammo[i] = persistentAmmo[i];
        }

        maxAmmo = new int[persistentMaxAmmo.Length];
        for (int i = 0; i < persistentMaxAmmo.Length; i++)
        {
            maxAmmo[i] = persistentMaxAmmo[i];
        }

        currentWeapon = persistentCurrentWeapon;

        weaponUnlocked = new bool[persistentWeaponUnlocked.Length];
        for (int i = 0; i < persistentWeaponUnlocked.Length; i++)
        {
            weaponUnlocked[i] = persistentWeaponUnlocked[i];
        }
    }

    // 重置游戏数据（新游戏时调用）
    public static void ResetGameData()
    {
        persistentMaxHealth = 100;
        persistentCurrentHealth = 100;

        persistentAmmo[0] = 50;  // 胡萝卜枪初始弹药
        persistentAmmo[1] = 0;   // 肉枪
        persistentAmmo[2] = 0;   // 胡椒枪
        persistentAmmo[3] = 1;   // 刀

        persistentMaxAmmo[0] = 150; // 胡萝卜枪最大弹药
        persistentMaxAmmo[1] = 40;  // 肉枪最大弹药
        persistentMaxAmmo[2] = 30;  // 胡椒枪最大弹药
        persistentMaxAmmo[3] = 1;   // 刀

        persistentCurrentWeapon = 0;

        persistentWeaponUnlocked[0] = true;  // 第一把武器默认解锁
        persistentWeaponUnlocked[1] = false;
        persistentWeaponUnlocked[2] = false;
        persistentWeaponUnlocked[3] = false;

        highestLevelUnlocked = 1;

        Debug.Log("Game data reset to default values");
    }

    // 解锁武器
    public static void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < persistentWeaponUnlocked.Length)
        {
            persistentWeaponUnlocked[weaponIndex] = true;
            Debug.Log($"Weapon {weaponIndex} unlocked!");
        }
    }

    // 添加弹药
    public static void AddAmmo(int weaponIndex, int amount)
    {
        if (weaponIndex >= 0 && weaponIndex < persistentAmmo.Length)
        {
            persistentAmmo[weaponIndex] += amount;
            Debug.Log($"Added {amount} ammo to weapon {weaponIndex}. Total: {persistentAmmo[weaponIndex]}");
        }
    }

    // 解锁关卡
    public static void UnlockLevel(int levelNumber)
    {
        if (levelNumber > highestLevelUnlocked)
        {
            highestLevelUnlocked = levelNumber;
            Debug.Log($"Level {levelNumber} unlocked!");
        }
    }

    // 检查关卡是否解锁
    public static bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= highestLevelUnlocked;
    }
}