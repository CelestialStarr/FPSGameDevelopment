using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject[] weapons; // ��˳��0���ܲ�ǹ 1��ǹ 2����ǹ 3��

    private int currentWeaponIndex = 0; // Ĭ�ϴӺ��ܲ�ǹ��ʼ
    private bool[] weaponUnlocked = new bool[4] { true, false, false, false }; // Ĭ��ֻ�е�һ����������

    void Start()
    {
        // �ӳټ������ݣ�ȷ������������ѳ�ʼ��
        Invoke("LoadWeaponData", 0.5f);
    }

    void LoadWeaponData()
    {
        // �� GameDataManager ������������
        GameDataManager.GetWeaponData(out int[] savedAmmo, out int[] savedMaxAmmo, out int savedCurrentWeapon, out bool[] savedWeaponUnlocked);

        currentWeaponIndex = savedCurrentWeapon;
        weaponUnlocked = savedWeaponUnlocked;

        // Ӧ�õ�ҩ���ݵ�����
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

        // ��ʼ��������ʾ
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == currentWeaponIndex);
        }

        // ����UI
        UpdateCurrentWeaponUI();

        Debug.Log($"Weapon data loaded: Current weapon = {currentWeaponIndex}");
    }

    void Update()
    {
        // ������ּ��л�����
        for (int i = 0; i < weapons.Length && i < 4; i++) // ���4������
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (weaponUnlocked[i]) // ֻ���л����ѽ���������
                {
                    SwitchWeapon(i);
                }
                else
                {
                    Debug.Log($"Weapon {i} is locked!");
                }
            }
        }

        // �������л�����
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
            // ���浱ǰ��������
            SaveCurrentWeaponData();

            // ���õ�ǰ����
            weapons[currentWeaponIndex].SetActive(false);

            // �л���������
            currentWeaponIndex = newWeaponIndex;
            weapons[currentWeaponIndex].SetActive(true);

            // ����UI
            UpdateCurrentWeaponUI();

            Debug.Log($"Switched to weapon {currentWeaponIndex}: {weapons[currentWeaponIndex].name}");
        }
    }

    // ��������
    public void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weaponUnlocked.Length)
        {
            weaponUnlocked[weaponIndex] = true;
            GameDataManager.UnlockWeapon(weaponIndex);
            Debug.Log($"Weapon {weaponIndex} unlocked!");
        }
    }

    // ��ȡ��ǰ��������
    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    // ��������Ƿ����
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
            // ���Ի�ȡGun���
            Gun gun = weapons[currentWeaponIndex].GetComponent<Gun>();
            if (gun != null)
            {
                gun.UpdateUI();
                return;
            }

            // ���Ի�ȡKnife���
            Knife knife = weapons[currentWeaponIndex].GetComponent<Knife>();
            if (knife != null)
            {
                knife.UpdateUI();
                return;
            }
        }
    }

    // ���浱ǰ��������
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
                    // ���ڵ��Ƚ�ս����������Ĭ��ֵ
                    currentAmmo[i] = 1;
                    maxAmmo[i] = 1;
                }
            }
        }

        GameDataManager.SaveWeaponData(currentAmmo, maxAmmo, currentWeaponIndex, weaponUnlocked);
    }

    // ��ӵ�ҩ��ָ������
    public void AddAmmoToWeapon(int weaponIndex, int amount)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Length && weapons[weaponIndex] != null)
        {
            Gun gun = weapons[weaponIndex].GetComponent<Gun>();
            if (gun != null)
            {
                gun.GetAmmo();

                // ����ǵ�ǰ����������UI
                if (weaponIndex == currentWeaponIndex)
                {
                    gun.UpdateUI();
                }

                // ͬʱ���� GameDataManager
                GameDataManager.AddAmmo(weaponIndex, amount);
            }
        }
    }

    // �ڳ����л�����Ϸ����ʱ��������
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

    // �ڹؿ����ʱ����
    public void OnLevelComplete()
    {
        SaveCurrentWeaponData();

        // ������һ��������������ã�
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