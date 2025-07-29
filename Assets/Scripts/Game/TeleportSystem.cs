using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TeleportSystem : MonoBehaviour
{
    public static TeleportSystem Instance;

    [Header("Teleport Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Teleport Settings")]
    public float teleportRange = 8f; // How close player needs to be to teleport point
    public float teleportDelay = 0.5f;

    [Header("Effects")]
    public GameObject teleportEffect;
    public AudioClip teleportSound;

    [Header("UI")]
    public GameObject teleportPromptUI;
    public Text teleportPromptText;

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

        Debug.Log("Simple teleport system initialized - ready to use!");
    }

    void Update()
    {
        // 直接检查传送点，无需解锁
        CheckPlayerNearTeleportPoints();

        if (isNearTeleportPoint && Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(TeleportPlayer());
        }
    }

    void CheckPlayerNearTeleportPoints()
    {
        // 检查PlayerController是否存在
        if (PlayerController.instance == null)
        {
            isNearTeleportPoint = false;
            if (teleportPromptUI != null)
                teleportPromptUI.SetActive(false);
            return;
        }

        // 检查传送点是否存在
        if (pointA == null || pointB == null)
        {
            isNearTeleportPoint = false;
            nearestTeleportPoint = null;
            if (teleportPromptUI != null)
                teleportPromptUI.SetActive(false);
            return;
        }

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

            if (isNearTeleportPoint && teleportPromptText != null && nearestTeleportPoint != null)
            {
                string targetName = (nearestTeleportPoint == pointA) ? "Point B" : "Point A";
                teleportPromptText.text = $"Press T to teleport to {targetName}";
            }
        }
    }

    IEnumerator TeleportPlayer()
    {
        if (!isNearTeleportPoint || nearestTeleportPoint == null) yield break;
        if (PlayerController.instance == null) yield break;

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

        // 再次检查目标点是否存在
        if (targetPoint == null)
        {
            // 重新启用玩家控制
            if (PlayerController.instance != null)
                PlayerController.instance.enabled = true;
            yield break;
        }

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
        if (PlayerController.instance != null)
            PlayerController.instance.enabled = true;

        // Update UI (player is now near different point)
        CheckPlayerNearTeleportPoints();

        Debug.Log($"Player teleported to {targetPoint.name}");
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