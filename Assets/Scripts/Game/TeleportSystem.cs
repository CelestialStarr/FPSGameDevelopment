using UnityEngine;
using UnityEngine.UI;

public class KillBasedTeleportSystem : MonoBehaviour
{
    public static KillBasedTeleportSystem Instance;

    [Header("Teleport Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Teleport Settings")]
    public float teleportRange = 8f; // How close player needs to be to teleport point
    public float teleportDelay = 0.5f;

    [Header("Effects")]
    public GameObject teleportEffect;
    public AudioClip teleportSound;

    [Header("UI - Only Essential")]
    public GameObject teleportPromptUI;
    public Text teleportPromptText;

    private bool teleportUnlocked = false;
    private bool isNearTeleportPoint = false;
    private Transform nearestTeleportPoint;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize UI
        if (teleportPromptUI != null)
            teleportPromptUI.SetActive(false);

        Debug.Log("Simplified teleport system initialized");
    }

    void Update()
    {
        if (teleportUnlocked)
        {
            CheckPlayerNearTeleportPoints();

            if (isNearTeleportPoint && Input.GetKeyDown(KeyCode.T))
            {
                StartCoroutine(TeleportPlayer());
            }
        }
    }

    void CheckPlayerNearTeleportPoints()
    {
        if (PlayerController.instance == null) return;

        Vector3 playerPos = PlayerController.instance.transform.position;
        bool wasNear = isNearTeleportPoint;
        Transform previousNearest = nearestTeleportPoint;

        // Check distance to both points
        float distanceToA = Vector3.Distance(playerPos, pointA.position);
        float distanceToB = Vector3.Distance(playerPos, pointB.position);

        // Find nearest point within range
        if (distanceToA <= teleportRange && distanceToA <= distanceToB)
        {
            isNearTeleportPoint = true;
            nearestTeleportPoint = pointA;
        }
        else if (distanceToB <= teleportRange)
        {
            isNearTeleportPoint = true;
            nearestTeleportPoint = pointB;
        }
        else
        {
            isNearTeleportPoint = false;
            nearestTeleportPoint = null;
        }

        // Update UI if status changed
        if (wasNear != isNearTeleportPoint || previousNearest != nearestTeleportPoint)
        {
            UpdateTeleportPromptUI();
        }
    }

    void UpdateTeleportPromptUI()
    {
        if (teleportPromptUI != null)
        {
            teleportPromptUI.SetActive(isNearTeleportPoint);

            if (isNearTeleportPoint && teleportPromptText != null)
            {
                string targetName = (nearestTeleportPoint == pointA) ? "Point B" : "Point A";
                teleportPromptText.text = $"Press T to teleport to {targetName}";
            }
        }
    }

    // 公共方法：由故事系统调用来解锁传送
    public void UnlockTeleportFromStory()
    {
        teleportUnlocked = true;
        Debug.Log("Teleport unlocked by story system!");
    }

    System.Collections.IEnumerator TeleportPlayer()
    {
        if (!isNearTeleportPoint || nearestTeleportPoint == null) yield break;

        GameObject player = PlayerController.instance.gameObject;

        // Disable player control
        PlayerController.instance.enabled = false;

        // Play teleport effect at current position
        if (teleportEffect != null)
        {
            GameObject effect1 = Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
            Destroy(effect1, 3f);
        }

        // Play teleport sound
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        yield return new WaitForSeconds(teleportDelay);

        // Determine target point (teleport to the other point)
        Transform targetPoint = (nearestTeleportPoint == pointA) ? pointB : pointA;

        // Teleport player
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
            player.transform.position = targetPoint.position;
            player.transform.rotation = targetPoint.rotation;
            charController.enabled = true;
        }
        else
        {
            player.transform.position = targetPoint.position;
            player.transform.rotation = targetPoint.rotation;
        }

        // Play teleport effect at destination
        if (teleportEffect != null)
        {
            GameObject effect2 = Instantiate(teleportEffect, targetPoint.position, Quaternion.identity);
            Destroy(effect2, 3f);
        }

        // Re-enable player control
        PlayerController.instance.enabled = true;

        // Update UI (player is now near different point)
        CheckPlayerNearTeleportPoints();

        Debug.Log($"Player teleported from {nearestTeleportPoint.name} to {targetPoint.name}");
    }

    // 公共方法：检查传送是否已解锁
    public bool IsTeleportUnlocked()
    {
        return teleportUnlocked;
    }

    // 重置传送系统（测试用）
    public void ResetTeleportSystem()
    {
        teleportUnlocked = false;

        if (teleportPromptUI != null)
            teleportPromptUI.SetActive(false);

        Debug.Log("Teleport system reset");
    }

    // 手动解锁传送（测试用）
    [ContextMenu("Unlock Teleport")]
    public void ManualUnlockTeleport()
    {
        UnlockTeleportFromStory();
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (pointA != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, teleportRange);
            Gizmos.DrawWireCube(pointA.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }

        if (pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointB.position, teleportRange);
            Gizmos.DrawWireCube(pointB.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }

        // Draw line between points
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}