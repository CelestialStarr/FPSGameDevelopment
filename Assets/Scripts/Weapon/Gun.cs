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
        // �ӳٳ�ʼ������ WeaponManager �ȼ�������
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
                if (currentAmmo == 0) currentAmmo = 50;  // ֻ��û�б�������ʱ����
                if (pickupAmount == 0) pickupAmount = 30;
                canAutoFire = true;
                break;
            case GunType.Meat:
                weaponName = "Meat Blaster";
                if (fireRate == 0) fireRate = 1.0f;      // Slow
                if (maxAmmo == 0) maxAmmo = 40;
                if (currentAmmo == 0) currentAmmo = 20;  // ֻ��û�б�������ʱ����
                if (pickupAmount == 0) pickupAmount = 10;
                canAutoFire = false;
                isShotgun = true;
                break;
            case GunType.Pepper:
                weaponName = "Pepper Shooter";
                if (fireRate == 0) fireRate = 1.5f;      // Very slow
                if (maxAmmo == 0) maxAmmo = 30;
                if (currentAmmo == 0) currentAmmo = 10;  // ֻ��û�б�������ʱ����
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

    // ������������ʱ����
    void OnEnable()
    {
        // �ӳٸ���UI��ȷ�������������׼����
        Invoke("UpdateUI", 0.1f);
    }

    // ����UI��ʾ
    public void UpdateUI()
    {
        if (UIController.Instance != null)
        {
            // �����ӵ�����
            if (UIController.Instance.ammoText != null)
            {
                UIController.Instance.ammoText.text = "AMMO: " + currentAmmo;
            }

            // ��������ͼ��
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

        // ����UI
        UpdateUI();

        Debug.Log($"{weaponName} ammo collected. Current: {currentAmmo}/{maxAmmo}");
    }

    // �������
    public void Shoot()
    {
        if (fireCounter <= 0 && currentAmmo > 0)
        {
            if (isShotgun)
            {
                // ����ǹģʽ - �������ӵ�
                ShootShotgun();
            }
            else
            {
                // ��ͨģʽ - ���䵥���ӵ�
                ShootSingle();
            }

            currentAmmo--;
            fireCounter = fireRate;

            // ����UI
            UpdateUI();
        }
    }

    void ShootSingle()
    {
        if (bullet != null)
        {
            GameObject newBullet = Instantiate(bullet, transform.position, transform.rotation);
            // �����������ӵ���ʼ���߼�
        }
    }

    void ShootShotgun()
    {
        if (bullet != null)
        {
            for (int i = 0; i < pelletCount; i++)
            {
                // ����ɢ��Ƕ�
                float angle = Random.Range(-spreadAngle, spreadAngle);
                Quaternion spreadRotation = transform.rotation * Quaternion.Euler(0, angle, 0);

                GameObject newBullet = Instantiate(bullet, transform.position, spreadRotation);
                // �����������ӵ���ʼ���߼�
            }
        }
    }

    // ���õ�ҩ�����ڴӱ������ݼ��أ�
    public void SetAmmo(int newCurrentAmmo, int newMaxAmmo)
    {
        currentAmmo = newCurrentAmmo;
        maxAmmo = newMaxAmmo;
        Debug.Log($"{weaponName} ammo set to: {currentAmmo}/{maxAmmo}");
    }

    // ����Ƿ�������
    public bool CanShoot()
    {
        return fireCounter <= 0 && currentAmmo > 0;
    }

    // ��ȡ��ҩ��Ϣ
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    // ����ΪĬ�ϵ�ҩ������Ϸʱ��
    public void ResetToDefault()
    {
        hasBeenInitialized = false;
        InitializeGun();
    }
}