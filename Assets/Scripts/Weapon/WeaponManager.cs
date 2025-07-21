using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject[] weapons; // 按顺序：0胡萝卜枪 1肉枪 2胡椒枪 3刀

    private int currentWeaponIndex = 0; // 默认从胡萝卜枪开始
    private bool[] weaponUnlocked = new bool[4] { true, false, false, false }; // 默认只有第一把武器解锁

    void Start()
    {
        // 延迟加载数据，确保所有组件都已初始化
        Invoke("LoadWeaponData", 0.5f);
    }

    void LoadWeaponData()
    {
        // 从 GameDataManager 加载武器数据
        GameDataManager.GetWeaponData(out int[] savedAmmo, out int[] savedMaxAmmo, out int savedCurrentWeapon, out bool[] savedWeaponUnlocked);

        currentWeaponIndex = savedCurrentWeapon;
        weaponUnlocked = savedWeaponUnlocked;

        // 应用弹药数据到武器
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                Gun gun = weapons[i].GetComponent<Gun>();
                if (gun != null && i < savedAmmo.Length)
                {
                    gun.currentAmmo = savedAmmo[i];
                    gun.maxAmmo = savedMaxAmmo[i];
                    Debug.Log($"Loaded ammo for {gun.weaponName}: {gun.currentAmmo}/{gun.maxAmmo}");
                }
            }
        }

        // 初始化武器显示
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == currentWeaponIndex);
        }

        // 更新UI
        UpdateCurrentWeaponUI();

        Debug.Log($"Weapon data loaded: Current weapon = {currentWeaponIndex}");
    }

    void Update()
    {
        // 检测数字键切换武器
        for (int i = 0; i < weapons.Length && i < 4; i++) // 最多4个武器
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (weaponUnlocked[i]) // 只能切换到已解锁的武器
                {
                    SwitchWeapon(i);
                }
                else
                {
                    Debug.Log($"Weapon {i} is locked!");
                }
            }
        }

        // 鼠标滚轮切换武器
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0f)
        {
            SwitchToNextUnlockedWeapon(1);
        }
        else if (scrollInput < 0f)
        {
            SwitchToNextUnlockedWeapon(-1);
        }
    }

    private void SwitchToNextUnlockedWeapon(int direction)
    {
        int attempts = 0;
        int newIndex = currentWeaponIndex;

        do
        {
            newIndex = (newIndex + direction + weapons.Length) % weapons.Length;
            attempts++;
        } while (!weaponUnlocked[newIndex] && attempts < weapons.Length);

        if (weaponUnlocked[newIndex] && newIndex != currentWeaponIndex)
        {
            SwitchWeapon(newIndex);
        }
    }

    public void SwitchWeapon(int newWeaponIndex)
    {
        if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Length &&
            newWeaponIndex != currentWeaponIndex && weaponUnlocked[newWeaponIndex])
        {
            // 保存当前武器数据
            SaveCurrentWeaponData();

            // 禁用当前武器
            weapons[currentWeaponIndex].SetActive(false);

            // 切换到新武器
            currentWeaponIndex = newWeaponIndex;
            weapons[currentWeaponIndex].SetActive(true);

            // 更新UI
            UpdateCurrentWeaponUI();

            Debug.Log($"Switched to weapon {currentWeaponIndex}: {weapons[currentWeaponIndex].name}");
        }
    }

    // 解锁武器
    public void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weaponUnlocked.Length)
        {
            weaponUnlocked[weaponIndex] = true;
            GameDataManager.UnlockWeapon(weaponIndex);
            Debug.Log($"Weapon {weaponIndex} unlocked!");
        }
    }

    // 获取当前武器索引
    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    // 检查武器是否解锁
    public bool IsWeaponUnlocked(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weaponUnlocked.Length)
        {
            return weaponUnlocked[weaponIndex];
        }
        return false;
    }

    private void UpdateCurrentWeaponUI()
    {
        if (weapons[currentWeaponIndex] != null)
        {
            // 尝试获取Gun组件
            Gun gun = weapons[currentWeaponIndex].GetComponent<Gun>();
            if (gun != null)
            {
                gun.UpdateUI();
                return;
            }

            // 尝试获取Knife组件
            Knife knife = weapons[currentWeaponIndex].GetComponent<Knife>();
            if (knife != null)
            {
                knife.UpdateUI();
                return;
            }
        }
    }

    // 保存当前武器数据
    public void SaveCurrentWeaponData()
    {
        int[] currentAmmo = new int[weapons.Length];
        int[] maxAmmo = new int[weapons.Length];

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                Gun gun = weapons[i].GetComponent<Gun>();
                if (gun != null)
                {
                    currentAmmo[i] = gun.currentAmmo;
                    maxAmmo[i] = gun.maxAmmo;
                }
                else
                {
                    // 对于刀等近战武器，设置默认值
                    currentAmmo[i] = 1;
                    maxAmmo[i] = 1;
                }
            }
        }

        GameDataManager.SaveWeaponData(currentAmmo, maxAmmo, currentWeaponIndex, weaponUnlocked);
    }

    // 添加弹药到指定武器
    public void AddAmmoToWeapon(int weaponIndex, int amount)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Length && weapons[weaponIndex] != null)
        {
            Gun gun = weapons[weaponIndex].GetComponent<Gun>();
            if (gun != null)
            {
                gun.GetAmmo();

                // 如果是当前武器，更新UI
                if (weaponIndex == currentWeaponIndex)
                {
                    gun.UpdateUI();
                }

                // 同时更新 GameDataManager
                GameDataManager.AddAmmo(weaponIndex, amount);
            }
        }
    }

    // 在场景切换或游戏结束时保存数据
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentWeaponData();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveCurrentWeaponData();
        }
    }

    void OnDestroy()
    {
        SaveCurrentWeaponData();
    }

    // 在关卡完成时调用
    public void OnLevelComplete()
    {
        SaveCurrentWeaponData();

        // 解锁下一个武器（如果适用）
        int nextWeaponToUnlock = -1;
        for (int i = 0; i < weaponUnlocked.Length; i++)
        {
            if (!weaponUnlocked[i])
            {
                nextWeaponToUnlock = i;
                break;
            }
        }

        if (nextWeaponToUnlock != -1)
        {
            UnlockWeapon(nextWeaponToUnlock);
        }
    }
}