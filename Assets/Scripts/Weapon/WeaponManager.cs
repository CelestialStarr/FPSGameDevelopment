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
            weapons[i].SetActive(i == currentWeaponIndex);
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

        // 鼠标滚轮切换武器
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0f)
        {
            SwitchWeapon((currentWeaponIndex + 1) % weapons.Length);
        }
        else if (scrollInput < 0f)
        {
            SwitchWeapon((currentWeaponIndex - 1 + weapons.Length) % weapons.Length);
        }
    }

    public void SwitchWeapon(int newWeaponIndex)
    {
        if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Length && newWeaponIndex != currentWeaponIndex)
        {
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
}