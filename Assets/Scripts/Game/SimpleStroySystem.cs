using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

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

    [Header("Video Settings (CutScene Only)")]
    public VideoPlayer videoPlayer;
    public VideoClip videoClip;
    public RawImage videoDisplay; // 用于显示视频的UI
    public Canvas videoCanvas;    // 视频Canvas

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
            if (TeleportSystem.Instance != null)
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

    // 在你的SimpleStorySystem.cs中，修改PlayCutSceneStory方法：

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
            Debug.Log("剧情结束，准备播放视频");
            StartCoroutine(PlayVideoAfterDialogue());
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

    private IEnumerator PlayVideoAfterDialogue()
    {
        Debug.Log("=== 开始视频播放调试 ===");

        if (videoPlayer == null || videoClip == null)
        {
            Debug.LogError("videoPlayer 或 videoClip 为空！");
            SceneManager.LoadScene("GameOver");
            yield break;
        }

        // 隐藏所有UI
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            canvas.gameObject.SetActive(false);
        }

        Camera mainCamera = Camera.main;

        // 修改相机设置
        CameraClearFlags originalClearFlags = mainCamera.clearFlags;
        Color originalBackgroundColor = mainCamera.backgroundColor;

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.black;

        // 创建VideoQuad
        GameObject videoQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        videoQuad.name = "VideoQuad";
        Destroy(videoQuad.GetComponent<Collider>());

        // 设置Quad位置
        float distance = 1f;
        videoQuad.transform.position = mainCamera.transform.position + mainCamera.transform.forward * distance;
        videoQuad.transform.rotation = mainCamera.transform.rotation;

        // 创建RenderTexture和材质
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.Create();

        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        Material videoMaterial = new Material(Shader.Find("Unlit/Texture"));
        videoMaterial.mainTexture = renderTexture;
        videoQuad.GetComponent<Renderer>().material = videoMaterial;

        // 准备视频以获取正确的尺寸信息
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log($"视频分辨率: {videoPlayer.width} x {videoPlayer.height}");
        Debug.Log($"屏幕分辨率: {Screen.width} x {Screen.height}");
        Debug.Log($"相机视野: {mainCamera.fieldOfView}");

        // **重要：根据视频实际比例和屏幕比例计算正确的Quad尺寸**
        float videoAspectRatio = (float)videoPlayer.width / videoPlayer.height;
        float screenAspectRatio = (float)Screen.width / Screen.height;

        Debug.Log($"视频宽高比: {videoAspectRatio}");
        Debug.Log($"屏幕宽高比: {screenAspectRatio}");

        // 计算填满屏幕需要的尺寸
        float screenHeight = 2.0f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float screenWidth = screenHeight * screenAspectRatio;

        float quadWidth, quadHeight;

        if (videoAspectRatio > screenAspectRatio)
        {
            // 视频比屏幕更宽，以宽度为准
            quadWidth = screenWidth;
            quadHeight = screenWidth / videoAspectRatio;
        }
        else
        {
            // 视频比屏幕更高，以高度为准
            quadHeight = screenHeight;
            quadWidth = screenHeight * videoAspectRatio;
        }

        videoQuad.transform.localScale = new Vector3(quadWidth, quadHeight, 1f);

        Debug.Log($"Quad最终尺寸: {quadWidth} x {quadHeight}");

        videoPlayer.Play();
        Debug.Log("视频开始播放");

        // 等待播放完成
        while (videoPlayer.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                videoPlayer.Stop();
                break;
            }
            yield return null;
        }

        // 恢复相机设置
        mainCamera.clearFlags = originalClearFlags;
        mainCamera.backgroundColor = originalBackgroundColor;

        // 清理
        Destroy(videoQuad);
        Destroy(videoMaterial);
        renderTexture.Release();

        SceneManager.LoadScene("GameOver");
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