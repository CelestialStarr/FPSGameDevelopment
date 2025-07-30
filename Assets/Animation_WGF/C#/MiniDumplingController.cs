using UnityEngine;

public class MiniDumplingController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform player;
    public float moveSpeed = 2f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    [Tooltip("The point from which bullets spawn; usually an empty child at the front of the minion.")]
    public Transform firePoint;
    public float fireCooldown = 2f;
    public float bulletSpeed = 8f;
    public int bulletDamage = 10;
    private float lastFireTime;

    [Header("Health Settings")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Drop & Effects")]
    public GameObject healthPickupPrefab;    // ����Ļ�Ѫ���� Prefab
    public int healAmount = 10;              // ʰȡ��Ѫ��
    public ParticleSystem smokeEffect;       // ����ʱ��������Ч

    void Start()
    {
        currentHealth = maxHealth;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        // 1. �Զ�׷�����
        if (player)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }

        // 2. ��ʱ�����ӵ�
        if (Time.time - lastFireTime > fireCooldown && bulletPrefab != null && firePoint != null)
        {
            ShootBullet();
            lastFireTime = Time.time;
        }
    }

    void ShootBullet()
    {
        var b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (b.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        if (b.TryGetComponent<EnemyBullet>(out var eb))
            eb.damage = bulletDamage;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // 1. ����������Ч
        if (smokeEffect != null)
        {
            var fx = Instantiate(smokeEffect, transform.position, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        // 2. �����Ѫ����
        if (healthPickupPrefab != null)
        {
            var drop = Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
            // �����Ҫ��������ת���ɹ� FloatingRotatingPickup �ű�
            if (!drop.GetComponent<FloatingRotatingPickup>())
                drop.AddComponent<FloatingRotatingPickup>().healAmount = healAmount;
        }

        // 3. ��������
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            if (other.TryGetComponent<EnemyBullet>(out var eb))
            {
                TakeDamage(eb.damage);
                Destroy(other.gameObject);
            }
        }
    }
}
