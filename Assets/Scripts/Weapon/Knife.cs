using UnityEngine;
using System.Collections;

public class Knife : MonoBehaviour
{
    [Header("Knife Settings")]
    public string weaponName = "Kitchen Knife";
    public int damage = 50;
    public float attackRange = 2f;
    public float attackRate = 0.1f; // ÿ����Իӵ�2��

    [HideInInspector]
    public float attackCounter;

    [Header("Attack Detection")]
    public Transform attackPoint;  // ���Ĺ�����
    public LayerMask enemyLayer;   // ���˲�

    [Header("Effects")]
    public GameObject slashEffect; // �ӵ���Ч
    public AudioClip slashSound;   // �ӵ���Ч

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

    // ������������ʱ����
    void OnEnable()
    {
        UpdateUI();
    }

    // ����UI��ʾ
    public void UpdateUI()
    {
        if (UIController.Instance != null)
        {
            // ��û���ӵ�����ʾ��ս����
            UIController.Instance.ammoText.text = "MELEE WEAPON";

            // ��������ͼ��
            int weaponIndex = UIController.Instance.GetWeaponIndex(weaponName);
            UIController.Instance.UpdateWeaponDisplay(weaponName, weaponIndex);
        }
    }

    public void Attack()
    {
        if (attackCounter <= 0)
        {
            // ���Żӵ�������ʹ��SimpleKnifeAnimation��
            SimpleKnifeAnimation knifeAnim = GetComponent<SimpleKnifeAnimation>();
            if (knifeAnim != null)
            {
                knifeAnim.PlayAnimation();
            }

            // ����Animator�������������Animator��
            if (knifeAnimator != null)
            {
                knifeAnimator.SetTrigger("Attack");
            }

            // ������Ч
            if (slashSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(slashSound);
            }

            // ��⹥����Χ�ڵĵ���
            PerformAttack();

            // ���ù�����ʱ��
            attackCounter = attackRate;
        }
    }

    void PerformAttack()
    {
        // ���ǰ�����������ڵĵ���
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // ����Ƿ���ǰ������ѡ��
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);

            if (angle < 60f) // 120�����η�Χ
            {
                // ����˺�
                EnemyHealthController enemyHealth = enemy.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    enemyHealth.DamageEnemy(damage);
                }

                // ����Ч������ѡ��
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 pushDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(pushDirection * 5f + Vector3.up * 2f, ForceMode.Impulse);
                }
            }
        }

        // ���ɻӵ���Ч
        if (slashEffect != null)
        {
            GameObject effect = Instantiate(slashEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 1f);
        }
    }

    // ���ӻ�������Χ���༭���У�
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}