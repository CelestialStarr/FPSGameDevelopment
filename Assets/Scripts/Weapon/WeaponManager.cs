using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject[] weapons; // ��˳��0���ܲ�ǹ 1��ǹ 2����ǹ 3��

    private int currentWeaponIndex = 0; // Ĭ�ϴӺ��ܲ�ǹ��ʼ

    void Start()
    {
        // ��ʼ��ʱֻ�����һ�����������ܲ�ǹ��
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == currentWeaponIndex);
        }

        // ����UI
        UpdateCurrentWeaponUI();
    }

    void Update()
    {
        // ������ּ��л�����
        for (int i = 0; i < weapons.Length && i < 4; i++) // ���4������
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
            }
        }

        // �������л�����
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
}