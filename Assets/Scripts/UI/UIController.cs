using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Health UI")]
    public Slider healthSlider;          // Ѫ��������
    public Text healthText;              // Ѫ��������ʾ

    [Header("Pick up UI")]
    public GameObject pickupHintUI;      // ��ʾUI������
    public Text pickupHintText;          // ��ʾ��������

    [Header("Weapon UI")]
    public Text ammoText;                // �ӵ�������ʾ
    public Image[] weaponIcons;          // 4������ͼ�꣨��������

    [Header("Weapon Sprites")]
    public Sprite knifeSprite;           // ��ͼ��
    public Sprite carrotSprite;          // ���ܲ�ͼ�� 
    public Sprite meatSprite;            // ��ĩͼ�� 
    public Sprite pepperSprite;          // ����ͼ�� 

    // ������������
    public const int CARROT_INDEX = 0;
    public const int MEAT_INDEX = 1;
    public const int PEPPER_INDEX = 2;
    public const int KNIFE_INDEX = 3;

    private void Awake()
    {
        // ������ֵ
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // ��ʼ������ͼ��
        if (weaponIcons != null && weaponIcons.Length >= 4)
        {
            // ��������ͼ���Ĭ����ɫ - ��˳��0���ܲ� 1�� 2���� 3��
            if (carrotSprite != null)
                weaponIcons[CARROT_INDEX].sprite = carrotSprite;
            else
                weaponIcons[CARROT_INDEX].color = new Color(1f, 0.5f, 0f); // ��ɫ

            if (meatSprite != null)
                weaponIcons[MEAT_INDEX].sprite = meatSprite;
            else
                weaponIcons[MEAT_INDEX].color = new Color(0.6f, 0.3f, 0.1f); // ��ɫ

            if (pepperSprite != null)
                weaponIcons[PEPPER_INDEX].sprite = pepperSprite;
            else
                weaponIcons[PEPPER_INDEX].color = Color.red; // ��ɫ

            if (knifeSprite != null)
                weaponIcons[KNIFE_INDEX].sprite = knifeSprite;
            else
                weaponIcons[KNIFE_INDEX].color = Color.gray; // ��Ϊ��ɫ
        }

        // ������Ϣ����ӡ��ʼ��״̬
        Debug.Log("=== UIController��ʼ����� ===");
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            if (weaponIcons[i] != null)
            {
                Debug.Log($"����ͼ�� {i}: {weaponIcons[i].name}, sprite: {weaponIcons[i].sprite?.name ?? "null"}");
            }
        }
    }

    // ����������ʾ - ��ǿ���԰汾
    public void UpdateWeaponDisplay(string weaponName, int weaponIndex)
    {
        Debug.Log($"=== ����������ʾ ===");
        Debug.Log($"��������: '{weaponName}'");
        Debug.Log($"��������: {weaponIndex}");
        Debug.Log($"GetWeaponIndex���: {GetWeaponIndex(weaponName)}");

        // ʹ��GetWeaponIndex�Ľ���������Ǵ����weaponIndex
        int actualIndex = GetWeaponIndex(weaponName);

        if (weaponIcons != null && weaponIcons.Length > actualIndex)
        {
            for (int i = 0; i < weaponIcons.Length; i++)
            {
                if (weaponIcons[i] != null)
                {
                    // ��ǰ����������������͸��
                    if (i == actualIndex)
                    {
                        // ��ǰ���� - ��ȫ��͸�����Ŵ�
                        if (weaponIcons[i].sprite != null)
                        {
                            // ��sprite������ԭɫ����Ϊ��͸��
                            Color spriteColor = Color.white;
                            weaponIcons[i].color = spriteColor;
                        }
                        else
                        {
                            // û��sprite��ʹ��Ĭ����ɫ
                            Color defaultColor = GetDefaultColor(i);
                            weaponIcons[i].color = defaultColor;
                        }
                        weaponIcons[i].transform.localScale = Vector3.one * 1.2f;
                        Debug.Log($"��������ͼ�� {i}");
                    }
                    else
                    {
                        // �������� - ��͸����������С
                        Color currentColor = weaponIcons[i].color;
                        weaponIcons[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.3f);
                        weaponIcons[i].transform.localScale = Vector3.one;
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"�������� {actualIndex} ������Χ��weaponIcons����: {weaponIcons?.Length ?? 0}");
        }
    }

    // ��ȡĬ����ɫ
    Color GetDefaultColor(int index)
    {
        switch (index)
        {
            case CARROT_INDEX: return new Color(1f, 0.5f, 0f); // ��ɫ
            case MEAT_INDEX: return new Color(0.6f, 0.3f, 0.1f); // ��ɫ
            case PEPPER_INDEX: return Color.red; // ��ɫ
            case KNIFE_INDEX: return Color.gray; // ��ɫ
            default: return Color.white;
        }
    }

    // �����������ͻ�ȡ���� - ֧�����������ʽ
    public int GetWeaponIndex(string weaponName)
    {
        Debug.Log($"���ڽ�����������: '{weaponName}'");

        if (string.IsNullOrEmpty(weaponName))
        {
            Debug.LogWarning("��������Ϊ�գ�");
            return 0;
        }

        // ת��ΪСд���бȽϣ������Сд����
        string lowerName = weaponName.ToLower();

        // ֧��ԭ�е�����
        if (lowerName.Contains("carrot") || lowerName.Contains("���ܲ�"))
        {
            Debug.Log("ʶ��Ϊ���ܲ�ǹ");
            return CARROT_INDEX;
        }
        else if (lowerName.Contains("meat") || lowerName.Contains("��"))
        {
            Debug.Log("ʶ��Ϊ��ǹ");
            return MEAT_INDEX;
        }
        else if (lowerName.Contains("pepper") || lowerName.Contains("����"))
        {
            Debug.Log("ʶ��Ϊ����ǹ");
            return PEPPER_INDEX;
        }
        else if (lowerName.Contains("knife") || lowerName.Contains("��"))
        {
            Debug.Log("ʶ��Ϊ��");
            return KNIFE_INDEX;
        }
        // ֧�����gun1, gun2, gun3������ʽ
        else if (lowerName == "gun1" || lowerName == "gun")
        {
            Debug.Log("ʶ��Ϊ���ܲ�ǹ (gun1)");
            return CARROT_INDEX;
        }
        else if (lowerName == "gun2")
        {
            Debug.Log("ʶ��Ϊ��ǹ (gun2)");
            return MEAT_INDEX;
        }
        else if (lowerName == "gun3")
        {
            Debug.Log("ʶ��Ϊ����ǹ (gun3)");
            return PEPPER_INDEX;
        }

        Debug.LogWarning($"δʶ�����������: '{weaponName}'��ʹ��Ĭ������0");
        return 0; // Ĭ�Ϸ��غ��ܲ�ǹ
    }

    // ��ʾ��ʾ������"��Fʳ�ý���"
    public void ShowPickupHint(string message)
    {
        if (pickupHintText != null && pickupHintUI != null)
        {
            pickupHintText.text = message;
            pickupHintUI.SetActive(true);
        }
    }

    // ������ʾ
    public void HidePickupHint()
    {
        if (pickupHintUI != null)
        {
            pickupHintUI.SetActive(false);
        }
    }

    // ���Է������ֶ�����������ʾ
    [ContextMenu("���Ժ��ܲ�ǹ")]
    public void TestCarrot()
    {
        UpdateWeaponDisplay("Carrot Launcher", 0);
    }

    [ContextMenu("������ǹ")]
    public void TestMeat()
    {
        UpdateWeaponDisplay("Meat Blaster", 1);
    }

    [ContextMenu("���Ժ���ǹ")]
    public void TestPepper()
    {
        UpdateWeaponDisplay("Pepper Shooter", 2);
    }

    [ContextMenu("���Ե�")]
    public void TestKnife()
    {
        UpdateWeaponDisplay("Kitchen Knife", 3);
    }
}