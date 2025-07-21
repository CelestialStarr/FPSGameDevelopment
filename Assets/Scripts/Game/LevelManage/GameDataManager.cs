using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("Persistent Data")]
    // ���Ѫ������
    public static int persistentMaxHealth = 100;
    public static int persistentCurrentHealth = 100;

    // ������ҩ���� - ��Ӧ�������ϵͳ��0���ܲ�ǹ 1��ǹ 2����ǹ 3��
    public static int[] persistentAmmo = new int[4]; // 4�������ĵ�ҩ
    public static int[] persistentMaxAmmo = new int[4]; // 4�����������ҩ
    public static int persistentCurrentWeapon = 0;
    public static bool[] persistentWeaponUnlocked = new bool[4] { true, false, false, false }; // ��һ������Ĭ�Ͻ���

    // �ؿ�����
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

        // ��ʼ��Ĭ������
        InitializeDefaultData();
    }

    void InitializeDefaultData()
    {
        // ��ʼ����ҩ�������û�г�ʼ������
        if (persistentAmmo[0] == 0 && persistentAmmo[1] == 0 && persistentAmmo[2] == 0 && persistentAmmo[3] == 0)
        {
            // �����������ϵͳ����Ĭ��ֵ
            persistentAmmo[0] = 50;   // ���ܲ�ǹ��ʼ��ҩ
            persistentAmmo[1] = 0;    // ��ǹ
            persistentAmmo[2] = 0;    // ����ǹ
            persistentAmmo[3] = 1;    // ������ս������

            // �������ҩ
            persistentMaxAmmo[0] = 150; // ���ܲ�ǹ���ҩ
            persistentMaxAmmo[1] = 40;  // ��ǹ���ҩ
            persistentMaxAmmo[2] = 30;  // ����ǹ���ҩ
            persistentMaxAmmo[3] = 1;   // ������ս������
        }
    }

    // �������Ѫ��״̬
    public static void SaveHealthData(int currentHealth, int maxHealth)
    {
        persistentCurrentHealth = currentHealth;
        persistentMaxHealth = maxHealth;
        Debug.Log($"Health data saved: {currentHealth}/{maxHealth}");
    }

    // ��ȡ���Ѫ��״̬
    public static void GetHealthData(out int currentHealth, out int maxHealth)
    {
        currentHealth = persistentCurrentHealth;
        maxHealth = persistentMaxHealth;
    }

    // ����������ҩ״̬
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

    // ��ȡ������ҩ״̬
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

    // ������Ϸ���ݣ�����Ϸʱ���ã�
    public static void ResetGameData()
    {
        persistentMaxHealth = 100;
        persistentCurrentHealth = 100;

        persistentAmmo[0] = 50;  // ���ܲ�ǹ��ʼ��ҩ
        persistentAmmo[1] = 0;   // ��ǹ
        persistentAmmo[2] = 0;   // ����ǹ
        persistentAmmo[3] = 1;   // ��

        persistentMaxAmmo[0] = 150; // ���ܲ�ǹ���ҩ
        persistentMaxAmmo[1] = 40;  // ��ǹ���ҩ
        persistentMaxAmmo[2] = 30;  // ����ǹ���ҩ
        persistentMaxAmmo[3] = 1;   // ��

        persistentCurrentWeapon = 0;

        persistentWeaponUnlocked[0] = true;  // ��һ������Ĭ�Ͻ���
        persistentWeaponUnlocked[1] = false;
        persistentWeaponUnlocked[2] = false;
        persistentWeaponUnlocked[3] = false;

        highestLevelUnlocked = 1;

        Debug.Log("Game data reset to default values");
    }

    // ��������
    public static void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < persistentWeaponUnlocked.Length)
        {
            persistentWeaponUnlocked[weaponIndex] = true;
            Debug.Log($"Weapon {weaponIndex} unlocked!");
        }
    }

    // ��ӵ�ҩ
    public static void AddAmmo(int weaponIndex, int amount)
    {
        if (weaponIndex >= 0 && weaponIndex < persistentAmmo.Length)
        {
            persistentAmmo[weaponIndex] += amount;
            Debug.Log($"Added {amount} ammo to weapon {weaponIndex}. Total: {persistentAmmo[weaponIndex]}");
        }
    }

    // �����ؿ�
    public static void UnlockLevel(int levelNumber)
    {
        if (levelNumber > highestLevelUnlocked)
        {
            highestLevelUnlocked = levelNumber;
            Debug.Log($"Level {levelNumber} unlocked!");
        }
    }

    // ���ؿ��Ƿ����
    public static bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= highestLevelUnlocked;
    }
}