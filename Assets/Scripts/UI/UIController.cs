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
    }

    // ����������ʾ - �Ľ��汾
    public void UpdateWeaponDisplay(string weaponName, int weaponIndex)
    {
        Debug.Log($"Updating weapon display: {weaponName}, Index: {weaponIndex}");

        if (weaponIcons != null && weaponIcons.Length > weaponIndex)
        {
            for (int i = 0; i < weaponIcons.Length; i++)
            {
                if (weaponIcons[i] != null)
                {
                    // ��ǰ����������������͸��
                    Color currentColor = weaponIcons[i].color;
                    if (i == weaponIndex)
                    {
                        // ��ǰ���� - ��ȫ��͸��
                        weaponIcons[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
                        weaponIcons[i].transform.localScale = Vector3.one * 1.2f; // �Ŵ�
                    }
                    else
                    {
                        // �������� - ��͸��
                        weaponIcons[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.3f);
                        weaponIcons[i].transform.localScale = Vector3.one; // ������С
                    }
                }
            }
        }
    }

    // �����������ͻ�ȡ����
    public int GetWeaponIndex(string weaponName)
    {
        if (weaponName.Contains("Carrot") || weaponName.Contains("���ܲ�"))
            return CARROT_INDEX;
        else if (weaponName.Contains("Meat") || weaponName.Contains("��"))
            return MEAT_INDEX;
        else if (weaponName.Contains("Pepper") || weaponName.Contains("����"))
            return PEPPER_INDEX;
        else if (weaponName.Contains("Knife") || weaponName.Contains("��"))
            return KNIFE_INDEX;

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
}