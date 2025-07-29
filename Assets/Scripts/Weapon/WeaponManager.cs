using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject[] weapons; // 按顺序：0胡萝卜枪 1肉枪 2胡椒枪 3刀
    private int currentWeaponIndex = 0; // 默认从胡萝卜枪开始

    void Start()
    {
        // 初始化时只激活第一个武器（胡萝卜枪）
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null) // 添加空值检查
            {
                weapons[i].SetActive(i == currentWeaponIndex);
            }
        }
        // 更新UI
        UpdateCurrentWeaponUI();
    }

    void Update()
    {
        // 检测数字键切换武器
        for (int i = 0; i < weapons.Length && i < 4; i++) // 最多4个武器
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
            }
        }
    }

    public void SwitchWeapon(int newWeaponIndex)
    {
        if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Length && newWeaponIndex != currentWeaponIndex)
        {
            // 添加空值检查 - 禁用当前武器
            if (weapons[currentWeaponIndex] != null)
            {
                weapons[currentWeaponIndex].SetActive(false);
            }

            // 切换到新武器
            currentWeaponIndex = newWeaponIndex;

            // 添加空值检查 - 启用新武器
            if (weapons[currentWeaponIndex] != null)
            {
                weapons[currentWeaponIndex].SetActive(true);
                Debug.Log($"Switched to weapon {currentWeaponIndex}: {weapons[currentWeaponIndex].name}");
            }
            else
            {
                Debug.LogWarning($"武器 {currentWeaponIndex} 为空！");
            }

            // 更新UI
            UpdateCurrentWeaponUI();
        }
    }

    // 获取当前武器索引
    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    private void UpdateCurrentWeaponUI()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Length && weapons[currentWeaponIndex] != null)
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
        else
        {
            Debug.LogWarning("当前武器为空，无法更新UI");
        }
    }
}