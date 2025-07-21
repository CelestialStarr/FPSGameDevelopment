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

    private bool hasBeenInitialized = false;

    void Start()
    {
        // 延迟初始化，让 WeaponManager 先加载数据
        Invoke("InitializeGun", 0.1f);
    }

    void InitializeGun()
    {
        if (hasBeenInitialized) return;

        // Set up default values based on gun type (only if not loaded from save data)
        switch (gunType)
        {
            case GunType.Carrot:
                weaponName = "Carrot Launcher";
                if (fireRate == 0) fireRate = 0.2f;      // Fast
                if (maxAmmo == 0) maxAmmo = 150;
                if (currentAmmo == 0) currentAmmo = 50;  // 只在没有保存数据时设置
                if (pickupAmount == 0) pickupAmount = 30;
                canAutoFire = true;
                break;
            case GunType.Meat:
                weaponName = "Meat Blaster";
                if (fireRate == 0) fireRate = 1.0f;      // Slow
                if (maxAmmo == 0) maxAmmo = 40;
                if (currentAmmo == 0) currentAmmo = 20;  // 只在没有保存数据时设置
                if (pickupAmount == 0) pickupAmount = 10;
                canAutoFire = false;
                isShotgun = true;
                break;
            case GunType.Pepper:
                weaponName = "Pepper Shooter";
                if (fireRate == 0) fireRate = 1.5f;      // Very slow
                if (maxAmmo == 0) maxAmmo = 30;
                if (currentAmmo == 0) currentAmmo = 10;  // 只在没有保存数据时设置
                if (pickupAmount == 0) pickupAmount = 5;
                canAutoFire = false;
                break;
        }

        hasBeenInitialized = true;
        Debug.Log($"Gun initialized: {weaponName}, Type: {gunType}, Ammo: {currentAmmo}/{maxAmmo}");
    }

    void Update()
    {
        if (fireCounter > 0)
        {
            fireCounter -= Time.deltaTime;
        }
    }

    // 当武器被激活时调用
    void OnEnable()
    {
        // 延迟更新UI，确保所有组件都已准备好
        Invoke("UpdateUI", 0.1f);
    }

    // 更新UI显示
    public void UpdateUI()
    {
        if (UIController.Instance != null)
        {
            // 更新子弹数量
            if (UIController.Instance.ammoText != null)
            {
                UIController.Instance.ammoText.text = "AMMO: " + currentAmmo;
            }

            // 更新武器图标
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

        // 更新UI
        UpdateUI();

        Debug.Log($"{weaponName} ammo collected. Current: {currentAmmo}/{maxAmmo}");
    }

    // 射击方法
    public void Shoot()
    {
        if (fireCounter <= 0 && currentAmmo > 0)
        {
            if (isShotgun)
            {
                // 霰弹枪模式 - 发射多个子弹
                ShootShotgun();
            }
            else
            {
                // 普通模式 - 发射单个子弹
                ShootSingle();
            }

            currentAmmo--;
            fireCounter = fireRate;

            // 更新UI
            UpdateUI();
        }
    }

    void ShootSingle()
    {
        if (bullet != null)
        {
            GameObject newBullet = Instantiate(bullet, transform.position, transform.rotation);
            // 这里可以添加子弹初始化逻辑
        }
    }

    void ShootShotgun()
    {
        if (bullet != null)
        {
            for (int i = 0; i < pelletCount; i++)
            {
                // 计算散射角度
                float angle = Random.Range(-spreadAngle, spreadAngle);
                Quaternion spreadRotation = transform.rotation * Quaternion.Euler(0, angle, 0);

                GameObject newBullet = Instantiate(bullet, transform.position, spreadRotation);
                // 这里可以添加子弹初始化逻辑
            }
        }
    }

    // 设置弹药（用于从保存数据加载）
    public void SetAmmo(int newCurrentAmmo, int newMaxAmmo)
    {
        currentAmmo = newCurrentAmmo;
        maxAmmo = newMaxAmmo;
        Debug.Log($"{weaponName} ammo set to: {currentAmmo}/{maxAmmo}");
    }

    // 检查是否可以射击
    public bool CanShoot()
    {
        return fireCounter <= 0 && currentAmmo > 0;
    }

    // 获取弹药信息
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    // 重置为默认弹药（新游戏时）
    public void ResetToDefault()
    {
        hasBeenInitialized = false;
        InitializeGun();
    }
}