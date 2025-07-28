// ===== Updated Knife.cs =====
using UnityEngine;
using System.Collections;

public class Knife : MonoBehaviour
{
    [Header("Knife Settings")]
    public string weaponName = "Kitchen Knife";
    public int damage = 50;
    public float attackRange = 2f;
    public float attackRate = 0.1f; // Can swing knife 2 times per second

    [HideInInspector]
    public float attackCounter;

    [Header("Attack Detection")]
    public Transform attackPoint;  // Knife attack point
    public LayerMask enemyLayer;   // Enemy layer

    [Header("Effects")]
    public GameObject slashEffect; // Slash effect
    public AudioClip slashSound;   // Slash sound

    private Animator knifeAnimator;
    private AudioSource audioSource;
    private SimpleKnifeAnimation knifeSwing;

    void Start()
    {
        knifeAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        knifeSwing = GetComponent<SimpleKnifeAnimation>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (attackCounter > 0)
        {
            attackCounter -= Time.deltaTime;
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
            // Knife has no ammo, display melee weapon
            UIController.Instance.ammoText.text = "MELEE WEAPON";

            // Update weapon icon
            int weaponIndex = UIController.Instance.GetWeaponIndex(weaponName);
            UIController.Instance.UpdateWeaponDisplay(weaponName, weaponIndex);
        }
    }

    public void Attack()
    {
        if (attackCounter <= 0)
        {
            // Play swing animation (using SimpleKnifeAnimation)
            SimpleKnifeAnimation knifeAnim = GetComponent<SimpleKnifeAnimation>();
            if (knifeAnim != null)
            {
                knifeAnim.PlayAnimation();
            }

            // Play Animator animation (if you have Animator)
            if (knifeAnimator != null)
            {
                knifeAnimator.SetTrigger("Attack");
            }

            // Play sound effect
            if (slashSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(slashSound);
            }

            // Detect enemies in attack range
            PerformAttack();

            // Reset attack timer
            attackCounter = attackRate;
        }
    }

    void PerformAttack()
    {
        // Detect enemies in front fan-shaped area
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // Check if in front (optional)
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);

            if (angle < 60f) // 120 degree fan range
            {
                // Deal damage
                EnemyHealthController enemyHealth = enemy.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    enemyHealth.DamageEnemy(damage);
                    Debug.Log($"Knife hit enemy for {damage} damage");
                }

                // Knockback effect (optional)
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 pushDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(pushDirection * 5f + Vector3.up * 2f, ForceMode.Impulse);
                }
            }
        }

        // NEW: Detect ammo sources in attack range
        Collider[] hitAmmoSources = Physics.OverlapSphere(attackPoint.position, attackRange);

        foreach (Collider hit in hitAmmoSources)
        {
            if (hit.CompareTag("AmmoSource"))
            {
                // Check if in front
                Vector3 dirToSource = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToSource);

                if (angle < 60f) // 120 degree fan range
                {
                    // Deal damage to ammo source
                    DestructibleAmmoSource ammoSource = hit.GetComponent<DestructibleAmmoSource>();
                    if (ammoSource != null && !ammoSource.IsDestroyed())
                    {
                        ammoSource.TakeDamage(damage);
                        Debug.Log($"Knife hit ammo source for {damage} damage");
                    }
                }
            }
        }

        // Generate slash effect
        if (slashEffect != null)
        {
            GameObject effect = Instantiate(slashEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 1f);
        }
    }

    // Visualize attack range (in editor)
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        // Draw attack direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(attackPoint.position, transform.forward * attackRange);
    }
}