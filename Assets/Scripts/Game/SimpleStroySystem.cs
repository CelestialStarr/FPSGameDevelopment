using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleStorySystem : MonoBehaviour
{
    public static SimpleStorySystem Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Text characterNameText;
    public AudioSource audioSource;

    [Header("Visual Effects")]
    public float typewriterSpeed = 0.05f;
    public AudioClip textBeepSound;

    [Header("Time-Based Teleport Unlock Settings")]
    [SerializeField] private float teleportUnlockTime = 30f; // 默认30秒后解锁传送

    [Header("Current Scene")]
    public SceneType currentScene = SceneType.Level1;

    // 私有变量
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private int currentDialogueIndex = 0;
    private string currentFullText = "";
    private Coroutine typewriterCoroutine;
    private List<DialogueEntry> currentDialogueList;
    private System.Action onDialogueComplete;

    // 跨场景数据存储到PlayerPrefs（更稳定）
    private bool teleportUnlocked
    {
        get { return PlayerPrefs.GetInt("StorySystem_TeleportUnlocked", 0) == 1; }
        set { PlayerPrefs.SetInt("StorySystem_TeleportUnlocked", value ? 1 : 0); }
    }

    private bool teleportDialogueShown = false;

    void Awake()
    {
        // 每个场景都是独立的Instance
        Instance = this;
    }

    void Start()
    {
        // 初始化UI
        dialoguePanel.SetActive(false);

        // 设置音效
        if (audioSource && textBeepSound)
            audioSource.clip = textBeepSound;

        // 播放场景开始剧情
        switch (currentScene)
        {
            case SceneType.Level1:
                PlayLevel1StartStory();
                // 启动传送解锁计时器
                StartCoroutine(TeleportUnlockTimer());
                break;
            case SceneType.Level2:
                PlayLevel2StartStory();
                break;
            case SceneType.Level3:
                PlayLevel3StartStory();
                break;
            case SceneType.CutScene:
                PlayCutSceneStory();
                break;
        }
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                OnNextButtonClick();
            }
        }
    }

    // 传送解锁计时器
    private IEnumerator TeleportUnlockTimer()
    {
        if (teleportUnlocked || teleportDialogueShown) yield break;

        yield return new WaitForSeconds(teleportUnlockTime);

        if (!teleportDialogueShown && !isDialogueActive)
        {
            PlayTeleportUnlockStory();
        }
    }

    #region 所有剧情节点

    /// <summary>
    /// 第一关开始剧情
    /// </summary>
    public void PlayLevel1StartStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "You woke up?... We've been waiting for you for a long time."),
            new DialogueEntry("???", "The food world is in crisis. The ingredients are crazy and everything is polluted."),
            new DialogueEntry("???", "Now, go to the front and defeat those out-of-control ingredients."),
            new DialogueEntry("???", "Don't ask too much - you will find yourself in the battle.")
        };

        StartDialogue(dialogue);
    }

    /// <summary>
    /// 第一关中间 - 传送解锁剧情（基于时间触发）
    /// </summary>
    private void PlayTeleportUnlockStory()
    {
        if (teleportDialogueShown) return;

        teleportUnlocked = true;
        teleportDialogueShown = true;

        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "You are beginning to awaken... your abilities are returning."),
            new DialogueEntry("???", "Are you beginning to remember something? Your skills... are not like ordinary ingredients."),
            new DialogueEntry("", "[Teleportation function has been unlocked]"),
            new DialogueEntry("", "[Find teleport points in the area and press T to teleport between them]")
        };

        StartDialogue(dialogue, () => {
            Debug.Log("传送功能已解锁！");
            // 通知传送系统解锁
            if (SimpleTeleportSystem.Instance != null)
            {
                // 新的传送系统不需要解锁，直接可用
                Debug.Log("传送系统已经可用！");
            }
        });
    }

    /// <summary>
    /// 第一关结束剧情
    /// </summary>
    public void PlayLevel1EndStory()
    {
        var dialogue = new List<DialogueEntry>
        {
           new DialogueEntry("???", "Perhaps you were once some kind of special existence."),
           new DialogueEntry("???", "Focus your mind, I will guide you through the cracks in space."),
        };

        StartDialogue(dialogue);
    }

    /// <summary>
    /// 第二关开始剧情
    /// </summary>
    public void PlayLevel2StartStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "Here they come...dumpling skins."),
            new DialogueEntry("???", "Don't think they are just dough. Once they are formed, they will become extremely deadly."),
            new DialogueEntry("???", "However, I believe you can control this chaos - knock them down, fill them up, and make them work for you."),
        };

        if (teleportUnlocked)
        {
            dialogue.Add(new DialogueEntry("", "The teleport feature is available in this area."));
        }

        StartDialogue(dialogue);
    }

    /// <summary>
    /// 第二关结束剧情
    /// </summary>
    public void PlayLevel2EndStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "You're starting to adapt...even manipulate the ingredients."),
            new DialogueEntry("???", "But don't you find it strange? Why are you so familiar with everything in the kitchen?"),
            new DialogueEntry("???", "You're our hero...right?"),
        };

        StartDialogue(dialogue);
    }

    /// <summary>
    /// 第三关开始剧情
    /// </summary>
    public void PlayLevel3StartStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "You have finally come to this point..."),
            new DialogueEntry("???", "It...is the final test of your memory. Defeat it and you will know who you are."),
        };

        StartDialogue(dialogue);
    }

    /// <summary>
    /// 第三关Boss二阶段剧情
    /// </summary>
    public void PlayBossPhase2Story()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("BOSS", "You are not a \"hero\"..."),
            new DialogueEntry("BOSS", "You have always crawled out of the cracks in the garbage - \"it\"..."),
            new DialogueEntry("BOSS", "Do you really think that the food world needs you to save it?"),
            new DialogueEntry("BOSS", "We have always...been hunting you!!")
        };

        StartDialogue(dialogue);
    }

    /// <summary>
    /// 第三关结束剧情（跳转剧情关）
    /// </summary>
    public void PlayLevel3EndStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "You succeeded... but... is this really the answer you want?"),
            new DialogueEntry("???", "The truth... is deeper... you will remember it..."),
            new DialogueEntry("???", "But are you ready?"),
        };

        StartDialogue(dialogue, () => {
            Debug.Log("准备跳转到剧情关！");
            // 这里可以添加场景跳转逻辑
        });
    }

    public void PlayCutSceneStory()
    {
        var dialogue = new List<DialogueEntry>
        {
            new DialogueEntry("System", "Intruder identification completed."),
            new DialogueEntry("System", "Target: Periplaneta americana"),
            new DialogueEntry("???", "You are not food... you are cockroaches."),
            new DialogueEntry("???", "You kill food because you are their natural enemy."),
            new DialogueEntry("???", "All this is not a heroic epic... but an invasion."),
            new DialogueEntry("System", "The expulsion protocol is activated."),
        };

        StartDialogue(dialogue, () => {
            Debug.Log("准备播放视频");
            // 这里可以添加场景跳转逻辑
        });
    }
    #endregion

    #region 触发器和公共方法

    /// <summary>
    /// 播放自定义剧情
    /// </summary>
    public void PlayCustomStory(List<DialogueEntry> dialogue)
    {
        StartDialogue(dialogue);
    }

    /// <summary>
    /// 重置所有剧情数据（测试用）
    /// </summary>
    public void ResetStoryData()
    {
        PlayerPrefs.DeleteKey("StorySystem_TeleportUnlocked");
        teleportDialogueShown = false;
        Debug.Log("剧情数据已重置");
    }

    /// <summary>
    /// 手动触发传送解锁（测试用）
    /// </summary>
    [ContextMenu("Unlock Teleport Now")]
    public void UnlockTeleportNow()
    {
        if (!teleportDialogueShown && !isDialogueActive)
        {
            PlayTeleportUnlockStory();
        }
    }

    #endregion

    #region UI控制

    private void StartDialogue(List<DialogueEntry> dialogueEntries, System.Action onComplete = null)
    {
        currentDialogueList = dialogueEntries;
        onDialogueComplete = onComplete;
        isDialogueActive = true;
        currentDialogueIndex = 0;

        // 暂停游戏
        Time.timeScale = 0f;

        // 禁用玩家控制
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController)
            playerController.enabled = false;

        // 显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        dialoguePanel.SetActive(true);
        DisplayCurrentDialogue();
    }

    private void DisplayCurrentDialogue()
    {
        if (currentDialogueIndex >= currentDialogueList.Count)
        {
            EndDialogue();
            return;
        }

        var currentDialogue = currentDialogueList[currentDialogueIndex];
        characterNameText.text = currentDialogue.characterName;

        currentFullText = currentDialogue.text;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentFullText));
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogueText.text = fullText.Substring(0, i);

            if (audioSource && textBeepSound && i < fullText.Length)
            {
                audioSource.PlayOneShot(textBeepSound, 0.3f);
            }

            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        isTyping = false;
    }

    public void OnNextButtonClick()
    {
        if (isTyping)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            dialogueText.text = currentFullText;
            isTyping = false;
        }
        else
        {
            currentDialogueIndex++;
            DisplayCurrentDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        Time.timeScale = 1f;

        var playerController = FindObjectOfType<PlayerController>();
        if (playerController)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        onDialogueComplete?.Invoke();
    }

    #endregion
}

/// <summary>
/// 对话条目数据结构
/// </summary>
[System.Serializable]
public class DialogueEntry
{
    public string characterName;
    [TextArea(2, 4)]
    public string text;

    public DialogueEntry(string name, string content)
    {
        characterName = name;
        text = content;
    }
}

/// <summary>
/// 场景类型枚举
/// </summary>
public enum SceneType
{
    Level1,
    Level2,
    Level3,
    CutScene
}