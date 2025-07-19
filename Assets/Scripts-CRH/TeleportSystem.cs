using UnityEngine;
using UnityEngine.UI;

public class TeleportSystem : MonoBehaviour
{
    [Header("Teleport Points")]
    public Transform pointA;
    public Transform pointB;
    public Transform currentPoint;

    [Header("UI")]
    public GameObject teleportPrompt;
    public Text promptText;
    public string npcMessage = "Press T to teleport";

    [Header("Effects")]
    public GameObject teleportEffect;
    public float teleportDelay = 0.5f;

    private bool canTeleport = false;
    private bool isAtPointA = true;

    void Start()
    {
        if (teleportPrompt != null)
        {
            teleportPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (canTeleport && Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(TeleportPlayer());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTeleport = true;
            ShowPrompt(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTeleport = false;
            ShowPrompt(false);
        }
    }

    void ShowPrompt(bool show)
    {
        if (teleportPrompt != null)
        {
            teleportPrompt.SetActive(show);
            if (promptText != null && show)
            {
                promptText.text = npcMessage;
            }
        }
    }

    System.Collections.IEnumerator TeleportPlayer()
    {
        GameObject player = PlayerController.instance.gameObject;
        PlayerController.instance.enabled = false;

        // 播放传送特效
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(teleportDelay);

        // 确定传送目标
        Transform targetPoint = isAtPointA ? pointB : pointA;

        // 传送玩家
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
            player.transform.position = targetPoint.position;
            player.transform.rotation = targetPoint.rotation;
            charController.enabled = true;
        }

        // 在目标点播放特效
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, targetPoint.position, Quaternion.identity);
        }

        PlayerController.instance.enabled = true;
        isAtPointA = !isAtPointA;

        ShowPrompt(false);
        canTeleport = false;
    }
}

// NPC触发传送的脚本
public class TeleportNPC : MonoBehaviour
{
    [Header("Dialog")]
    public string[] dialogLines = {
        "你好，蟑螂...",
        "按T键可以传送到厨房另一边",
        "小心那边的危险！"
    };

    public GameObject dialogUI;
    public Text dialogText;
    public float dialogSpeed = 0.05f;

    private bool isPlayerNear = false;
    private int currentLine = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            StartDialog();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            EndDialog();
        }
    }

    void StartDialog()
    {
        if (dialogUI != null)
        {
            dialogUI.SetActive(true);
            currentLine = 0;
            ShowNextLine();
        }
    }

    void ShowNextLine()
    {
        if (currentLine < dialogLines.Length)
        {
            StartCoroutine(TypeLine(dialogLines[currentLine]));
            currentLine++;
        }
        else
        {
            EndDialog();
        }
    }

    System.Collections.IEnumerator TypeLine(string line)
    {
        dialogText.text = "";
        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(dialogSpeed);
        }

        yield return new WaitForSeconds(1f);

        if (currentLine < dialogLines.Length - 1)
        {
            ShowNextLine();
        }
    }

    void EndDialog()
    {
        if (dialogUI != null)
        {
            dialogUI.SetActive(false);
        }
    }
}