using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossBehaviorController : MonoBehaviour
{
    public BossAnimationController animationController;
    public Transform player;
    public float moveSpeed = 2f;
    public int maxHealth = 100;

    public GameObject dumplingMinionPrefab;
    public ParticleSystem summonEffect;
    public int minionCount = 3;
    public float summonRadius = 2f;

    private int currentHealth;
    private bool isRolling = false;
    private bool isSummoning = false;
    private bool rollHit = false;
    private NavMeshAgent agent;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;

        animationController.PlayState(BossAnimationController.BossState.Born);
        Invoke(nameof(StartWalk), 3f);
    }

    void StartWalk()
    {
        animationController.PlayState(BossAnimationController.BossState.Walk);
        StartCoroutine(SkillLoop());
    }

    void Update()
    {
        if ((isRolling || animationController.CurrentState == BossAnimationController.BossState.Walk) && player != null)
        {
            agent.speed = isRolling ? moveSpeed * 2f : moveSpeed;
            agent.SetDestination(player.position);
        }
    }

    IEnumerator SkillLoop()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(Random.Range(4f, 7f));
            if (!isRolling && !isSummoning)
            {
                if (Random.value < 0.5f)
                    StartCoroutine(DoRoll());
                else
                    StartCoroutine(DoSummon());
            }
        }
    }

    IEnumerator DoRoll()
    {
        isRolling = true;
        rollHit = false;
        animationController.PlayState(BossAnimationController.BossState.Roll);

        while (!rollHit)
        {
            yield return null;
        }

        isRolling = false;
        animationController.PlayState(BossAnimationController.BossState.Walk);
    }

    IEnumerator DoSummon()
    {
        isSummoning = true;
        animationController.PlayState(BossAnimationController.BossState.Summon);

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < minionCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-summonRadius, summonRadius), 0, Random.Range(-summonRadius, summonRadius));
            Instantiate(dumplingMinionPrefab, transform.position + offset, Quaternion.identity);
        }

        if (summonEffect != null)
            summonEffect.Play();

        yield return new WaitForSeconds(1f);
        isSummoning = false;
        animationController.PlayState(BossAnimationController.BossState.Walk);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isRolling && other.CompareTag("Player"))
        {
            rollHit = true;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            animationController.PlayState(BossAnimationController.BossState.Dead);
            animationController.FadeOutAll();
            Destroy(gameObject, 2f);
        }
    }
}
