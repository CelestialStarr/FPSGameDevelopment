// ===== Updated Gun.cs =====
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName = "Gun";
    public enum GunType
    {
        Carrot,   // Fast firing, low damage
        Meat,     // Shotgun style, multiple projectiles
        Pepper    // Slow firing, high damage
    }
    public GunType gunType;

    [Header("Basic Settings")]
    public GameObject bullet;
    public bool canAutoFire;
    public float fireRate;
    [HideInInspector]
    public float fireCounter;

    [Header("Ammo Settings")]
    public int currentAmmo;
    public int maxAmmo;
    public int pickupAmount;

    [Header("Special Settings")]
    public bool isShotgun = false;  // For meat gun
    public int pelletCount = 5;     // How many bullets per shot
    public float spreadAngle = 15f; // Spread for shotgun

    void Start()
    {
        // Set up default values based on gun type
        switch (gunType)
        {
            case GunType.Carrot:
                weaponName = "Corn Launcher";  // Updated name to match ammo system
                if (fireRate == 0) fireRate = 0.2f;      // Fast
                if (maxAmmo == 0) maxAmmo = 150;
                if (currentAmmo == 0) currentAmmo = 50;
                if (pickupAmount == 0) pickupAmount = 30;
                canAutoFire = true;
                break;
            case GunType.Meat:
                weaponName = "Meat Blaster";     // Consistent naming
                if (fireRate == 0) fireRate = 1.0f;      // Slow
                if (maxAmmo == 0) maxAmmo = 40;
                if (currentAmmo == 0) currentAmmo = 20;
                if (pickupAmount == 0) pickupAmount = 10;
                canAutoFire = false;
                isShotgun = true;
                break;
            case GunType.Pepper:
                weaponName = "Vegetable Shooter";   // Updated name to match ammo system
                if (fireRate == 0) fireRate = 1.5f;      // Very slow
                if (maxAmmo == 0) maxAmmo = 30;
                if (currentAmmo == 0) currentAmmo = 10;
                if (pickupAmount == 0) pickupAmount = 5;
                canAutoFire = false;
                break;
        }
        Debug.Log($"Gun initialized: {weaponName}, Type: {gunType}");
    }

    void Update()
    {
        if (fireCounter > 0)
        {
            fireCounter -= Time.deltaTime;
        }
    }

    // Called when weapon is activated
    void OnEnable()
    {
        UpdateUI();
    }

    // Update UI display
    public void UpdateUI()
    {
        if (UIController.Instance != null)
        {
            // Update ammo count
            UIController.Instance.ammoText.text = "AMMO: " + currentAmmo;
            // Update weapon icon
            int weaponIndex = UIController.Instance.GetWeaponIndex(weaponName);
            UIController.Instance.UpdateWeaponDisplay(weaponName, weaponIndex);
        }
    }

    public void GetAmmo()
    {
        currentAmmo += pickupAmount;
        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
        // Update UI
        if (UIController.Instance != null)
        {
            UIController.Instance.ammoText.text = "AMMO: " + currentAmmo;
        }
    }

    // NEW: Method to configure bullet for ammo source damage
    public void ConfigureBullet(GameObject bulletInstance)
    {
        BullerController bulletScript = bulletInstance.GetComponent<BullerController>();
        if (bulletScript != null)
        {
            // Player bullets can damage enemies and ammo sources, but not player
            bulletScript.SetTargetType(false, true, true); // canHitPlayer, canHitEnemies, canHitAmmoSources
        }
    }
}
