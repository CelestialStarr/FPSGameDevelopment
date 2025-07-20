using UnityEngine;
using System.Collections;

public class Knife : MonoBehaviour
{
    [Header("Knife Settings")]
    public string weaponName = "Kitchen Knife";
    public int damage = 50;
    public float attackRange = 2f;
    public float attackRate = 0.1f; // 每秒可以挥刀2次

    [HideInInspector]
    public float attackCounter;

    [Header("Attack Detection")]
    public Transform attackPoint;  // 刀的攻击点
    public LayerMask enemyLayer;   // 敌人层

    [Header("Effects")]
    public GameObject slashEffect; // 挥刀特效
    public AudioClip slashSound;   // 挥刀音效

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

    // 当武器被激活时调用
    void OnEnable()
    {
        UpdateUI();
    }

    // 更新UI显示
    public void UpdateUI()
    {
        if (UIController.Instance != null)
        {
            // 刀没有子弹，显示近战武器
            UIController.Instance.ammoText.text = "MELEE WEAPON";

            // 更新武器图标
            int weaponIndex = UIController.Instance.GetWeaponIndex(weaponName);
            UIController.Instance.UpdateWeaponDisplay(weaponName, weaponIndex);
        }
    }

    public void Attack()
    {
        if (attackCounter <= 0)
        {
            // 播放挥刀动画（使用SimpleKnifeAnimation）
            SimpleKnifeAnimation knifeAnim = GetComponent<SimpleKnifeAnimation>();
            if (knifeAnim != null)
            {
                knifeAnim.PlayAnimation();
            }

            // 播放Animator动画（如果你有Animator）
            if (knifeAnimator != null)
            {
                knifeAnimator.SetTrigger("Attack");
            }

            // 播放音效
            if (slashSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(slashSound);
            }

            // 检测攻击范围内的敌人
            PerformAttack();

            // 重置攻击计时器
            attackCounter = attackRate;
        }
    }

    void PerformAttack()
    {
        // 检测前方扇形区域内的敌人
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // 检查是否在前方（可选）
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);

            if (angle < 60f) // 120度扇形范围
            {
                // 造成伤害
                EnemyHealthController enemyHealth = enemy.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    enemyHealth.DamageEnemy(damage);
                }

                // 击退效果（可选）
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 pushDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(pushDirection * 5f + Vector3.up * 2f, ForceMode.Impulse);
                }
            }
        }

        // 生成挥刀特效
        if (slashEffect != null)
        {
            GameObject effect = Instantiate(slashEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 1f);
        }
    }

    // 可视化攻击范围（编辑器中）
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}