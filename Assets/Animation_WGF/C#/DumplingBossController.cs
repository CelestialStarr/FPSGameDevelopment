using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossBehaviorController : MonoBehaviour
{
    public BossAnimationController anim;
    public Transform player;
    public NavMeshAgent agent;

    [Header("Stats")]
    public int maxHealth = 200;
    public int rollDamage = 30;

    [Header("Prefabs & FX")]
    public GameObject dumplingMinionPrefab;
    public GameObject healthPickupPrefab;
    public ParticleSystem rollDustEffect;

    [Header("Cooldowns")]
    public float summonCooldown = 8f;
    public float rollCooldown = 5f;

    [Header("Roll Detection")]
    public float rollHitRadius = 1.5f;       // 检测半径
    public LayerMask playerLayerMask;       // 把玩家放到专用 Layer，然后在 Inspector 里选中

    private int currentHealth;
    private bool isRolling, isSummoning;
    private float summonTimer, rollTimer;
    private bool bornFinished;

    void Start()
    {
        currentHealth = maxHealth;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        anim.PlayState(BossAnimationController.BossState.Born);
        StartCoroutine(StartAfter(anim.bornDuration));
    }

    IEnumerator StartAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        bornFinished = true;
        anim.PlayWalk();
    }

    void Update()
    {
        if (!bornFinished || isRolling || isSummoning) return;

        // 追踪玩家
        agent.SetDestination(player.position);

        // 冷却计时
        rollTimer += Time.deltaTime;
        summonTimer += Time.deltaTime;

        // 优先 Roll
        if (rollTimer >= rollCooldown)
        {
            rollTimer = 0f;
            StartCoroutine(DoRoll());
        }
        else if (summonTimer >= summonCooldown)
        {
            summonTimer = 0f;
            StartCoroutine(DoSummon());
        }
    }

    IEnumerator DoRoll()
    {
        isRolling = true;
        anim.PlayRoll();

        // 粉尘特效
        if (rollDustEffect != null)
            Instantiate(rollDustEffect, transform.position, Quaternion.identity).Play();

        // 暂时加速
        float oldSpeed = agent.speed;
        agent.speed = oldSpeed * 2f;

        // 等待直到半径检测到玩家
        while (true)
        {
            // Physics.CheckSphere 返回是否有符合 playerLayerMask 的碰撞体
            if (Physics.CheckSphere(transform.position, rollHitRadius, playerLayerMask))
            {
                // 命中玩家
                var ph = player.GetComponent<PlayerHealthController>();
                if (ph != null && !ph.IsDead())
                    ph.DamagePlayer(rollDamage);

                // 再来一次粉尘爆炸在玩家处
                if (rollDustEffect != null)
                    Instantiate(rollDustEffect, player.position, Quaternion.identity).Play();

                break;
            }
            // 持续追踪
            agent.SetDestination(player.position);
            yield return null;
        }

        // 恢复
        agent.speed = oldSpeed;
        isRolling = false;
        anim.PlayWalk();
    }

    IEnumerator DoSummon()
    {
        isSummoning = true;
        anim.PlaySummon();
        yield return new WaitForSeconds(anim.summonDuration);

        // 只召唤一个
        Instantiate(dumplingMinionPrefab, transform.position + transform.forward * 1.5f, Quaternion.identity);

        isSummoning = false;
        anim.PlayWalk();
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            anim.PlayState(BossAnimationController.BossState.Dead);
            anim.FadeOutAll();
            Destroy(gameObject, 2f);
        }
    }

    // 用于在 Scene 视图中可视化检测半径（可选）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red * 0.5f;
        Gizmos.DrawSphere(transform.position, rollHitRadius);
    }
}
