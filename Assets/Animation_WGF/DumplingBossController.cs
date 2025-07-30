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

        // 计时
        rollTimer += Time.deltaTime;
        summonTimer += Time.deltaTime;

        // 技能触发优先级：Roll > Summon
        if (rollTimer >= rollCooldown)
        {
            rollTimer = 0;
            StartCoroutine(DoRoll());
        }
        else if (summonTimer >= summonCooldown)
        {
            summonTimer = 0;
            StartCoroutine(DoSummon());
        }
    }

    IEnumerator DoRoll()
    {
        isRolling = true;
        anim.PlayRoll();
        // 粉尘粒子
        if (rollDustEffect != null)
            Instantiate(rollDustEffect, transform.position, Quaternion.identity).Play();

        float startSpeed = agent.speed;
        agent.speed *= 2f;

        // 持续追踪直到撞到玩家
        while (Vector3.Distance(transform.position, player.position) > 1.5f)
        {
            agent.SetDestination(player.position);
            yield return null;
        }

        // 撞击伤害 + 爆粉粒子
        PlayerHealthController ph = player.GetComponent<PlayerHealthController>();
        if (ph != null && !ph.IsDead())
            ph.DamagePlayer(rollDamage);

        if (rollDustEffect != null)
            Instantiate(rollDustEffect, player.position, Quaternion.identity).Play();

        // 恢复
        agent.speed = startSpeed;
        isRolling = false;
        anim.PlayWalk();
    }

    IEnumerator DoSummon()
    {
        isSummoning = true;
        anim.PlaySummon();
        yield return new WaitForSeconds(anim.summonDuration);

        // 只召唤一个
        var minion = Instantiate(dumplingMinionPrefab, transform.position + transform.forward * 1.5f, Quaternion.identity);
        // 小饺子脚本里自行接收 player、healthPickupPrefab…

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
}
