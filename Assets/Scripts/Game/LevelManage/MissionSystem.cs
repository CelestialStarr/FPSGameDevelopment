using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Mission类定义
[System.Serializable]
public class Mission
{
    public string id;
    public string description;
    public int targetAmount;
    public bool completed;

    public Mission(string id, string description, int targetAmount, bool completed = false)
    {
        this.id = id;
        this.description = description;
        this.targetAmount = targetAmount;
        this.completed = completed;
    }
}

public class MissionSystem : MonoBehaviour
{
    public static MissionSystem Instance;

    [Header("Mission UI References")]
    public GameObject missionPanel;
    public Text missionTitleText;
    public Transform missionListParent;
    public GameObject missionItemPrefab; // 可选，留空会自动创建

    [Header("Mission UI Settings")]
    [SerializeField] private int missionTextFontSize = 50;
    [SerializeField] private float missionItemHeight = 60f;
    [SerializeField] private float missionItemSpacing = 65f;
    [SerializeField] private Vector2 missionStartPosition = new Vector2(-200f, 50f); // 第一个任务的起始位置
    [SerializeField] private Color missionTextColor = Color.white;
    [SerializeField] private Color missionCompletedColor = Color.green;
    [SerializeField] private TextAnchor textAlignment = TextAnchor.MiddleLeft;

    [Header("Level 1 Settings")]
    [SerializeField] private int level1_KillsRequired = 5;
    [SerializeField] private int level1_CornAmmoRequired = 3;
    [SerializeField] private int level1_MeatAmmoRequired = 2;
    [SerializeField] private int level1_VegetableAmmoRequired = 1;
    [Space]
    [Header("Level 1 Task Control (Set to 0 to hide task)")]
    [SerializeField] private bool level1_ShowKillTask = true;
    [SerializeField] private bool level1_ShowCornTask = true;
    [SerializeField] private bool level1_ShowMeatTask = true;
    [SerializeField] private bool level1_ShowVegetableTask = true;

    [Header("Level 2 Settings")]
    [SerializeField] private int level2_KillsRequired = 10;
    [SerializeField] private int level2_CornAmmoRequired = 5;
    [SerializeField] private int level2_MeatAmmoRequired = 3;
    [SerializeField] private int level2_VegetableAmmoRequired = 2;
    [Space]
    [Header("Level 2 Task Control (Set to 0 to hide task)")]
    [SerializeField] private bool level2_ShowKillTask = true;
    [SerializeField] private bool level2_ShowCornTask = true;
    [SerializeField] private bool level2_ShowMeatTask = true;
    [SerializeField] private bool level2_ShowVegetableTask = true;

    [Header("Level 3 Settings")]
    [SerializeField] private int level3_KillsRequired = 15;
    [SerializeField] private int level3_CornAmmoRequired = 8;
    [SerializeField] private int level3_MeatAmmoRequired = 5;
    [SerializeField] private int level3_VegetableAmmoRequired = 3;
    [SerializeField] private bool level3_BossRequired = true;
    [Space]
    [Header("Level 3 Task Control (Set to 0 to hide task)")]
    [SerializeField] private bool level3_ShowKillTask = true;
    [SerializeField] private bool level3_ShowCornTask = true;
    [SerializeField] private bool level3_ShowMeatTask = true;
    [SerializeField] private bool level3_ShowVegetableTask = true;
    [SerializeField] private bool level3_ShowBossTask = true;

    [Header("Current Scene")]
    public SceneType currentScene = SceneType.Level1;

    // 私有变量
    private List<Mission> currentMissions = new List<Mission>();
    private Dictionary<string, Text> missionUIElements = new Dictionary<string, Text>();

    // 当前进度
    private int currentKills = 0;
    private int currentCornAmmo = 0;
    private int currentMeatAmmo = 0;
    private int currentVegetableAmmo = 0;
    private bool bossDefeated = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeMissions();

        // 如果UI设置了就创建，没设置就只在后台跟踪
        if (missionPanel != null && missionTitleText != null && missionListParent != null)
        {
            CreateMissionUI();
        }
        else
        {
            Debug.LogWarning("[MissionSystem] UI not set up, running in background mode");
        }
    }

    void InitializeMissions()
    {
        currentMissions.Clear();
        currentKills = 0;
        currentCornAmmo = 0;
        currentMeatAmmo = 0;
        currentVegetableAmmo = 0;
        bossDefeated = false;

        switch (currentScene)
        {
            case SceneType.Level1:
                // 只添加数量大于0且显示标志为true的任务
                if (level1_KillsRequired > 0 && level1_ShowKillTask)
                    currentMissions.Add(new Mission("kill_enemies", $"Eliminate {level1_KillsRequired} enemies", level1_KillsRequired, false));

                if (level1_CornAmmoRequired > 0 && level1_ShowCornTask)
                    currentMissions.Add(new Mission("collect_corn", $"Collect {level1_CornAmmoRequired} Corn ammo", level1_CornAmmoRequired, false));

                if (level1_MeatAmmoRequired > 0 && level1_ShowMeatTask)
                    currentMissions.Add(new Mission("collect_meat", $"Collect {level1_MeatAmmoRequired} Meat ammo", level1_MeatAmmoRequired, false));

                if (level1_VegetableAmmoRequired > 0 && level1_ShowVegetableTask)
                    currentMissions.Add(new Mission("collect_vegetable", $"Collect {level1_VegetableAmmoRequired} Vegetable ammo", level1_VegetableAmmoRequired, false));
                break;

            case SceneType.Level2:
                if (level2_KillsRequired > 0 && level2_ShowKillTask)
                    currentMissions.Add(new Mission("kill_enemies", $"Eliminate {level2_KillsRequired} enemies", level2_KillsRequired, false));

                if (level2_CornAmmoRequired > 0 && level2_ShowCornTask)
                    currentMissions.Add(new Mission("collect_corn", $"Collect {level2_CornAmmoRequired} Corn ammo", level2_CornAmmoRequired, false));

                if (level2_MeatAmmoRequired > 0 && level2_ShowMeatTask)
                    currentMissions.Add(new Mission("collect_meat", $"Collect {level2_MeatAmmoRequired} Meat ammo", level2_MeatAmmoRequired, false));

                if (level2_VegetableAmmoRequired > 0 && level2_ShowVegetableTask)
                    currentMissions.Add(new Mission("collect_vegetable", $"Collect {level2_VegetableAmmoRequired} Vegetable ammo", level2_VegetableAmmoRequired, false));
                break;

            case SceneType.Level3:
                if (level3_KillsRequired > 0 && level3_ShowKillTask)
                    currentMissions.Add(new Mission("kill_enemies", $"Eliminate {level3_KillsRequired} enemies", level3_KillsRequired, false));

                if (level3_CornAmmoRequired > 0 && level3_ShowCornTask)
                    currentMissions.Add(new Mission("collect_corn", $"Collect {level3_CornAmmoRequired} Corn ammo", level3_CornAmmoRequired, false));

                if (level3_MeatAmmoRequired > 0 && level3_ShowMeatTask)
                    currentMissions.Add(new Mission("collect_meat", $"Collect {level3_MeatAmmoRequired} Meat ammo", level3_MeatAmmoRequired, false));

                if (level3_VegetableAmmoRequired > 0 && level3_ShowVegetableTask)
                    currentMissions.Add(new Mission("collect_vegetable", $"Collect {level3_VegetableAmmoRequired} Vegetable ammo", level3_VegetableAmmoRequired, false));

                if (level3_BossRequired && level3_ShowBossTask)
                    currentMissions.Add(new Mission("defeat_boss", "Defeat the Boss", 1, false));
                break;
        }

        Debug.Log($"[MissionSystem] Initialized {currentMissions.Count} missions for {currentScene}");
    }

    void CreateMissionUI()
    {
        // 设置标题
        missionTitleText.text = "MISSIONS:";

        // 清理旧的任务UI
        foreach (Transform child in missionListParent)
        {
            if (child.name.StartsWith("Mission_"))
            {
                Destroy(child.gameObject);
            }
        }
        missionUIElements.Clear();

        // 为每个任务创建UI
        for (int i = 0; i < currentMissions.Count; i++)
        {
            CreateMissionUIItem(currentMissions[i], i);
        }

        // 显示面板
        missionPanel.SetActive(true);
        Debug.Log("[MissionSystem] Mission UI created successfully!");
    }

    void CreateMissionUIItem(Mission mission, int index)
    {
        GameObject item = new GameObject($"Mission_{mission.id}");
        item.transform.SetParent(missionListParent);

        // 添加Text组件
        Text itemText = item.AddComponent<Text>();
        itemText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemText.fontSize = missionTextFontSize;
        itemText.color = missionTextColor;
        itemText.alignment = textAlignment; // 使用Inspector设置的对齐方式
        itemText.verticalOverflow = VerticalWrapMode.Overflow;
        itemText.horizontalOverflow = HorizontalWrapMode.Overflow;

        // 设置RectTransform
        RectTransform rectTransform = item.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1); // 锚点在左上角
        rectTransform.anchorMax = new Vector2(1, 1); // 右上角
        rectTransform.pivot = new Vector2(0, 1); // 轴心在左上角
        rectTransform.sizeDelta = new Vector2(0, missionItemHeight);

        // 计算每个任务的位置
        float yPosition = missionStartPosition.y + (-index * missionItemSpacing);
        rectTransform.anchoredPosition = new Vector2(missionStartPosition.x, yPosition);

        Debug.Log($"[MissionSystem] Created mission {index}: {mission.description} at position ({missionStartPosition.x}, {yPosition})");

        // 更新文本并保存引用
        UpdateMissionText(mission, itemText);
        missionUIElements[mission.id] = itemText;
    }

    void UpdateMissionText(Mission mission, Text uiText)
    {
        if (uiText == null) return;

        string statusIcon = mission.completed ? "✓" : "○";
        string progressText = "";

        switch (mission.id)
        {
            case "kill_enemies":
                progressText = $" ({currentKills}/{mission.targetAmount})";
                break;
            case "collect_corn":
                progressText = $" ({currentCornAmmo}/{mission.targetAmount})";
                break;
            case "collect_meat":
                progressText = $" ({currentMeatAmmo}/{mission.targetAmount})";
                break;
            case "collect_vegetable":
                progressText = $" ({currentVegetableAmmo}/{mission.targetAmount})";
                break;
            case "defeat_boss":
                progressText = bossDefeated ? " (Completed)" : " (In Progress)";
                break;
        }

        uiText.text = $"{statusIcon} {mission.description}{progressText}";
        uiText.color = mission.completed ? missionCompletedColor : missionTextColor; // 使用Inspector设置的颜色
    }

    // 公共方法：敌人击杀
    public void OnEnemyKilled()
    {
        currentKills++;
        CheckMissionProgress("kill_enemies", currentKills);
        Debug.Log($"[MissionSystem] Enemy killed. Total: {currentKills}");
    }

    // 公共方法：弹药收集
    public void OnAmmoCollected(Gun.GunType ammoType)
    {
        string missionId = "";
        int currentAmount = 0;

        switch (ammoType)
        {
            case Gun.GunType.Carrot:
                currentCornAmmo++;
                missionId = "collect_corn";
                currentAmount = currentCornAmmo;
                break;
            case Gun.GunType.Meat:
                currentMeatAmmo++;
                missionId = "collect_meat";
                currentAmount = currentMeatAmmo;
                break;
            case Gun.GunType.Pepper:
                currentVegetableAmmo++;
                missionId = "collect_vegetable";
                currentAmount = currentVegetableAmmo;
                break;
        }

        CheckMissionProgress(missionId, currentAmount);
        Debug.Log($"[MissionSystem] {ammoType} ammo collected. Total: {currentAmount}");
    }

    // 公共方法：Boss击败
    public void OnBossDefeated()
    {
        bossDefeated = true;
        CheckMissionProgress("defeat_boss", 1);
        Debug.Log("[MissionSystem] Boss defeated!");
    }

    void CheckMissionProgress(string missionId, int currentAmount)
    {
        foreach (Mission mission in currentMissions)
        {
            if (mission.id == missionId && !mission.completed)
            {
                if (currentAmount >= mission.targetAmount)
                {
                    CompleteMission(mission.id);
                }
                else
                {
                    UpdateMissionUI(mission.id);
                }
                break;
            }
        }

        CheckAllMissionsComplete();
    }

    void CompleteMission(string missionId)
    {
        foreach (Mission mission in currentMissions)
        {
            if (mission.id == missionId)
            {
                mission.completed = true;
                UpdateMissionUI(missionId);
                Debug.Log($"[MissionSystem] Mission completed: {mission.description}");
                break;
            }
        }
    }

    void UpdateMissionUI(string missionId)
    {
        if (missionUIElements.ContainsKey(missionId))
        {
            Mission mission = currentMissions.Find(m => m.id == missionId);
            if (mission != null)
            {
                UpdateMissionText(mission, missionUIElements[missionId]);
            }
        }
    }

    void CheckAllMissionsComplete()
    {
        bool allCompleted = true;
        foreach (Mission mission in currentMissions)
        {
            if (!mission.completed)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            Debug.Log("[MissionSystem] All missions completed! Level can be completed.");

            // 触发关卡完成
            if (LevelComplete.Instance != null)
            {
                Invoke("TriggerLevelComplete", 1.5f);
            }
        }
    }

    void TriggerLevelComplete()
    {
        if (LevelComplete.Instance != null)
        {
            LevelComplete.Instance.ShowLevelCompleteUI();
        }
    }

    // 在运行时更新UI样式和位置
    [ContextMenu("Refresh Mission UI Style")]
    void RefreshMissionUIStyle()
    {
        // 重新创建所有UI项目以应用新的位置设置
        CreateMissionUI();
        Debug.Log("[MissionSystem] UI style and positions refreshed!");
    }
    [ContextMenu("Test Kill Enemy")]
    void TestKillEnemy() { OnEnemyKilled(); }

    [ContextMenu("Test Collect Corn")]
    void TestCollectCorn() { OnAmmoCollected(Gun.GunType.Carrot); }

    [ContextMenu("Test Collect Meat")]
    void TestCollectMeat() { OnAmmoCollected(Gun.GunType.Meat); }

    [ContextMenu("Test Collect Vegetable")]
    void TestCollectVegetable() { OnAmmoCollected(Gun.GunType.Pepper); }

    [ContextMenu("Test Defeat Boss")]
    void TestDefeatBoss() { OnBossDefeated(); }
}