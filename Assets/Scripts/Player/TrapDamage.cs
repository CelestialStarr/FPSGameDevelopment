using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [Header("Trap Type")]
    public TrapType trapType = TrapType.InstantKill;

    [Header("Damage Settings")]
    public int damage = 100;
    [Tooltip("For continuous damage - damage interval in seconds")]
    public float damageInterval = 1f;
    [Tooltip("For one-time damage - delay before damage is applied")]
    public float damageDelay = 0f;

    [Header("Visual & Audio")]
    public GameObject deathEffect;
    public AudioClip trapSound;
    public bool showWarning = true;
    public Color warningColor = Color.red;

    [Header("Debug")]
    public bool enableDebugLog = true;

    public enum TrapType
    {
        InstantKill,        // Immediate death (knife, spike, etc.)
        ContinuousDamage,   // Damage over time while in trap (fire, acid, etc.)
        OneTimeDamage       // Single damage when entering (explosion, etc.)
    }

    private float nextDamageTime = 0f;
    private bool playerInTrap = false;
    private bool hasDealtDamage = false; // For one-time damage
    private AudioSource audioSource;

    void Start()
    {
        // Set trap tag
        if (!gameObject.CompareTag("Trap"))
        {
            gameObject.tag = "Trap";
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && trapSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Visual warning
        if (showWarning)
        {
            ApplyWarningVisual();
        }

        // Ensure trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (enableDebugLog)
        {
            Debug.Log($"Trap {gameObject.name} initialized - Type: {trapType}, Damage: {damage}");
        }
    }

    void ApplyWarningVisual()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create a new material instance to avoid affecting other objects
            Material warningMaterial = new Material(renderer.material);
            warningMaterial.color = warningColor;
            renderer.material = warningMaterial;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrap = true;
            hasDealtDamage = false; // Reset for one-time damage

            if (enableDebugLog)
            {
                Debug.Log($"Player entered trap: {gameObject.name} (Type: {trapType})");
            }

            // Play trap sound
            PlayTrapSound();

            switch (trapType)
            {
                case TrapType.InstantKill:
                    InstantKillPlayer();
                    break;

                case TrapType.OneTimeDamage:
                    if (damageDelay > 0)
                    {
                        Invoke(nameof(DealOneTimeDamage), damageDelay);
                    }
                    else
                    {
                        DealOneTimeDamage();
                    }
                    break;

                case TrapType.ContinuousDamage:
                    // Deal first damage immediately
                    DealContinuousDamage();
                    nextDamageTime = Time.time + damageInterval;
                    break;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && trapType == TrapType.ContinuousDamage && playerInTrap)
        {
            if (Time.time >= nextDamageTime)
            {
                DealContinuousDamage();
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrap = false;

            if (enableDebugLog)
            {
                Debug.Log($"Player exited trap: {gameObject.name}");
            }

            // Cancel any pending one-time damage
            if (trapType == TrapType.OneTimeDamage)
            {
                CancelInvoke(nameof(DealOneTimeDamage));
            }
        }
    }

    void InstantKillPlayer()
    {
        if (PlayerHealthController.instance != null)
        {
            // Create death effect at player position
            if (deathEffect != null)
            {
                Instantiate(deathEffect, PlayerController.instance.transform.position, Quaternion.identity);
            }

            // Kill player
            PlayerHealthController.instance.KillPlayer();

            if (enableDebugLog)
            {
                Debug.Log($"Player instantly killed by {gameObject.name}");
            }
        }
    }

    void DealOneTimeDamage()
    {
        if (playerInTrap && !hasDealtDamage && PlayerHealthController.instance != null)
        {
            hasDealtDamage = true;
            PlayerHealthController.instance.DamagePlayer(damage);

            if (enableDebugLog)
            {
                Debug.Log($"Player took {damage} one-time damage from {gameObject.name}");
            }

            // Create effect
            CreateDamageEffect();
        }
    }

    void DealContinuousDamage()
    {
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.DamagePlayer(damage);

            if (enableDebugLog)
            {
                Debug.Log($"Player took {damage} continuous damage from {gameObject.name}");
            }

            // Create effect
            CreateDamageEffect();
        }
    }

    void CreateDamageEffect()
    {
        if (deathEffect != null && PlayerController.instance != null)
        {
            GameObject effect = Instantiate(deathEffect, PlayerController.instance.transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up effect after 2 seconds
        }
    }

    void PlayTrapSound()
    {
        if (trapSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(trapSound);
        }
    }

    // Public methods for external control
    public void SetTrapType(TrapType newType)
    {
        trapType = newType;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetDamageInterval(float newInterval)
    {
        damageInterval = newInterval;
    }

    public void ActivateTrap()
    {
        enabled = true;
        GetComponent<Collider>().enabled = true;
    }

    public void DeactivateTrap()
    {
        enabled = false;
        GetComponent<Collider>().enabled = false;
        playerInTrap = false;
    }

    // Debug visualization in Scene view
    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = trapType == TrapType.InstantKill ? Color.red :
                          trapType == TrapType.ContinuousDamage ? new Color(1f, 0.5f, 0f) : Color.yellow; // orange = new Color(1f, 0.5f, 0f)

            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}