using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Health UI")]
    public Slider healthSlider;          // 血量滑动条
    public Text healthText;          // 血量文字显示

    [Header("Pick up UI")]
    public GameObject pickupHintUI;      // 提示UI背景框
    public Text pickupHintText;      // 提示文字内容

    [Header("Weapon UI")]
    public Text ammoText;            // 子弹数量显示
    public Image[] weaponIcons;        // 3个武器图标（底部1,2,3） 

    [Header("Weapon Sprites (Optional)")]
    public Sprite carrotSprite;        // 胡萝卜图标 
    public Sprite meatSprite;          // 肉末图标 
    public Sprite pepperSprite;        // 辣椒图标 



    private void Awake()
    {
        // 单例赋值
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

        // 初始化武器图标（如果有的话） 

        if (weaponIcons != null && weaponIcons.Length >= 3)

        {

            // 如果有图标就设置，没有就用纯色 

            if (carrotSprite != null) weaponIcons[0].sprite = carrotSprite;

            else weaponIcons[0].color = new Color(1f, 0.5f, 0f); // 橙色 



            if (meatSprite != null) weaponIcons[1].sprite = meatSprite;

            else weaponIcons[1].color = new Color(0.6f, 0.3f, 0.1f); // 棕色 



            if (pepperSprite != null) weaponIcons[2].sprite = pepperSprite;

            else weaponIcons[2].color = Color.red; // 红色 

        }

    }



    // 这个方法你已经有了，保持不变 

    public void UpdateWeaponDisplay(string weaponName, int weaponIndex)

    {

        // Update weapon icons if you have them 

        if (weaponIcons != null && weaponIcons.Length > 0)

        {

            for (int i = 0; i < weaponIcons.Length; i++)

            {

                if (i < weaponIcons.Length)

                {

                    // 当前武器高亮，其他半透明 

                    weaponIcons[i].color = (i == weaponIndex) ? Color.white : new Color(1, 1, 1, 0.3f);



                    // 可选：当前武器放大一点 

                    weaponIcons[i].transform.localScale = (i == weaponIndex) ? Vector3.one * 1.2f : Vector3.one;

                }

            }
        }
    }

    // 显示提示，例如“按F食用饺子”
    public void ShowPickupHint(string message)
    {
        if (pickupHintText != null && pickupHintUI != null)
        {
            pickupHintText.text = message;
            pickupHintUI.SetActive(true);
        }
    }

    // 隐藏提示
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