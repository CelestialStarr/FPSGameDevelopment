using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 50;
    public int currentHealth;
    public float invulnerabilityTime = 0.1f; // ���˺���޵�ʱ��

    [Header("Death Settings")]
    public GameObject[] possibleDrops;      // ���ܵ������Ʒ���ӵ����ȣ�
    public float dropChance = 0.7f;         // �������
    public GameObject deathEffect;          // ������Ч
    public AudioClip hurtSound;             // ������Ч
    public AudioClip deathSound;            // ������Ч

    [Header("Visual Feedback")]
    public float hurtFlashDuration = 0.1f;  // ������˸ʱ��
    public Color hurtColor = Color.red;     // ����ʱ����ɫ

    // Private variables
    private float invulnerabilityCounter = 0f;
    private bool isDead = false;
    private AudioSource audioSource;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Material originalMaterial;

    // ���ó���ϵͳ
    private EnemySpawner parentSpawner;
    private WaveManager waveManager;

    void Start()
    {
        // ��ʼ��Ѫ��
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        // ��ȡ���
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
            originalColor = originalMaterial.color;
        }

        // ���ҳ���ϵͳ����
        waveManager = FindObjectOfType<WaveManager>();
        // parentSpawner �����ڳ���ʱ���ã�����ͨ��������ʽ��ȡ
    }

    void Update()
    {
        // �����޵�ʱ��
        if (invulnerabilityCounter > 0)
        {
            invulnerabilityCounter -= Time.deltaTime;
        }
    }

    public void DamageEnemy(int bulletDamage)
    {
        // ����Ƿ����޵�ʱ���ڻ�������
        if (invulnerabilityCounter > 0 || isDead)
        {
            return;
        }

        // �۳�Ѫ��
        currentHealth -= bulletDamage;

        // �����޵�ʱ��
        invulnerabilityCounter = invulnerabilityTime;

        // ����������Ч
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // �����Ӿ�����
        StartCoroutine(HurtFlash());

        Debug.Log($"{gameObject.name} took {bulletDamage} damage. Health: {currentHealth}/{maxHealth}");

        // ����Ƿ�����
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator HurtFlash()
    {
        if (enemyRenderer != null)
        {
            // ���
            enemyRenderer.material.color = hurtColor;

            // �ȴ�
            yield return new WaitForSeconds(hurtFlashDuration);

            // �ָ�ԭɫ
            if (enemyRenderer != null) // ȷ�����˻�û������
            {
                enemyRenderer.material.color = originalColor;
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        // ����������Ч
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // ����������Ч
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // ���������Ʒ
        HandleDrops();

        // ֪ͨ����ϵͳ
        NotifySpawnSystem();

        // ���õ����������������һС��ʱ�䣨������Ч���ţ�
        DisableEnemyComponents();

        // �ӳ�����
        Destroy(gameObject, 1f);
    }

    void HandleDrops()
    {
        if (possibleDrops.Length > 0 && Random.value < dropChance)
        {
            // ���ѡ�������Ʒ
            int randomIndex = Random.Range(0, possibleDrops.Length);
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;

            GameObject droppedItem = Instantiate(possibleDrops[randomIndex], dropPosition, Quaternion.identity);

            // ��������Ʒһ��С�������
            Rigidbody dropRb = droppedItem.GetComponent<Rigidbody>();
            if (dropRb != null)
            {
                Vector3 randomForce = new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(1f, 3f),
                    Random.Range(-2f, 2f)
                );
                dropRb.AddForce(randomForce, ForceMode.Impulse);
            }

            Debug.Log($"Dropped {droppedItem.name}");
        }
    }

    void NotifySpawnSystem()
    {
        // ֪ͨ���ι�������������
        if (waveManager != null)
        {
            waveManager.OnEnemyDeath();
        }

        // ֪ͨ��������������
        if (parentSpawner != null)
        {
            parentSpawner.OnEnemyDeath();
        }
        else
        {
            // ���û��ֱ�����ã������ҵ�����ĳ�����
            EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
            if (spawners.Length > 0)
            {
                // �������֪ͨ��һ���ҵ��ĳ�����
                spawners[0].OnEnemyDeath();
            }
        }
    }

    void DisableEnemyComponents()
    {
        // ����AI���ƶ�
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // ���õ���
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // ������ײ��������Trigger���ڵ�����Ʒ���ȣ�
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.enabled = false;
        }
    }

    // �������������ó���������
    public void SetParentSpawner(EnemySpawner spawner)
    {
        parentSpawner = spawner;
    }

    // �������������Ƶ��ˣ������Ҫ��
    public void HealEnemy(int healAmount)
    {
        if (!isDead)
        {
            currentHealth += healAmount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            Debug.Log($"{gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
        }
    }

    // ������������ȡѪ����Ϣ
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
