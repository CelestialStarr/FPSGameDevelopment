using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("Movement")]
    public float moveSpeed, gravityModifier, jumpPower, runSpeed = 12f;
    public CharacterController charCon;
    public int jumpcount;
    public int Maxjump = 2;

    [Header("Camera")]
    public Transform camTrans;
    public float mouseSensitivity;
    public bool isAiming; // For CameraFollow access

    [Header("Weapons")]
    public Transform firePoint;
    public Gun[] allGuns;  // 0-2 are guns
    public Knife meleeKnife;  // ADD THIS: 3 is knife
    private int currentWeaponIndex = 0;  // Track current weapon (0-3)

    private Animator anim;
    private Vector3 moveInput;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        anim = GetComponent<Animator>();

        // Initialize first weapon
        if (allGuns.Length > 0)
        {
            SwitchToWeapon(0);  // CHANGED: Use SwitchToWeapon instead of SwitchToGun
        }
    }

    void Update()
    {
        // Movement code (unchanged)
        float yStore = moveInput.y;
        Vector3 vertMove = transform.forward * Input.GetAxis("Vertical");
        Vector3 horiMove = transform.right * Input.GetAxis("Horizontal");

        moveInput = vertMove + horiMove;
        moveInput.Normalize();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveInput = moveInput * runSpeed;
        }
        else
        {
            moveInput = moveInput * moveSpeed;
        }

        moveInput.y = yStore;
        moveInput.y += Physics.gravity.y * gravityModifier * Time.deltaTime;

        if (charCon.isGrounded)
        {
            jumpcount = 0;
            moveInput.y = 0f;
            moveInput.y += Physics.gravity.y * gravityModifier * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpcount < Maxjump)
        {
            moveInput.y = jumpPower;
            jumpcount++;
        }

        charCon.Move(moveInput * Time.deltaTime);

        // Rotation (unchanged)
        Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
        camTrans.rotation = Quaternion.Euler(camTrans.rotation.eulerAngles.x - mouseInput.y, camTrans.rotation.eulerAngles.y, camTrans.rotation.eulerAngles.z);

        // Handle weapon switching
        HandleWeaponSwitching();

        // CHANGED: Handle shooting OR melee attack based on current weapon
        if (currentWeaponIndex < 3)  // Using a gun
        {
            Gun activeGun = GetCurrentGun();
            if (activeGun != null)
            {
                // Shooting logic
                if (Input.GetMouseButtonDown(0) && activeGun.fireCounter <= 0)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, 500f))
                    {
                        firePoint.LookAt(hit.point);
                    }
                    else
                    {
                        firePoint.LookAt(camTrans.position + transform.forward * 30f);
                    }

                    FireShot();
                }

                if (Input.GetMouseButton(0) && activeGun.canAutoFire)
                {
                    if (activeGun.fireCounter <= 0)
                    {
                        FireShot();
                    }
                }
            }
        }
        else if (currentWeaponIndex == 3)  // Using knife
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (meleeKnife != null)
                {
                    meleeKnife.Attack();
                }
            }
        }

        anim.SetFloat("moveSpeed", moveInput.magnitude);

        // Aiming (unchanged)
        isAiming = Input.GetMouseButton(1);
    }

    // CHANGED: Updated to handle 4 weapons (3 guns + 1 knife)
    void HandleWeaponSwitching()
    {
        // Number keys 1, 2, 3, 4
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToWeapon(3);  // ADD THIS: Knife

        // Q/E switching
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int newIndex = currentWeaponIndex - 1;
            if (newIndex < 0) newIndex = 3;  // CHANGED: Loop back to knife (index 3)
            SwitchToWeapon(newIndex);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            int newIndex = currentWeaponIndex + 1;
            if (newIndex > 3) newIndex = 0;  // CHANGED: Loop back to first gun
            SwitchToWeapon(newIndex);
        }

        // Mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            int newIndex = currentWeaponIndex + 1;
            if (newIndex > 3) newIndex = 0;
            SwitchToWeapon(newIndex);
        }
        else if (scroll < 0f)
        {
            int newIndex = currentWeaponIndex - 1;
            if (newIndex < 0) newIndex = 3;
            SwitchToWeapon(newIndex);
        }
    }

    // CHANGED: Renamed and updated to handle both guns and knife
    void SwitchToWeapon(int index)
    {
        if (index < 0 || index > 3) return;

        // Deactivate all weapons
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        if (meleeKnife != null)
        {
            meleeKnife.gameObject.SetActive(false);
        }

        // Activate selected weapon
        currentWeaponIndex = index;

        if (index < 3)  // It's a gun
        {
            if (index < allGuns.Length)
            {
                allGuns[index].gameObject.SetActive(true);
            }
        }
        else if (index == 3)  // It's the knife
        {
            if (meleeKnife != null)
            {
                meleeKnife.gameObject.SetActive(true);
            }
        }

        // Update UI
        UpdateAmmoUI();
    }

    // Helper method to get current gun (unchanged)
    Gun GetCurrentGun()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < allGuns.Length)
        {
            return allGuns[currentWeaponIndex];
        }
        return null;
    }

    // FireShot method (unchanged)
    public void FireShot()
    {
        Gun activeGun = GetCurrentGun();
        if (activeGun == null) return;

        if (activeGun.currentAmmo > 0)
        {
            activeGun.currentAmmo--;

            // Check if it's a shotgun
            if (activeGun.isShotgun)
            {
                // Fire multiple pellets
                for (int i = 0; i < activeGun.pelletCount; i++)
                {
                    float spreadX = Random.Range(-activeGun.spreadAngle, activeGun.spreadAngle);
                    float spreadY = Random.Range(-activeGun.spreadAngle, activeGun.spreadAngle);
                    Quaternion spread = firePoint.rotation * Quaternion.Euler(spreadX, spreadY, 0);
                    Instantiate(activeGun.bullet, firePoint.position, spread);
                }
            }
            else
            {
                // Normal single bullet
                Instantiate(activeGun.bullet, firePoint.position, firePoint.rotation);
            }

            activeGun.fireCounter = activeGun.fireRate;
            UpdateAmmoUI();
        }
    }

    // CHANGED: Updated to handle knife UI
    void UpdateAmmoUI()
    {
        if (currentWeaponIndex < 3)  // It's a gun
        {
            Gun activeGun = GetCurrentGun();
            if (activeGun != null)
            {
                UIController.Instance.ammoText.text = "AMMO: " + activeGun.currentAmmo;
                UIController.Instance.UpdateWeaponDisplay(activeGun.weaponName, currentWeaponIndex);
            }
        }
        else if (currentWeaponIndex == 3)  // It's the knife
        {
            if (meleeKnife != null)
            {
                UIController.Instance.ammoText.text = "MELEE WEAPON";
                UIController.Instance.UpdateWeaponDisplay(meleeKnife.weaponName, 3);
            }
        }
    }

    // ADD THIS: Public method to get current weapon index (for UI)
    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }
}