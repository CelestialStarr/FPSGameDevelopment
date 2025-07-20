using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName = "Gun";  // ADD THIS LINE!

    //[Header("Weapon Type")]
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
                if (string.IsNullOrEmpty(weaponName)) weaponName = "Carrot Launcher";  // ADD THIS
                if (fireRate == 0) fireRate = 0.2f;      // Fast
                if (maxAmmo == 0) maxAmmo = 150;
                if (currentAmmo == 0) currentAmmo = 50;
                if (pickupAmount == 0) pickupAmount = 30;
                canAutoFire = true;
                break;

            case GunType.Meat:
                if (string.IsNullOrEmpty(weaponName)) weaponName = "Meat Blaster";     // ADD THIS
                if (fireRate == 0) fireRate = 1.0f;      // Slow
                if (maxAmmo == 0) maxAmmo = 40;
                if (currentAmmo == 0) currentAmmo = 20;
                if (pickupAmount == 0) pickupAmount = 10;
                canAutoFire = false;
                isShotgun = true;
                break;

            case GunType.Pepper:
                if (string.IsNullOrEmpty(weaponName)) weaponName = "Pepper Shooter";   // ADD THIS
                if (fireRate == 0) fireRate = 1.5f;      // Very slow
                if (maxAmmo == 0) maxAmmo = 30;
                if (currentAmmo == 0) currentAmmo = 10;
                if (pickupAmount == 0) pickupAmount = 5;
                canAutoFire = false;
                break;
        }
    }

    void Update()
    {
        if (fireCounter > 0)
        {
            fireCounter -= Time.deltaTime;
        }
    }

    public void GetAmmo()
    {
        currentAmmo += pickupAmount;
        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
        UIController.Instance.ammoText.text = "AMMO: " + currentAmmo;
    }
}