using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用剧情对话UI系统 - 逆转裁判风格
/// </summary>
public class StoryDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Text characterNameText;
    public Button nextButton;
    public Image characterPortrait;
    public AudioSource audioSource;

    [Header("Visual Effects")]
    public float typewriterSpeed = 0.05f;
    public Color highlightColor = Color.yellow;
    public AudioClip textBeepSound;
    public AudioClip pageFlipSound;

    [Header("Character Portraits")]
    public Sprite systemPortrait;
    public Sprite aiAssistantPortrait;
    public Sprite playerPortrait;

    private bool isTyping = false;
    private bool isDialogueActive = false;
    private int currentDialogueIndex = 0;
    private string currentFullText = "";
    private Coroutine typewriterCoroutine;
    private List<DialogueEntry> currentDialogueList;
    private System.Action onDialogueComplete;

    // 引用其他脚本
    private PlayerController playerController;

    void Start()
    {
        // 初始化UI
        dialoguePanel.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClick);

        // 获取PlayerController引用
        playerController = FindObjectOfType<PlayerController>();

        // 设置音效
        if (audioSource && textBeepSound)
            audioSource.clip = textBeepSound;
    }

    void Update()
    {
        // 如果对话激活，监听输入
        if (isDialogueActive)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnNextButtonClick();
            }
        }
    }

    /// <summary>
    /// 开始播放指定的剧情对话
    /// </summary>
    /// <param name="dialogueEntries">对话列表</param>
    /// <param name="onComplete">对话完成后的回调</param>
    public void StartDialogue(List<DialogueEntry> dialogueEntries, System.Action onComplete = null)
    {
        if (dialogueEntries == null || dialogueEntries.Count == 0)
        {
            Debug.LogWarning("对话列表为空！");
            return;
        }

        currentDialogueList = dialogueEntries;
        onDialogueComplete = onComplete;
        isDialogueActive = true;
        currentDialogueIndex = 0;

        // 暂停游戏
        Time.timeScale = 0f;

        // 禁用玩家控制
        if (playerController)
            playerController.enabled = false;

        // 显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 显示对话界面
        dialoguePanel.SetActive(true);

        // 显示第一句对话
        DisplayCurrentDialogue();
    }

    /// <summary>
    /// 显示当前对话
    /// </summary>
    private void DisplayCurrentDialogue()
    {
        if (currentDialogueIndex >= currentDialogueList.Count)
        {
            EndDialogue();
            return;
        }

        var currentDialogue = currentDialogueList[currentDialogueIndex];

        // 设置角色名称
        characterNameText.text = currentDialogue.characterName;

        // 设置角色头像
        if (characterPortrait && currentDialogue.characterPortrait)
        {
            characterPortrait.sprite = currentDialogue.characterPortrait;
            characterPortrait.gameObject.SetActive(true);
        }
        else
        {
            characterPortrait.gameObject.SetActive(false);
        }

        // 开始打字机效果
        currentFullText = ProcessTextEffects(currentDialogue.text);

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentFullText));
    }

    /// <summary>
    /// 处理文本特效
    /// </summary>
    private string ProcessTextEffects(string originalText)
    {
        string processedText = originalText;
        processedText = processedText.Replace("[highlight]", $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>");
        processedText = processedText.Replace("[/highlight]", "</color>");
        processedText = processedText.Replace("[bold]", "<b>");
        processedText = processedText.Replace("[/bold]", "</b>");
        processedText = processedText.Replace("[italic]", "<i>");
        processedText = processedText.Replace("[/italic]", "</i>");

        return processedText;
    }

    /// <summary>
    /// 打字机效果
    /// </summary>
    private IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogueText.text = fullText.Substring(0, i);

            // 播放打字音效
            if (audioSource && textBeepSound && i < fullText.Length)
            {
                audioSource.PlayOneShot(textBeepSound, 0.3f);
            }

            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// 继续按钮点击事件
    /// </summary>
    public void OnNextButtonClick()
    {
        if (isTyping)
        {
            // 如果正在打字，立即显示完整文本
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            dialogueText.text = currentFullText;
            isTyping = false;
        }
        else
        {
            // 播放翻页音效
            if (audioSource && pageFlipSound)
                audioSource.PlayOneShot(pageFlipSound);

            // 继续下一句对话
            currentDialogueIndex++;
            DisplayCurrentDialogue();
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        // 恢复游戏
        Time.timeScale = 1f;

        // 恢复玩家控制
        if (playerController)
            playerController.enabled = true;

        // 隐藏鼠标（FPS模式）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 执行完成回调
        onDialogueComplete?.Invoke();
    }
}

/// <summary>
/// 对话条目数据结构
/// </summary>
[System.Serializable]
public class DialogueEntry
{
    public string characterName;
    public Sprite characterPortrait;
    [TextArea(2, 4)]
    public string text;

    public DialogueEntry(string name, string content, Sprite portrait = null)
    {
        characterName = name;
        text = content;
        characterPortrait = portrait;
    }
}

/// <summary>
/// 游戏剧情管理器 - 管理所有剧情触发逻辑
/// </summary>
public class StoryManager : MonoBehaviour
{
    [Header("Story System")]
    public StoryDialogueUI dialogueUI;

    [Header("Story Triggers")]
    public int enemiesRequiredForTeleport = 10;
    public bool teleportUnlocked = false;

    // 击杀计数
    private int currentKills = 0;

    void Start()
    {
        // 如果没有手动分配，自动查找
        if (!dialogueUI)
            dialogueUI = FindObjectOfType<StoryDialogueUI>();
    }

    #region 击杀相关剧情

    /// <summary>
    /// 当敌人被杀死时调用
    /// </summary>
    public void OnEnemyKilled()
    {
        if (teleportUnlocked) return;

        currentKills++;
        Debug.Log($"敌人击杀数: {currentKills}/{enemiesRequiredForTeleport}");

        // 检查是否达到传送解锁条件
        if (currentKills >= enemiesRequiredForTeleport)
        {
            PlayTeleportUnlockStory();
        }
    }

    /// <summary>
    /// 播放传送解锁剧情
    /// </summary>
    private void PlayTeleportUnlockStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("系统", "检测到所有威胁已被清除...", dialogueUI.systemPortrait),
            new DialogueEntry("AI助手", "分析完成。[highlight]传送系统[/highlight]现在可以安全激活。", dialogueUI.aiAssistantPortrait),
            new DialogueEntry("系统", "传送装置已上线。按[bold]T键[/bold]即可使用传送功能。", dialogueUI.systemPortrait),
            new DialogueEntry("AI助手", "小心使用这项技术。传送消耗大量能源。", dialogueUI.aiAssistantPortrait),
            new DialogueEntry("系统", "[highlight]传送系统激活完成[/highlight]。祝你好运，特工。", dialogueUI.systemPortrait)
        };

        dialogueUI.StartDialogue(dialogue, OnTeleportUnlocked);
    }

    /// <summary>
    /// 传送解锁完成回调
    /// </summary>
    private void OnTeleportUnlocked()
    {
        teleportUnlocked = true;

        // 启用传送功能
        var teleportScript = FindObjectOfType<MonoBehaviour>(); // 替换为你的传送脚本类型
                                                                // teleportScript.UnlockTeleport(); // 调用你的传送脚本解锁方法

        Debug.Log("传送功能已解锁！");
    }

    #endregion

    #region 其他剧情触发器

    /// <summary>
    /// 游戏开始剧情
    /// </summary>
    public void PlayGameStartStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("系统", "欢迎回来，特工。", dialogueUI.systemPortrait),
            new DialogueEntry("AI助手", "任务简报：清除所有敌对目标。", dialogueUI.aiAssistantPortrait),
            new DialogueEntry("系统", "完成任务后将解锁[highlight]新功能[/highlight]。", dialogueUI.systemPortrait)
        };

        dialogueUI.StartDialogue(dialogue);
    }

    /// <summary>
    /// 捡到特殊道具剧情
    /// </summary>
    public void PlayItemFoundStory(string itemName)
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("系统", $"发现特殊道具：[highlight]{itemName}[/highlight]", dialogueUI.systemPortrait),
            new DialogueEntry("AI助手", "这个道具可能对你的任务有帮助。", dialogueUI.aiAssistantPortrait)
        };

        dialogueUI.StartDialogue(dialogue);
    }

    /// <summary>
    /// Boss战前剧情
    /// </summary>
    public void PlayPreBossStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("系统", "警告：检测到[bold]高危目标[/bold]接近。", dialogueUI.systemPortrait),
            new DialogueEntry("AI助手", "建议提高警戒等级。准备战斗。", dialogueUI.aiAssistantPortrait),
            new DialogueEntry("玩家", "收到。开始战斗。", dialogueUI.playerPortrait)
        };

        dialogueUI.StartDialogue(dialogue);
    }

    /// <summary>
    /// 游戏结束剧情
    /// </summary>
    public void PlayGameEndStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("系统", "任务完成。所有目标已清除。", dialogueUI.systemPortrait),
            new DialogueEntry("AI助手", "干得好，特工。准备撤离。", dialogueUI.aiAssistantPortrait),
            new DialogueEntry("系统", "[highlight]任务成功[/highlight]。感谢你的服务。", dialogueUI.systemPortrait)
        };

        dialogueUI.StartDialogue(dialogue, () => {
            // 游戏结束逻辑
            Debug.Log("游戏结束！");
        });
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 播放自定义剧情
    /// </summary>
    public void PlayCustomStory(List<DialogueEntry> customDialogue, System.Action onComplete = null)
    {
        dialogueUI.StartDialogue(customDialogue, onComplete);
    }

    /// <summary>
    /// 获取当前击杀数
    /// </summary>
    public int GetCurrentKills()
    {
        return currentKills;
    }

    /// <summary>
    /// 重置击杀计数
    /// </summary>
    public void ResetKills()
    {
        currentKills = 0;
        teleportUnlocked = false;
    }

    #endregion
}