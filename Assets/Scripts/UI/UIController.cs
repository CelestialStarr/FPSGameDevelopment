using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Health UI")]
    public Slider healthSlider;
    public Text healthText;

    [Header("Pick up UI")]
    public GameObject pickupHintUI;
    public Text pickupHintText;

    [Header("Weapon UI")]
    public Text ammoText;
    public Image[] weaponIcons;

    [Header("Weapon Sprites")]
    public Sprite knifeSprite;
    public Sprite carrotSprite;
    public Sprite meatSprite;
    public Sprite pepperSprite;

    [Header("Death UI")]
    public GameObject deathUIPanel;
    public Text deathTitleText;
    public Text deathCountdownText;

    [Header("Pause UI")]
    public GameObject pausePanel;
    public Slider volumeSlider;

    // 武器索引常量
    public const int CARROT_INDEX = 0;
    public const int MEAT_INDEX = 1;
    public const int PEPPER_INDEX = 2;
    public const int KNIFE_INDEX = 3;

    private void Awake()
    {
        // 关键修改：不再使用DontDestroyOnLoad，每个场景重新创建
        Instance = this;
        // 删除这行：DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeWeaponIcons();

        // 确保死亡UI和暂停UI开始时是隐藏的
        if (deathUIPanel != null)
        {
            deathUIPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 设置音量滑块事件
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void InitializeWeaponIcons()
    {
        // 初始化武器图标
        if (weaponIcons != null && weaponIcons.Length >= 4)
        {
            if (carrotSprite != null)
                weaponIcons[CARROT_INDEX].sprite = carrotSprite;
            else
                weaponIcons[CARROT_INDEX].color = new Color(1f, 0.5f, 0f);

            if (meatSprite != null)
                weaponIcons[MEAT_INDEX].sprite = meatSprite;
            else
                weaponIcons[MEAT_INDEX].color = new Color(0.6f, 0.3f, 0.1f);

            if (pepperSprite != null)
                weaponIcons[PEPPER_INDEX].sprite = pepperSprite;
            else
                weaponIcons[PEPPER_INDEX].color = Color.red;

            if (knifeSprite != null)
                weaponIcons[KNIFE_INDEX].sprite = knifeSprite;
            else
                weaponIcons[KNIFE_INDEX].color = Color.gray;
        }
    }

    public void UpdateWeaponDisplay(string weaponName, int weaponIndex)
    {
        int actualIndex = GetWeaponIndex(weaponName);

        if (weaponIcons != null && weaponIcons.Length > actualIndex)
        {
            for (int i = 0; i < weaponIcons.Length; i++)
            {
                if (weaponIcons[i] != null)
                {
                    if (i == actualIndex)
                    {
                        if (weaponIcons[i].sprite != null)
                        {
                            weaponIcons[i].color = Color.white;
                        }
                        else
                        {
                            weaponIcons[i].color = GetDefaultColor(i);
                        }
                        weaponIcons[i].transform.localScale = Vector3.one * 1.2f;
                    }
                    else
                    {
                        Color currentColor = weaponIcons[i].color;
                        weaponIcons[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.3f);
                        weaponIcons[i].transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }

    Color GetDefaultColor(int index)
    {
        switch (index)
        {
            case CARROT_INDEX: return new Color(1f, 0.5f, 0f);
            case MEAT_INDEX: return new Color(0.6f, 0.3f, 0.1f);
            case PEPPER_INDEX: return Color.red;
            case KNIFE_INDEX: return Color.gray;
            default: return Color.white;
        }
    }

    public int GetWeaponIndex(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName))
        {
            return 0;
        }

        string lowerName = weaponName.ToLower();

        if (lowerName.Contains("carrot") || lowerName.Contains("胡萝卜"))
            return CARROT_INDEX;
        else if (lowerName.Contains("meat") || lowerName.Contains("肉"))
            return MEAT_INDEX;
        else if (lowerName.Contains("pepper") || lowerName.Contains("辣椒"))
            return PEPPER_INDEX;
        else if (lowerName.Contains("knife") || lowerName.Contains("刀"))
            return KNIFE_INDEX;
        else if (lowerName == "gun1" || lowerName == "gun")
            return CARROT_INDEX;
        else if (lowerName == "gun2")
            return MEAT_INDEX;
        else if (lowerName == "gun3")
            return PEPPER_INDEX;

        return 0;
    }

    public void ShowPickupHint(string message)
    {
        if (pickupHintText != null && pickupHintUI != null)
        {
            pickupHintText.text = message;
            pickupHintUI.SetActive(true);
        }
    }

    public void HidePickupHint()
    {
        if (pickupHintUI != null)
        {
            pickupHintUI.SetActive(false);
        }
    }

    public void ShowDeathUI()
    {
        if (deathUIPanel != null)
        {
            deathUIPanel.SetActive(true);
        }

        if (deathTitleText != null)
        {
            deathTitleText.text = "YOU ARE DEAD";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("显示死亡UI");
    }

    public void UpdateDeathCountdown(float timeLeft)
    {
        if (deathCountdownText != null)
        {
            if (timeLeft > 0)
            {
                deathCountdownText.text = $"Respawning in {timeLeft:F0}s";
            }
            else
            {
                deathCountdownText.text = "Respawning...";
            }
        }
    }

    public void HideDeathUI()
    {
        if (deathUIPanel != null)
        {
            deathUIPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("隐藏死亡UI");
    }

    // 暂停UI管理方法
    public void ShowPauseUI()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void HidePauseUI()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    // 音量滑块事件
    private void OnVolumeChanged(float value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetVolume(value);
        }
    }

    // UI按钮事件方法
    public void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    public void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    public void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}