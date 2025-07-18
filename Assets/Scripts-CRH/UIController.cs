using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Health UI")]
    public Slider healthSlider;          // Ѫ��������
    public Text healthText;          // Ѫ��������ʾ

    [Header("Pick up UI")]
    public GameObject pickupHintUI;      // ��ʾUI������
    public Text pickupHintText;      // ��ʾ��������

    [Header("Weapon UI")]
    public Text ammoText;            // �ӵ�������ʾ
    public Image[] weaponIcons;        // 3������ͼ�꣨�ײ�1,2,3�� 

    [Header("Weapon Sprites (Optional)")]
    public Sprite carrotSprite;        // ���ܲ�ͼ�� 
    public Sprite meatSprite;          // ��ĩͼ�� 
    public Sprite pepperSprite;        // ����ͼ�� 



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

        // ��ʼ������ͼ�꣨����еĻ��� 

        if (weaponIcons != null && weaponIcons.Length >= 3)

        {

            // �����ͼ������ã�û�о��ô�ɫ 

            if (carrotSprite != null) weaponIcons[0].sprite = carrotSprite;

            else weaponIcons[0].color = new Color(1f, 0.5f, 0f); // ��ɫ 



            if (meatSprite != null) weaponIcons[1].sprite = meatSprite;

            else weaponIcons[1].color = new Color(0.6f, 0.3f, 0.1f); // ��ɫ 



            if (pepperSprite != null) weaponIcons[2].sprite = pepperSprite;

            else weaponIcons[2].color = Color.red; // ��ɫ 

        }

    }



    // ����������Ѿ����ˣ����ֲ��� 

    public void UpdateWeaponDisplay(string weaponName, int weaponIndex)

    {

        // Update weapon icons if you have them 

        if (weaponIcons != null && weaponIcons.Length > 0)

        {

            for (int i = 0; i < weaponIcons.Length; i++)

            {

                if (i < weaponIcons.Length)

                {

                    // ��ǰ����������������͸�� 

                    weaponIcons[i].color = (i == weaponIndex) ? Color.white : new Color(1, 1, 1, 0.3f);



                    // ��ѡ����ǰ�����Ŵ�һ�� 

                    weaponIcons[i].transform.localScale = (i == weaponIndex) ? Vector3.one * 1.2f : Vector3.one;

                }

            }
        }
    }

    // ��ʾ��ʾ�����硰��Fʳ�ý��ӡ�
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


/*
 
using UnityEngine; 

using UnityEngine.UI; 

  

public class UIController : MonoBehaviour 

{ 

    public static UIController Instance; 

     

    [Header("Health UI")] 

    public Slider healthSlider; 

    public Text healthText; 

     



    private void Awake() 

    { 

        Instance = this; 

    } 

     



    

} 

 */