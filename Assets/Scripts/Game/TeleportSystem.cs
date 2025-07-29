using UnityEngine;
using UnityEngine.UI;

public class KillBasedTeleportSystem : MonoBehaviour
{
    [Header("Kill Requirements")]
    public int requiredKills = 5;
    private int currentKills = 0;

    [Header("Teleport Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Teleport Settings")]
    public float teleportRange = 3f; // How close player needs to be to teleport point
    public float teleportDelay = 0.5f;

    [Header("Effects")]
    public GameObject teleportEffect;
    public AudioClip teleportSound;

    [Header("UI")]
    public GameObject killCountUI;
    public Text killCountText;
    public GameObject teleportPromptUI;
    public Text teleportPromptText;

    [Header("Story Dialog")]
    public GameObject storyDialogUI;
    public Text storyDialogText;
    public string[] storyMessages = {
        "Well done!",
        "You've cleared the area.",
        "Press T near a teleport point to travel."
    };

    private bool teleportUnlocked = false;
    private bool isNearTeleportPoint = false;
    private Transform nearestTeleportPoint;
    private AudioSource audioSource;

    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize UI
        UpdateKillCountUI();

        if (teleportPromptUI != null)
            teleportPromptUI.SetActive(false);

        if (storyDialogUI != null)
            storyDialogUI.SetActive(false);

        Debug.Log($"Teleport system initialized - Need {requiredKills} kills to unlock");
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

    public void OnEnemyKilled()
    {
        if (teleportUnlocked) return; // Already unlocked

        currentKills++;
        UpdateKillCountUI();

        Debug.Log($"Enemy killed! Count: {currentKills}/{requiredKills}");

        if (currentKills >= requiredKills)
        {
            UnlockTeleport();
        }
    }

    void UpdateKillCountUI()
    {
        if (killCountUI != null && killCountText != null)
        {
            if (!teleportUnlocked)
            {
                killCountUI.SetActive(true);
                killCountText.text = $"Enemies defeated: {currentKills}/{requiredKills}";
            }
            else
            {
                killCountUI.SetActive(false);
            }
        }
    }

    void UnlockTeleport()
    {
        teleportUnlocked = true;

        Debug.Log("Teleport system unlocked!");

        // Hide kill count UI
        UpdateKillCountUI();

        // Show story dialog
        StartCoroutine(ShowStoryDialog());
    }

    System.Collections.IEnumerator ShowStoryDialog()
    {
        if (storyDialogUI != null && storyMessages.Length > 0)
        {
            storyDialogUI.SetActive(true);

            for (int i = 0; i < storyMessages.Length; i++)
            {
                if (storyDialogText != null)
                {
                    storyDialogText.text = storyMessages[i];
                }

                yield return new WaitForSeconds(2f); // Show each message for 2 seconds
            }

            storyDialogUI.SetActive(false);
        }
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

    // Public methods for external control
    public void SetRequiredKills(int kills)
    {
        requiredKills = kills;
        UpdateKillCountUI();
    }

    public void ResetKillCount()
    {
        currentKills = 0;
        teleportUnlocked = false;
        UpdateKillCountUI();

        if (teleportPromptUI != null)
            teleportPromptUI.SetActive(false);
    }

    public bool IsTeleportUnlocked()
    {
        return teleportUnlocked;
    }

    public int GetCurrentKills()
    {
        return currentKills;
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