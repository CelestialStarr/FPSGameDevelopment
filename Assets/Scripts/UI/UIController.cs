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

        // 调试信息：打印初始化状态
        Debug.Log("=== UIController初始化完成 ===");
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            if (weaponIcons[i] != null)
            {
                Debug.Log($"武器图标 {i}: {weaponIcons[i].name}, sprite: {weaponIcons[i].sprite?.name ?? "null"}");
            }
        }
    }

    // 更新武器显示 - 增强调试版本
    public void UpdateWeaponDisplay(string weaponName, int weaponIndex)
    {
        Debug.Log($"=== 更新武器显示 ===");
        Debug.Log($"武器名称: '{weaponName}'");
        Debug.Log($"传入索引: {weaponIndex}");
        Debug.Log($"GetWeaponIndex结果: {GetWeaponIndex(weaponName)}");

        // 使用GetWeaponIndex的结果，而不是传入的weaponIndex
        int actualIndex = GetWeaponIndex(weaponName);

        if (weaponIcons != null && weaponIcons.Length > actualIndex)
        {
            for (int i = 0; i < weaponIcons.Length; i++)
            {
                if (weaponIcons[i] != null)
                {
                    // 当前武器高亮，其他半透明
                    if (i == actualIndex)
                    {
                        // 当前武器 - 完全不透明，放大
                        if (weaponIcons[i].sprite != null)
                        {
                            // 有sprite，保持原色但设为不透明
                            Color spriteColor = Color.white;
                            weaponIcons[i].color = spriteColor;
                        }
                        else
                        {
                            // 没有sprite，使用默认颜色
                            Color defaultColor = GetDefaultColor(i);
                            weaponIcons[i].color = defaultColor;
                        }
                        weaponIcons[i].transform.localScale = Vector3.one * 1.2f;
                        Debug.Log($"激活武器图标 {i}");
                    }
                    else
                    {
                        // 其他武器 - 半透明，正常大小
                        Color currentColor = weaponIcons[i].color;
                        weaponIcons[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.3f);
                        weaponIcons[i].transform.localScale = Vector3.one;
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"武器索引 {actualIndex} 超出范围！weaponIcons长度: {weaponIcons?.Length ?? 0}");
        }
    }

    // 获取默认颜色
    Color GetDefaultColor(int index)
    {
        switch (index)
        {
            case CARROT_INDEX: return new Color(1f, 0.5f, 0f); // 橙色
            case MEAT_INDEX: return new Color(0.6f, 0.3f, 0.1f); // 棕色
            case PEPPER_INDEX: return Color.red; // 红色
            case KNIFE_INDEX: return Color.gray; // 灰色
            default: return Color.white;
        }
    }

    // 根据武器类型获取索引 - 支持你的命名方式
    public int GetWeaponIndex(string weaponName)
    {
        Debug.Log($"正在解析武器名称: '{weaponName}'");

        if (string.IsNullOrEmpty(weaponName))
        {
            Debug.LogWarning("武器名称为空！");
            return 0;
        }

        // 转换为小写进行比较，避免大小写问题
        string lowerName = weaponName.ToLower();

        // 支持原有的名称
        if (lowerName.Contains("carrot") || lowerName.Contains("胡萝卜"))
        {
            Debug.Log("识别为胡萝卜枪");
            return CARROT_INDEX;
        }
        else if (lowerName.Contains("meat") || lowerName.Contains("肉"))
        {
            Debug.Log("识别为肉枪");
            return MEAT_INDEX;
        }
        else if (lowerName.Contains("pepper") || lowerName.Contains("辣椒"))
        {
            Debug.Log("识别为胡椒枪");
            return PEPPER_INDEX;
        }
        else if (lowerName.Contains("knife") || lowerName.Contains("刀"))
        {
            Debug.Log("识别为刀");
            return KNIFE_INDEX;
        }
        // 支持你的gun1, gun2, gun3命名方式
        else if (lowerName == "gun1" || lowerName == "gun")
        {
            Debug.Log("识别为胡萝卜枪 (gun1)");
            return CARROT_INDEX;
        }
        else if (lowerName == "gun2")
        {
            Debug.Log("识别为肉枪 (gun2)");
            return MEAT_INDEX;
        }
        else if (lowerName == "gun3")
        {
            Debug.Log("识别为胡椒枪 (gun3)");
            return PEPPER_INDEX;
        }

        Debug.LogWarning($"未识别的武器名称: '{weaponName}'，使用默认索引0");
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

    // 调试方法：手动测试武器显示
    [ContextMenu("测试胡萝卜枪")]
    public void TestCarrot()
    {
        UpdateWeaponDisplay("Carrot Launcher", 0);
    }

    [ContextMenu("测试肉枪")]
    public void TestMeat()
    {
        UpdateWeaponDisplay("Meat Blaster", 1);
    }

    [ContextMenu("测试胡椒枪")]
    public void TestPepper()
    {
        UpdateWeaponDisplay("Pepper Shooter", 2);
    }

    [ContextMenu("测试刀")]
    public void TestKnife()
    {
        UpdateWeaponDisplay("Kitchen Knife", 3);
    }
}