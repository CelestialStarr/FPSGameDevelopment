using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "FPS Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public string description = "Enemy description";

    [Header("Health & Defense")]
    public int maxHealth = 100;
    public float invulnerabilityTime = 0.1f; // ���˺���޵�ʱ��

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float chaseRange = 10f;        // ��ʼ׷����ҵľ���
    public float loseChaseRange = 15f;    // ֹͣ׷����ҵľ���
    public float stopRange = 2f;          // ֹͣ�ƶ���ʼ�����ľ���
    public float keepChasingTime = 5f;    // ʧȥĿ������׷�ٵ�ʱ��

    [Header("Combat")]
    public int damageToPlayer = 25;
    public float attackRange = 8f;
    public float fireRate = 1f;           // �������
    public float timeBetweenShots = 1f;   // �����ȴ�ʱ��
    public float shootingTime = 2f;       // �������ʱ��
    public float aimingAccuracy = 30f;    // ��׼���ȣ��Ƕȷ�Χ��

    [Header("Projectile")]
    public GameObject bulletPrefab;       // �ӵ�Ԥ����
    public float bulletSpeed = 10f;
    public int bulletDamage = 20;

    [Header("Behavior Type")]
    public EnemyType enemyType = EnemyType.Ranged;
    public bool canAutoFire = false;      // �Ƿ�����������

    [Header("Audio & Effects")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public GameObject deathEffect;        // ������Ч
    public GameObject hitEffect;          // ������Ч

    [Header("Rewards")]
    public int scoreValue = 100;          // ��ɱ�÷�
    public GameObject[] possibleDrops;    // ���ܵ������Ʒ
    public float dropChance = 0.3f;       // �������
}

public enum EnemyType
{
    Melee,      // ��ս�ͣ�������ң�
    Ranged,     // Զ���ͣ����־��������
    Tank,       // ̹���ͣ���Ѫ�������٣�
    Fast        // �����ͣ���Ѫ�������٣�
}