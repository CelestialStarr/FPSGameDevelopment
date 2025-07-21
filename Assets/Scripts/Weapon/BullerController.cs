using UnityEngine;

public class BullerController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float moveSpeed = 15f;
    public float lifeTime = 5f;
    public int damage = 20;

    [Header("Target Settings")]
    public bool damageEnemy = false;    // Can damage enemies (for player bullets)
    public bool damagePlayer = true;    // Can damage player (for enemy bullets)

    [Header("Effects")]
    public GameObject laserImpact;      // Impact effect prefab
    public AudioClip impactSound;       // Impact sound

    private Rigidbody rb;
    private bool hasHit = false;        // Prevent multiple hits

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Set bullet velocity
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * moveSpeed;
        }

        Debug.Log($"Bullet created - DamagePlayer: {damagePlayer}, DamageEnemy: {damageEnemy}");
    }

    void Update()
    {
        // Alternative movement if no Rigidbody
        if (rb == null)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        // Countdown lifetime
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            DestroyBullet();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return; // Prevent multiple triggers

        Debug.Log($"Bullet hit: {other.gameObject.name} (Tag: {other.tag})");

        bool shouldDestroy = false;

        // Check hit enemy
        if (other.CompareTag("Enemy") && damageEnemy)
        {
            Debug.Log("Bullet hit enemy!");
            EnemyHealthController enemyHealth = other.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                enemyHealth.DamageEnemy(damage);
                Debug.Log($"Enemy took {damage} damage");
            }
            shouldDestroy = true;
        }

        // Check hit enemy headshot
        if (other.CompareTag("headShot") && damageEnemy)
        {
            Debug.Log("Bullet hit enemy headshot!");
            EnemyHealthController enemyHealth = other.transform.parent.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                enemyHealth.DamageEnemy(damage * 2); // Double damage for headshot
                Debug.Log($"Enemy took {damage * 2} headshot damage");
            }
            shouldDestroy = true;
        }

        // Check hit player
        if (other.CompareTag("Player") && damagePlayer)
        {
            Debug.Log("Hit the player!!!");
            if (PlayerHealthController.instance != null)
            {
                PlayerHealthController.instance.DamagePlayer(damage);
                Debug.Log($"Player took {damage} damage from bullet");
            }
            else
            {
                Debug.LogError("PlayerHealthController.instance is null!");
            }
            shouldDestroy = true;
        }

        // Check hit environment
        if (!other.isTrigger && !other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            Debug.Log("Bullet hit environment");
            shouldDestroy = true;
        }

        // Destroy bullet and create impact
        if (shouldDestroy)
        {
            CreateImpactEffect();
            DestroyBullet();
        }
    }

    void CreateImpactEffect()
    {
        if (laserImpact != null)
        {
            // Calculate impact position slightly back from bullet position
            float offset = 0.1f;
            Vector3 impactPosition = transform.position - transform.forward * offset;

            // Create impact effect
            GameObject impact = Instantiate(laserImpact, impactPosition, transform.rotation);

            // Scale down the effect if it's too big
            impact.transform.localScale = Vector3.one * 0.5f; // Make it smaller

            // Destroy impact effect after some time
            Destroy(impact, 3f);

            Debug.Log("Impact effect created");
        }

        // Play impact sound
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }
    }

    void DestroyBullet()
    {
        hasHit = true;

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Destroy bullet
        Destroy(gameObject, 0.1f);
    }

    // Public methods to set bullet properties
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetTargetType(bool canHitPlayer, bool canHitEnemies)
    {
        damagePlayer = canHitPlayer;
        damageEnemy = canHitEnemies;
        Debug.Log($"Bullet target set - Player: {damagePlayer}, Enemy: {damageEnemy}");
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * moveSpeed;
        }
    }
}