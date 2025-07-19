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

        // ���Ŵ�����Ч
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(teleportDelay);

        // ȷ������Ŀ��
        Transform targetPoint = isAtPointA ? pointB : pointA;

        // �������
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
            player.transform.position = targetPoint.position;
            player.transform.rotation = targetPoint.rotation;
            charController.enabled = true;
        }

        // ��Ŀ��㲥����Ч
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

// NPC�������͵Ľű�
public class TeleportNPC : MonoBehaviour
{
    [Header("Dialog")]
    public string[] dialogLines = {
        "��ã����...",
        "��T�����Դ��͵�������һ��",
        "С���Ǳߵ�Σ�գ�"
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