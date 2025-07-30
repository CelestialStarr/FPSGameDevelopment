using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Main Dumpling Boss Controller
public class DumplingBoss : MonoBehaviour
{
    [Header("Basic Settings")]
    public int maxHealth = 200;
    public float moveSpeed = 3f;
    public float attackRange = 3f;
    public float detectionRange = 15f;

    [Header("Attack Settings")]
    public int chargeDamage = 30;
    public float chargeSpeed = 8f;
    public float chargeCooldown = 5f;
    public float chargeDistance = 10f;

    [Header("Summon Settings")]
    public GameObject minionPrefab;
    public int minionsToSummon = 2;
    public float summonCooldown = 8f;
    public Transform[] summonPoints; // Summon positions

    [Header("Visual Effects")]
    public ParticleSystem chargeEffect;
    public ParticleSystem summonEffect;
    public AudioSource audioSource;
    public AudioClip chargeSound;
    public AudioClip summonSound;

    // Private variables
    private int currentHealth;
    private Transform player;
    private NavMeshAgent navAgent;
    private Animator animator;
    private bool isDead = false;
    private bool isCharging = false;
    private bool isSummoning = false;
    private float lastChargeTime = 0f;
    private float lastSummonTime = 0f;

    // State enumeration
    public enum BossState
    {
        Idle,
        Chasing,
        Charging,
        Summoning,
        Dead
    }

    public BossState currentState = BossState.Idle;

    void Start()
    {
        // Initialize
        currentHealth = maxHealth;

        // Get components
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = attackRange * 0.8f;
        }

        // Find player
        FindPlayer();

        Debug.Log($"[DumplingBoss] Dumpling Boss initialized! Health: {maxHealth}");
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("[DumplingBoss] Player found!");
        }
        else
        {
            Debug.LogError("[DumplingBoss] Player not found! Make sure player has 'Player' tag");
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // State machine
        switch (currentState)
        {
            case BossState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(BossState.Chasing);
                }
                break;

            case BossState.Chasing:
                ChasePlayer(distanceToPlayer);
                CheckAttackOpportunity(distanceToPlayer);
                break;

            case BossState.Charging:
            case BossState.Summoning:
                // These states are controlled by coroutines
                break;
        }

        // Update animation parameters
        UpdateAnimations(distanceToPlayer);
    }

    void ChasePlayer(float distance)
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(player.position);
        }

        // If player runs too far, return to Idle state
        if (distance > detectionRange * 1.5f)
        {
            ChangeState(BossState.Idle);
        }
    }

    void CheckAttackOpportunity(float distance)
    {
        // Charge attack
        if (Time.time > lastChargeTime + chargeCooldown && distance > attackRange && distance < chargeDistance)
        {
            StartCoroutine(ChargeAttack());
        }
        // Summon attack
        else if (Time.time > lastSummonTime + summonCooldown && distance < attackRange * 2f)
        {
            StartCoroutine(SummonMinions());
        }
    }

    IEnumerator ChargeAttack()
    {
        ChangeState(BossState.Charging);
        isCharging = true;
        lastChargeTime = Time.time;

        Debug.Log("[DumplingBoss] Starting charge attack!");

        // Play sound and effects
        PlaySound(chargeSound);
        if (chargeEffect != null)
        {
            chargeEffect.Play();
        }

        // Pause navigation, manually control movement
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }

        // Calculate charge direction
        Vector3 chargeDirection = (player.position - transform.position).normalized;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + chargeDirection * chargeDistance;

        float chargeTime = chargeDistance / chargeSpeed;
        float elapsed = 0f;

        // Execute charge
        while (elapsed < chargeTime)
        {
            float progress = elapsed / chargeTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

            // Check if hit player
            if (Vector3.Distance(transform.position, player.position) < attackRange)
            {
                DamagePlayer(chargeDamage);
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore navigation
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }

        isCharging = false;
        ChangeState(BossState.Chasing);

        Debug.Log("[DumplingBoss] Charge attack finished!");
    }

    IEnumerator SummonMinions()
    {
        ChangeState(BossState.Summoning);
        isSummoning = true;
        lastSummonTime = Time.time;

        Debug.Log("[DumplingBoss] Starting minion summoning!");

        // Play sound and effects
        PlaySound(summonSound);
        if (summonEffect != null)
        {
            summonEffect.Play();
        }

        // Summoning animation delay
        yield return new WaitForSeconds(1f);

        // Summon minions
        for (int i = 0; i < minionsToSummon; i++)
        {
            if (minionPrefab != null)
            {
                Vector3 spawnPosition = GetSummonPosition(i);
                GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"[DumplingBoss] Summoned minion #{i + 1}");
            }

            yield return new WaitForSeconds(0.5f); // Staggered summoning
        }

        isSummoning = false;
        ChangeState(BossState.Chasing);

        Debug.Log("[DumplingBoss] Summoning complete!");
    }

    Vector3 GetSummonPosition(int index)
    {
        // If summon points are specified, use them
        if (summonPoints != null && summonPoints.Length > 0)
        {
            int pointIndex = index % summonPoints.Length;
            return summonPoints[pointIndex].position;
        }

        // Otherwise summon at random positions around boss
        float angle = (360f / minionsToSummon) * index;
        float radian = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radian) * 3f, 0f, Mathf.Sin(radian) * 3f);
        return transform.position + offset;
    }

    void DamagePlayer(int damage)
    {
        PlayerHealthController playerHealth = player.GetComponent<PlayerHealthController>();
        if (playerHealth != null && !playerHealth.IsDead())
        {
            playerHealth.DamagePlayer(damage);
            Debug.Log($"[DumplingBoss] Dealt {damage} damage to player!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"[DumplingBoss] Took {damage} damage! Remaining health: {currentHealth}/{maxHealth}");

        // Hit effect
        StartCoroutine(HitEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitEffect()
    {
        // Simple red flash hit effect
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color originalColor = Color.white;

        foreach (var renderer in renderers)
        {
            renderer.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var renderer in renderers)
        {
            renderer.material.color = originalColor;
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        ChangeState(BossState.Dead);

        Debug.Log("[DumplingBoss] Dumpling Boss died!");

        // Stop navigation
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }

        // Notify mission system
        if (MissionSystem.Instance != null)
        {
            MissionSystem.Instance.OnBossDefeated();
            Debug.Log("[DumplingBoss] Notified mission system of boss death");
        }

        // Death effects
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Death animation or effects
        yield return new WaitForSeconds(2f);

        // Optional: drop loot
        DropLoot();

        // Destroy boss
        Destroy(gameObject);
    }

    void DropLoot()
    {
        // Add loot dropping logic here
        Debug.Log("[DumplingBoss] Boss dropped loot!");
    }

    void ChangeState(BossState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log($"[DumplingBoss] State changed to: {newState}");
        }
    }

    void UpdateAnimations(float distanceToPlayer)
    {
        if (animator == null) return;

        // Update animation parameters
        animator.SetBool("IsMoving", navAgent != null && navAgent.velocity.magnitude > 0.1f);
        animator.SetBool("IsCharging", isCharging);
        animator.SetBool("IsSummoning", isSummoning);
        animator.SetBool("IsDead", isDead);
        animator.SetFloat("DistanceToPlayer", distanceToPlayer);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Gizmos for displaying ranges in Scene view
    void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Charge distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chargeDistance);
    }
}

// Boss Damage Receiver - Attach to boss to receive weapon damage
public class BossDamageReceiver : MonoBehaviour, IDamageable
{
    private DumplingBoss boss;

    void Start()
    {
        boss = GetComponent<DumplingBoss>();
        if (boss == null)
        {
            boss = GetComponentInParent<DumplingBoss>();
        }
    }

    public void TakeDamage(int damage)
    {
        if (boss != null)
        {
            boss.TakeDamage(damage);
        }
    }

    public bool IsDead()
    {
        return boss != null && boss.currentState == DumplingBoss.BossState.Dead;
    }
}

// Interface for your weapon system if needed
public interface IDamageable
{
    void TakeDamage(int damage);
    bool IsDead();
}