using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Health UI")]
    public Slider healthSlider;          // 血量滑动条
    public Text healthText;              // 血量文字显示

    [Header("Pick up UI")]
    public GameObject pickupHintUI;      // 提示UI背景框
    public Text pickupHintText;          // 提示文字内容

    [Header("Weapon UI")]
    public Text ammoText;                // 子弹数量显示
    public Image[] weaponIcons;          // 4个武器图标（包括刀）

    [Header("Weapon Sprites")]
    public Sprite knifeSprite;           // 刀图标
    public Sprite carrotSprite;          // 胡萝卜图标 
    public Sprite meatSprite;            // 肉末图标 
    public Sprite pepperSprite;          // 辣椒图标 

    // 武器索引常量
    public const int CARROT_INDEX = 0;
    public const int MEAT_INDEX = 1;
    public const int PEPPER_INDEX = 2;
    public const int KNIFE_INDEX = 3;

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
        // 初始化武器图标
        if (weaponIcons != null && weaponIcons.Length >= 4)
        {
            // 设置武器图标和默认颜色 - 新顺序：0胡萝卜 1肉 2胡椒 3刀
            if (carrotSprite != null)
                weaponIcons[CARROT_INDEX].sprite = carrotSprite;
            else
                weaponIcons[CARROT_INDEX].color = new Color(1f, 0.5f, 0f); // 橙色

            if (meatSprite != null)
                weaponIcons[MEAT_INDEX].sprite = meatSprite;
            else
                weaponIcons[MEAT_INDEX].color = new Color(0.6f, 0.3f, 0.1f); // 棕色

            if (pepperSprite != null)
                weaponIcons[PEPPER_INDEX].sprite = pepperSprite;
            else
                weaponIcons[PEPPER_INDEX].color = Color.red; // 红色

            if (knifeSprite != null)
                weaponIcons[KNIFE_INDEX].sprite = knifeSprite;
            else
                weaponIcons[KNIFE_INDEX].color = Color.gray; // 刀为灰色
        }
    }

    // 更新武器显示 - 改进版本
    public void UpdateWeaponDisplay(string weaponName, int weaponIndex)
    {
        Debug.Log($"Updating weapon display: {weaponName}, Index: {weaponIndex}");

        if (weaponIcons != null && weaponIcons.Length > weaponIndex)
        {
            for (int i = 0; i < weaponIcons.Length; i++)
            {
                if (weaponIcons[i] != null)
                {
                    // 当前武器高亮，其他半透明
                    Color currentColor = weaponIcons[i].color;
                    if (i == weaponIndex)
                    {
                        // 当前武器 - 完全不透明
                        weaponIcons[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
                        weaponIcons[i].transform.localScale = Vector3.one * 1.2f; // 放大
                    }
                    else
                    {
                        // 其他武器 - 半透明
                        weaponIcons[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.3f);
                        weaponIcons[i].transform.localScale = Vector3.one; // 正常大小
                    }
                }
            }
        }
    }

    // 根据武器类型获取索引
    public int GetWeaponIndex(string weaponName)
    {
        if (weaponName.Contains("Carrot") || weaponName.Contains("胡萝卜"))
            return CARROT_INDEX;
        else if (weaponName.Contains("Meat") || weaponName.Contains("肉"))
            return MEAT_INDEX;
        else if (weaponName.Contains("Pepper") || weaponName.Contains("辣椒"))
            return PEPPER_INDEX;
        else if (weaponName.Contains("Knife") || weaponName.Contains("刀"))
            return KNIFE_INDEX;

        return 0; // 默认返回胡萝卜枪
    }

    // 显示提示，例如"按F食用饺子"
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