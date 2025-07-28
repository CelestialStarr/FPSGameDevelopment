// ===== DestructibleAmmoSource.cs =====
using UnityEngine;

public class DestructibleAmmoSource : MonoBehaviour
{
    [Header("=== Ammo Source Type ===")]
    [Tooltip("Type of ammo this source provides")]
    public Gun.GunType ammoSourceType = Gun.GunType.Carrot;

    [Header("=== Health Settings ===")]
    [Tooltip("Maximum health of this ammo source")]
    public int maxHealth = 50;

    [Tooltip("Current health (auto-set to max health at start)")]
    public int currentHealth;

    [Tooltip("Invulnerability time after taking damage")]
    public float invulnerabilityTime = 0.1f;

    [Header("=== Drop Settings ===")]
    [Tooltip("Ammo pickup prefabs that can be dropped")]
    public GameObject[] ammoPrefabs; // Array of different ammo pickup prefabs

    [Tooltip("Number of ammo pickups to drop when destroyed")]
    [Range(1, 5)]
    public int dropCount = 2;

    [Tooltip("Chance to drop ammo (0.0 = never, 1.0 = always)")]
    [Range(0f, 1f)]
    public float dropChance = 0.8f;

    [Tooltip("Force applied to dropped items")]
    public float dropForce = 5f;

    [Header("=== Visual Effects ===")]
    [Tooltip("Effect played when taking damage")]
    public GameObject damageEffect;

    [Tooltip("Effect played when destroyed")]
    public GameObject destroyEffect;

    [Tooltip("Color when taking damage")]
    public Color damageColor = Color.red;

    [Tooltip("Duration of damage flash")]
    public float flashDuration = 0.2f;

    [Header("=== Audio ===")]
    [Tooltip("Sound played when taking damage")]
    public AudioClip damageSound;

    [Tooltip("Sound played when destroyed")]
    public AudioClip destroySound;

    [Header("=== First Encounter UI ===")]
    [Tooltip("Message shown when player first approaches this type")]
    public string firstEncounterMessage = "Break & collect ammo!";

    [Tooltip("How long to show the first encounter message")]
    public float messageDisplayTime = 3f;

    [Tooltip("Detection distance for first encounter message")]
    public float detectionDistance = 5f;

    // Private variables
    private bool isDestroyed = false;
    private float invulnerabilityCounter = 0f;
    private Renderer objectRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private bool hasShownFirstEncounter = false;
    private bool playerNearby = false;

    // Static variable to track which types have been encountered
    private static bool[] encounteredTypes = new bool[System.Enum.GetValues(typeof(Gun.GunType)).Length];

    void Start()
    {
        // Initialize health
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        // Get components
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set visual appearance based on type
        SetVisualByType();

        Debug.Log($"[AmmoSource] {GetSourceTypeName()} source initialized with {maxHealth} health");
    }

    void Update()
    {
        // Update invulnerability timer
        if (invulnerabilityCounter > 0)
        {
            invulnerabilityCounter -= Time.deltaTime;
        }

        // Check for first encounter
        CheckFirstEncounter();
    }

    void SetVisualByType()
    {
        if (objectRenderer != null)
        {
            Material mat = objectRenderer.material;

            // Set color based on ammo source type
            switch (ammoSourceType)
            {
                case Gun.GunType.Carrot:
                    mat.color = Color.yellow; // Corn pile - yellow
                    break;
                case Gun.GunType.Meat:
                    mat.color = Color.red;    // Meat pile - red
                    break;
                case Gun.GunType.Pepper:
                    mat.color = Color.green;  // Vegetable pile - green
                    break;
            }

            originalColor = mat.color;
        }
    }

    void CheckFirstEncounter()
    {
        if (PlayerController.instance == null || isDestroyed) return;

        float distance = Vector3.Distance(transform.position, PlayerController.instance.transform.position);
        bool wasNearby = playerNearby;
        playerNearby = distance <= detectionDistance;

        // Show first encounter message
        if (playerNearby && !wasNearby && !hasShownFirstEncounter && !encounteredTypes[(int)ammoSourceType])
        {
            ShowFirstEncounterMessage();
            hasShownFirstEncounter = true;
            encounteredTypes[(int)ammoSourceType] = true;
        }
    }

    void ShowFirstEncounterMessage()
    {
        string typeName = GetSourceTypeName();
        string message = $"{typeName} Source: {firstEncounterMessage}";

        if (UIController.Instance != null)
        {
            UIController.Instance.ShowPickupHint(message);
            StartCoroutine(HideMessageAfterDelay(messageDisplayTime));
        }

        Debug.Log($"[AmmoSource] First encounter with {typeName} source!");
    }

    System.Collections.IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (UIController.Instance != null && !isDestroyed)
        {
            UIController.Instance.HidePickupHint();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed || invulnerabilityCounter > 0) return;

        currentHealth -= damage;
        invulnerabilityCounter = invulnerabilityTime;

        Debug.Log($"[AmmoSource] {GetSourceTypeName()} source took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Play damage effects
        PlayDamageEffects();

        // Visual feedback
        StartCoroutine(DamageFlash());

        // Check if destroyed
        if (currentHealth <= 0)
        {
            DestroySource();
        }
    }

    void PlayDamageEffects()
    {
        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Play damage effect
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (objectRenderer != null)
        {
            // Flash red
            objectRenderer.material.color = damageColor;

            yield return new WaitForSeconds(flashDuration);

            // Return to original color
            if (objectRenderer != null && !isDestroyed)
            {
                objectRenderer.material.color = originalColor;
            }
        }
    }

    void DestroySource()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        Debug.Log($"[AmmoSource] {GetSourceTypeName()} source destroyed!");

        // Play destroy effects
        PlayDestroyEffects();

        // Drop ammo
        DropAmmo();

        // Hide UI message if showing
        if (UIController.Instance != null)
        {
            UIController.Instance.HidePickupHint();
        }

        // Disable components but keep object for a moment (for sound/effects)
        DisableComponents();

        // Destroy after delay
        Destroy(gameObject, 2f);
    }

    void PlayDestroyEffects()
    {
        // Play destroy sound
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound);
        }

        // Play destroy effect
        if (destroyEffect != null)
        {
            GameObject effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
    }

    void DropAmmo()
    {
        if (Random.value > dropChance) return; // Check drop chance

        GameObject ammoPrefab = GetAmmoPrefabForType();
        if (ammoPrefab == null) return;

        for (int i = 0; i < dropCount; i++)
        {
            // Random position around the source
            Vector3 dropPosition = transform.position + new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-1f, 1f)
            );

            // Create ammo pickup
            GameObject droppedAmmo = Instantiate(ammoPrefab, dropPosition, Random.rotation);

            // Add random force
            Rigidbody rb = droppedAmmo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomForce = new Vector3(
                    Random.Range(-dropForce, dropForce),
                    Random.Range(dropForce * 0.5f, dropForce),
                    Random.Range(-dropForce, dropForce)
                );
                rb.AddForce(randomForce, ForceMode.Impulse);
            }
        }

        string typeName = GetSourceTypeName();
        Debug.Log($"[AmmoSource] Dropped {dropCount} {typeName} ammo pickups");
    }

    GameObject GetAmmoPrefabForType()
    {
        if (ammoPrefabs == null || ammoPrefabs.Length == 0) return null;

        // If only one prefab, use it
        if (ammoPrefabs.Length == 1) return ammoPrefabs[0];

        // Try to find matching type
        foreach (GameObject prefab in ammoPrefabs)
        {
            if (prefab != null)
            {
                AmmoPickup pickup = prefab.GetComponent<AmmoPickup>();
                if (pickup != null && pickup.ammoType == ammoSourceType)
                {
                    return prefab;
                }
            }
        }

        // Fallback to first available prefab
        return ammoPrefabs[0];
    }

    void DisableComponents()
    {
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Hide renderer
        if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }
    }

    string GetSourceTypeName()
    {
        switch (ammoSourceType)
        {
            case Gun.GunType.Carrot:
                return "Corn Pile";
            case Gun.GunType.Meat:
                return "Meat Pile";
            case Gun.GunType.Pepper:
                return "Vegetable Pile";
            default:
                return "Unknown";
        }
    }

    // Public methods for external access
    public bool IsDestroyed()
    {
        return isDestroyed;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public void HealSource(int healAmount)
    {
        if (!isDestroyed)
        {
            currentHealth += healAmount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            Debug.Log($"[AmmoSource] {GetSourceTypeName()} source healed {healAmount}. Health: {currentHealth}/{maxHealth}");
        }
    }

    // Editor helper
    void OnDrawGizmosSelected()
    {
        // Show detection range for first encounter
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        // Show health status
        if (!isDestroyed)
        {
            Gizmos.color = Color.green;
            float healthRatio = (float)currentHealth / maxHealth;
            Gizmos.DrawRay(transform.position + Vector3.up * 2f, Vector3.right * healthRatio * 2f);
        }
    }
}