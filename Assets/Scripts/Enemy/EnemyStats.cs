using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "FPS Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public string description = "Enemy description";

    [Header("Health & Defense")]
    public int maxHealth = 100;
    public float invulnerabilityTime = 0.1f; // 受伤后的无敌时间

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float chaseRange = 10f;        // 开始追踪玩家的距离
    public float loseChaseRange = 15f;    // 停止追踪玩家的距离
    public float stopRange = 2f;          // 停止移动开始攻击的距离
    public float keepChasingTime = 5f;    // 失去目标后继续追踪的时间

    [Header("Combat")]
    public int damageToPlayer = 25;
    public float attackRange = 8f;
    public float fireRate = 1f;           // 攻击间隔
    public float timeBetweenShots = 1f;   // 射击间等待时间
    public float shootingTime = 2f;       // 持续射击时间
    public float aimingAccuracy = 30f;    // 瞄准精度（角度范围）

    [Header("Projectile")]
    public GameObject bulletPrefab;       // 子弹预制体
    public float bulletSpeed = 10f;
    public int bulletDamage = 20;

    [Header("Behavior Type")]
    public EnemyType enemyType = EnemyType.Ranged;
    public bool canAutoFire = false;      // 是否可以连续射击

    [Header("Audio & Effects")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public GameObject deathEffect;        // 死亡特效
    public GameObject hitEffect;          // 受伤特效

    [Header("Rewards")]
    public int scoreValue = 100;          // 击杀得分
    public GameObject[] possibleDrops;    // 可能掉落的物品
    public float dropChance = 0.3f;       // 掉落概率
}

public enum EnemyType
{
    Melee,      // 近战型（冲向玩家）
    Ranged,     // 远程型（保持距离射击）
    Tank,       // 坦克型（高血量，慢速）
    Fast        // 快速型（低血量，高速）
}