using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("=== Ammo Type Settings ===")]
    public Gun.GunType ammoType = Gun.GunType.Carrot;

    [Header("=== Balance Configuration ===")]
    [Tooltip("Amount of carrot ammo given when picked up")]
    public int carrotAmmoAmount = 30;

    [Tooltip("Amount of meat ammo given when picked up")]
    public int meatAmmoAmount = 10;

    [Tooltip("Amount of pepper ammo given when picked up")]
    public int pepperAmmoAmount = 5;

    [Header("=== Advanced Settings ===")]
    [Tooltip("Use custom amount instead of preset values above")]
    public bool useCustomAmount = false;

    [Tooltip("Custom ammo amount (only works when Use Custom Amount is checked)")]
    public int customAmmoAmount = 0;

    [Header("=== Interaction Settings ===")]
    [Tooltip("Distance at which pickup hint appears")]
    [Range(1f, 10f)]
    public float detectionDistance = 3f;

    [Tooltip("How long pickup success message is displayed")]
    [Range(0.5f, 5f)]
    public float successMessageDuration = 1.5f;

    [Tooltip("How long error messages are displayed")]
    [Range(1f, 5f)]
    public float errorMessageDuration = 2f;

    [Header("=== Visual and Audio Effects ===")]
    [Tooltip("Particle effect prefab spawned when picked up")]
    public GameObject pickupEffect;

    [Tooltip("Sound played when ammo is picked up")]
    public AudioClip pickupSound;

    [Tooltip("Rotation speed of the ammo pickup")]
    [Range(0f, 200f)]
    public float rotationSpeed = 50f;

    [Header("=== Color Configuration ===")]
    [Tooltip("Color for carrot ammo pickups")]
    public Color carrotColor = Color.yellow;

    [Tooltip("Color for meat ammo pickups")]
    public Color meatColor = Color.red;

    [Tooltip("Color for pepper ammo pickups")]
    public Color pepperColor = Color.green;

    [Header("=== Debug Settings ===")]
    [Tooltip("Show detailed debug information in console")]
    public bool showDebugInfo = true;

    // Private variables
    private bool isPlayerNearby = false;
    private AudioSource audioSource;
    private int actualAmmoAmount;
    private Coroutine currentHintCoroutine; // Track current hint coroutine

    void Start()
    {
        // Setup audio component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Calculate actual ammo amount
        CalculateAmmoAmount();

        // Set visual appearance
        SetVisualByType();

        // Debug information
        if (showDebugInfo)
        {
            Debug.Log($"[AmmoPickup] {gameObject.name} initialized - Type: {GetAmmoTypeName()}, Amount: {actualAmmoAmount}");
        }
    }

    void CalculateAmmoAmount()
    {
        if (useCustomAmount)
        {
            actualAmmoAmount = customAmmoAmount;
            if (showDebugInfo)
            {
                Debug.Log($"[AmmoPickup] {gameObject.name} using custom amount: {actualAmmoAmount}");
            }
        }
        else
        {
            // Use preset values configured by designers
            switch (ammoType)
            {
                case Gun.GunType.Carrot:
                    actualAmmoAmount = carrotAmmoAmount;
                    break;
                case Gun.GunType.Meat:
                    actualAmmoAmount = meatAmmoAmount;
                    break;
                case Gun.GunType.Pepper:
                    actualAmmoAmount = pepperAmmoAmount;
                    break;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[AmmoPickup] {gameObject.name} using preset amount: {actualAmmoAmount}");
            }
        }
    }

    void SetVisualByType()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            // 创建材质实例，避免修改原始材质
            Material mat = new Material(renderer.sharedMaterial);
            renderer.material = mat; // 这会自动创建实例

            // Set color based on ammo type
            switch (ammoType)
            {
                case Gun.GunType.Carrot:
                    mat.color = carrotColor;
                    break;
                case Gun.GunType.Meat:
                    mat.color = meatColor;
                    break;
                case Gun.GunType.Pepper:
                    mat.color = pepperColor;
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"[AmmoPickup] {gameObject.name} 没有Renderer或材质！");
        }

        // Set Point Light color if exists
        Light pointLight = GetComponentInChildren<Light>();
        if (pointLight != null)
        {
            switch (ammoType)
            {
                case Gun.GunType.Carrot:
                    pointLight.color = carrotColor;
                    break;
                case Gun.GunType.Meat:
                    pointLight.color = meatColor;
                    break;
                case Gun.GunType.Pepper:
                    pointLight.color = pepperColor;
                    break;
            }
        }
    }

    void Update()
    {
        // Rotation animation
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // Check for pickup input
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            PickupAmmo();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            string ammoTypeName = GetAmmoTypeName();

            // Show pickup prompt
            if (UIController.Instance != null)
            {
                UIController.Instance.ShowPickupHint($"Press F to pickup {ammoTypeName} ammo +{actualAmmoAmount}");
            }

            if (showDebugInfo)
            {
                Debug.Log($"[AmmoPickup] Player approached {ammoTypeName} ammo pickup");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Hide pickup prompt
            if (UIController.Instance != null)
            {
                UIController.Instance.HidePickupHint();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[AmmoPickup] Player left {GetAmmoTypeName()} ammo pickup");
            }
        }
    }

    void PickupAmmo()
    {
        // Find the corresponding weapon type
        Gun targetGun = FindGunOfType(ammoType);

        if (targetGun != null)
        {
            // Record ammo before pickup
            int oldAmmo = targetGun.currentAmmo;
            int maxAmmo = targetGun.maxAmmo;

            // Calculate how much can actually be picked up
            int canPickup = Mathf.Min(actualAmmoAmount, maxAmmo - oldAmmo);

            if (canPickup > 0)
            {
                // Add ammo to the corresponding weapon
                targetGun.currentAmmo += canPickup;

                if (MissionSystem.Instance != null)
                {
                    MissionSystem.Instance.OnAmmoCollected(ammoType);
                }

                // Update UI if it's the currently equipped weapon
                UpdateCurrentWeaponUI(targetGun);

                string ammoTypeName = GetAmmoTypeName();

                if (showDebugInfo)
                {
                    Debug.Log($"[AmmoPickup] Successfully picked up {canPickup} {ammoTypeName} ammo! Current: {targetGun.currentAmmo}/{maxAmmo}");
                }

                // Play pickup effects
                PlayPickupEffects();

                // Show pickup success message
                if (UIController.Instance != null)
                {
                    UIController.Instance.ShowPickupHint($"Picked up {canPickup} {ammoTypeName} ammo!");
                    // Hide hint after delay
                    currentHintCoroutine = StartCoroutine(HideHintAfterDelay(successMessageDuration));
                }

                // Destroy the ammo pickup
                Destroy(gameObject);
            }
            else
            {
                // Ammo is full
                string ammoTypeName = GetAmmoTypeName();
                if (UIController.Instance != null)
                {
                    UIController.Instance.ShowPickupHint($"{ammoTypeName} ammo is full! ({oldAmmo}/{maxAmmo})");
                    currentHintCoroutine = StartCoroutine(HideHintAfterDelay(errorMessageDuration));
                }

                if (showDebugInfo)
                {
                    Debug.Log($"[AmmoPickup] {ammoTypeName} ammo is full, cannot pickup");
                }
            }
        }
        else
        {
            // Player doesn't have the corresponding weapon
            string ammoTypeName = GetAmmoTypeName();

            if (UIController.Instance != null)
            {
                UIController.Instance.ShowPickupHint($"You don't have a {ammoTypeName} weapon!");
                currentHintCoroutine = StartCoroutine(HideHintAfterDelay(errorMessageDuration));
            }

            if (showDebugInfo)
            {
                Debug.LogWarning($"[AmmoPickup] Player doesn't have {ammoTypeName} weapon, cannot pickup ammo");
            }
        }
    }

    void UpdateCurrentWeaponUI(Gun targetGun)
    {
        // Update UI through WeaponManager
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
        {
            int currentIndex = weaponManager.GetCurrentWeaponIndex();
            if (currentIndex < weaponManager.weapons.Length)
            {
                GameObject currentWeapon = weaponManager.weapons[currentIndex];
                Gun currentGun = currentWeapon.GetComponent<Gun>();

                if (currentGun == targetGun)
                {
                    targetGun.UpdateUI();
                }
            }
        }

        // Backup method: Update UI through PlayerController
        if (PlayerController.instance != null)
        {
            // Check through all guns to find active one
            Gun[] allGuns = PlayerController.instance.allGuns;
            if (allGuns != null)
            {
                foreach (Gun gun in allGuns)
                {
                    if (gun == targetGun && gun.gameObject.activeInHierarchy)
                    {
                        targetGun.UpdateUI();
                        break;
                    }
                }
            }
        }
    }

    Gun FindGunOfType(Gun.GunType targetType)
    {
        // Find weapon through WeaponManager
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
        {
            foreach (GameObject weapon in weaponManager.weapons)
            {
                if (weapon != null)
                {
                    Gun gun = weapon.GetComponent<Gun>();
                    if (gun != null && gun.gunType == targetType)
                    {
                        return gun;
                    }
                }
            }
        }

        // Backup method: Find through PlayerController
        if (PlayerController.instance != null && PlayerController.instance.allGuns != null)
        {
            foreach (Gun gun in PlayerController.instance.allGuns)
            {
                if (gun != null && gun.gunType == targetType)
                {
                    return gun;
                }
            }
        }

        return null;
    }

    string GetAmmoTypeName()
    {
        switch (ammoType)
        {
            case Gun.GunType.Carrot:
                return "Corn";     // 玉米
            case Gun.GunType.Meat:
                return "Meat";     // 肉
            case Gun.GunType.Pepper:
                return "Vegetable"; // 蔬菜
            default:
                return "Unknown";
        }
    }

    void PlayPickupEffects()
    {
        // Play pickup sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Play pickup effect
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    // Coroutine to hide hint after delay
    System.Collections.IEnumerator HideHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (UIController.Instance != null)
        {
            UIController.Instance.HidePickupHint();
        }
        currentHintCoroutine = null; // Clear the reference when done
    }

    // ===== Editor Helper Functions =====
    void OnValidate()
    {
        // Automatically recalculate when Inspector values change
        if (Application.isPlaying)
        {
            CalculateAmmoAmount();
            SetVisualByType();
        }
    }

    // Display information in Scene view
    void OnDrawGizmosSelected()
    {
        // Show trigger area (collider)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // Green for collider
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }

        // Show detection distance
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow for detection range
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        // Show detection distance label
        Gizmos.color = Color.yellow;
        UnityEngine.Vector3 labelPos = transform.position + Vector3.up * (detectionDistance + 0.5f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"Detection: {detectionDistance:F1}m");
#endif
    }
}