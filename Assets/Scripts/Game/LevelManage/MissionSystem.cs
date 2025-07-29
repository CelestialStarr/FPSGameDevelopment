using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 将Mission类定义移到前面
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

    [Header("Mission UI References (独立的任务UI)")]
    public GameObject missionPanel;
    public Text missionTitleText;
    public Transform missionListParent;
    public GameObject missionItemPrefab;

    [Header("Level 1 Settings")]
    [SerializeField] private int level1_KillsRequired = 10;
    [SerializeField] private int level1_CornAmmoRequired = 20;
    [SerializeField] private int level1_MeatAmmoRequired = 15;
    [SerializeField] private int level1_VegetableAmmoRequired = 10;

    [Header("Level 2 Settings")]
    [SerializeField] private int level2_KillsRequired = 15;
    [SerializeField] private int level2_CornAmmoRequired = 25;
    [SerializeField] private int level2_MeatAmmoRequired = 20;
    [SerializeField] private int level2_VegetableAmmoRequired = 15;

    [Header("Level 3 Settings")]
    [SerializeField] private int level3_KillsRequired = 20;
    [SerializeField] private int level3_CornAmmoRequired = 30;
    [SerializeField] private int level3_MeatAmmoRequired = 25;
    [SerializeField] private int level3_VegetableAmmoRequired = 20;
    [SerializeField] private bool level3_BossRequired = true;

    [Header("Current Scene")]
    public SceneType currentScene = SceneType.Level1;

    // 当前任务数据
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
        // 只有当UI引用都设置好了才初始化
        if (missionPanel != null && missionTitleText != null && missionListParent != null)
        {
            InitializeMissions();
            CreateMissionUI();
        }
        else
        {
            Debug.LogWarning("[MissionSystem] UI references not set! Mission system will track progress but won't show UI.");
            InitializeMissions(); // 仍然初始化任务数据，只是不显示UI
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
                currentMissions.Add(new Mission("unlock_teleport", $"Kill {level1_KillsRequired} enemies to unlock teleport", level1_KillsRequired, false));
                currentMissions.Add(new Mission("collect_corn", $"Collect {level1_CornAmmoRequired} Corn ammo", level1_CornAmmoRequired, false));
                currentMissions.Add(new Mission("collect_meat", $"Collect {level1_MeatAmmoRequired} Meat ammo", level1_MeatAmmoRequired, false));
                currentMissions.Add(new Mission("collect_vegetable", $"Collect {level1_VegetableAmmoRequired} Vegetable ammo", level1_VegetableAmmoRequired, false));
                break;

            case SceneType.Level2:
                currentMissions.Add(new Mission("kill_enemies", $"Eliminate {level2_KillsRequired} enemies", level2_KillsRequired, false));
                currentMissions.Add(new Mission("collect_corn", $"Collect {level2_CornAmmoRequired} Corn ammo", level2_CornAmmoRequired, false));
                currentMissions.Add(new Mission("collect_meat", $"Collect {level2_MeatAmmoRequired} Meat ammo", level2_MeatAmmoRequired, false));
                currentMissions.Add(new Mission("collect_vegetable", $"Collect {level2_VegetableAmmoRequired} Vegetable ammo", level2_VegetableAmmoRequired, false));
                break;

            case SceneType.Level3:
                currentMissions.Add(new Mission("kill_enemies", $"Eliminate {level3_KillsRequired} enemies", level3_KillsRequired, false));
                currentMissions.Add(new Mission("collect_corn", $"Collect {level3_CornAmmoRequired} Corn ammo", level3_CornAmmoRequired, false));
                currentMissions.Add(new Mission("collect_meat", $"Collect {level3_MeatAmmoRequired} Meat ammo", level3_MeatAmmoRequired, false));
                currentMissions.Add(new Mission("collect_vegetable", $"Collect {level3_VegetableAmmoRequired} Vegetable ammo", level3_VegetableAmmoRequired, false));
                if (level3_BossRequired)
                {
                    currentMissions.Add(new Mission("defeat_boss", "Defeat the Boss", 1, false));
                }
                break;
        }

        Debug.Log($"[MissionSystem] Initialized {currentMissions.Count} missions for {currentScene}");
    }

    void CreateMissionUI()
    {
        // 安全检查：只有UI引用存在才创建UI
        if (missionPanel == null || missionTitleText == null || missionListParent == null)
        {
            Debug.LogWarning("[MissionSystem] Cannot create UI - missing references!");
            return;
        }

        // 设置标题
        missionTitleText.text = "MISSIONS:";

        // 清理现有的任务UI元素（只清理我们创建的）
        foreach (Transform child in missionListParent)
        {
            if (child.name.StartsWith("Mission_"))
            {
                Destroy(child.gameObject);
            }
        }
        missionUIElements.Clear();

        // 为每个任务创建UI
        foreach (Mission mission in currentMissions)
        {
            CreateMissionUIItem(mission);
        }

        // 显示任务面板
        missionPanel.SetActive(true);
        Debug.Log("[MissionSystem] Mission UI created successfully!");
    }

    void CreateMissionUIItem(Mission mission)
    {
        GameObject item;

        if (missionItemPrefab != null)
        {
            // 使用预制体
            item = Instantiate(missionItemPrefab, missionListParent);
            Debug.Log($"[MissionSystem] Created mission item using prefab for: {mission.description}");
        }
        else
        {
            // 创建简单的文本UI
            item = new GameObject($"Mission_{mission.id}");
            item.transform.SetParent(missionListParent);

            Text itemText = item.AddComponent<Text>();
            // 设置默认文本样式
            itemText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            itemText.fontSize = 12;
            itemText.color = Color.white;
            itemText.alignment = TextAnchor.MiddleLeft;

            Debug.Log($"[MissionSystem] Created mission text item for: {mission.description}");
        }

        Text textComponent = item.GetComponent<Text>();
        if (textComponent != null)
        {
            UpdateMissionText(mission, textComponent);
            missionUIElements[mission.id] = textComponent;
            Debug.Log($"[MissionSystem] Text component found and updated for: {mission.description}");
        }
        else
        {
            Debug.LogError($"[MissionSystem] No Text component found on mission item: {mission.description}");
        }

        // 设置RectTransform
        RectTransform rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(0, 18);

            // 设置位置（自动排列）
            int index = currentMissions.IndexOf(mission);
            rectTransform.anchoredPosition = new Vector2(5, -index * 20 - 5);

            Debug.Log($"[MissionSystem] RectTransform set for mission {index}: {mission.description} at position {rectTransform.anchoredPosition}");
        }
        else
        {
            Debug.LogError($"[MissionSystem] No RectTransform found on mission item: {mission.description}");
        }
    }

    void UpdateMissionText(Mission mission, Text uiText)
    {
        if (uiText == null) return;

        string statusIcon = mission.completed ? "✓" : "○";
        string progressText = "";

        switch (mission.id)
        {
            case "unlock_teleport":
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

        // 已完成的任务显示为绿色
        uiText.color = mission.completed ? Color.green : Color.white;
    }

    // 公共方法：敌人击杀事件
    public void OnEnemyKilled()
    {
        currentKills++;

        // 检查击杀相关任务
        foreach (Mission mission in currentMissions)
        {
            if ((mission.id == "unlock_teleport" || mission.id == "kill_enemies") && !mission.completed)
            {
                if (currentKills >= mission.targetAmount)
                {
                    CompleteMission(mission.id);
                }
                else
                {
                    UpdateMissionUI(mission.id);
                }
            }
        }

        CheckLevelComplete();
        Debug.Log($"[MissionSystem] Enemy killed. Total: {currentKills}");
    }

    // 公共方法：弹药收集事件（按类型分别计数）
    public void OnAmmoCollected(Gun.GunType ammoType)
    {
        string missionId = "";

        switch (ammoType)
        {
            case Gun.GunType.Carrot:
                currentCornAmmo++;
                missionId = "collect_corn";
                Debug.Log($"[MissionSystem] Corn ammo collected. Total: {currentCornAmmo}");
                break;
            case Gun.GunType.Meat:
                currentMeatAmmo++;
                missionId = "collect_meat";
                Debug.Log($"[MissionSystem] Meat ammo collected. Total: {currentMeatAmmo}");
                break;
            case Gun.GunType.Pepper:
                currentVegetableAmmo++;
                missionId = "collect_vegetable";
                Debug.Log($"[MissionSystem] Vegetable ammo collected. Total: {currentVegetableAmmo}");
                break;
        }

        // 检查对应的收集任务
        foreach (Mission mission in currentMissions)
        {
            if (mission.id == missionId && !mission.completed)
            {
                int currentAmount = GetCurrentAmmoAmount(ammoType);
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

        CheckLevelComplete();
    }

    // 辅助方法：获取当前弹药数量
    private int GetCurrentAmmoAmount(Gun.GunType ammoType)
    {
        switch (ammoType)
        {
            case Gun.GunType.Carrot: return currentCornAmmo;
            case Gun.GunType.Meat: return currentMeatAmmo;
            case Gun.GunType.Pepper: return currentVegetableAmmo;
            default: return 0;
        }
    }

    // 公共方法：Boss击败事件
    public void OnBossDefeated()
    {
        bossDefeated = true;
        CompleteMission("defeat_boss");
        CheckLevelComplete();
        Debug.Log("[MissionSystem] Boss defeated!");
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

    void CheckLevelComplete()
    {
        // 检查是否所有任务都完成了
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
                // 延迟一下让玩家看到任务完成
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

    // 重置任务系统（测试用）
    public void ResetMissions()
    {
        InitializeMissions();
        CreateMissionUI();
        Debug.Log("[MissionSystem] Missions reset!");
    }

    // 获取当前任务进度（用于其他系统查询）
    public int GetCurrentKills() => currentKills;
    public int GetCurrentCornAmmo() => currentCornAmmo;
    public int GetCurrentMeatAmmo() => currentMeatAmmo;
    public int GetCurrentVegetableAmmo() => currentVegetableAmmo;
    public bool IsBossDefeated() => bossDefeated;

    // 检查特定任务是否完成
    public bool IsMissionCompleted(string missionId)
    {
        Mission mission = currentMissions.Find(m => m.id == missionId);
        return mission != null && mission.completed;
    }

    // Inspector中的测试按钮功能
    [ContextMenu("Test Kill Enemy")]
    void TestKillEnemy()
    {
        OnEnemyKilled();
    }

    [ContextMenu("Test Collect Corn Ammo")]
    void TestCollectCornAmmo()
    {
        OnAmmoCollected(Gun.GunType.Carrot);
    }

    [ContextMenu("Test Collect Meat Ammo")]
    void TestCollectMeatAmmo()
    {
        OnAmmoCollected(Gun.GunType.Meat);
    }

    [ContextMenu("Test Collect Vegetable Ammo")]
    void TestCollectVegetableAmmo()
    {
        OnAmmoCollected(Gun.GunType.Pepper);
    }

    [ContextMenu("Test Defeat Boss")]
    void TestDefeatBoss()
    {
        OnBossDefeated();
    }
}