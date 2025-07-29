using UnityEngine;
using System.Collections;

public class BossBehaviorController : MonoBehaviour
{
    public BossAnimationController animationController;
    public Transform player;
    public float moveSpeed = 2f;
    public int maxHealth = 100;
    public GameObject dumplingMinionPrefab;
    public ParticleSystem summonEffect;

    private int currentHealth;
    private bool isRolling = false;
    private bool isSummoning = false;
    private bool rollHit = false;

    private Rigidbody rb;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        rb = GetComponent<Rigidbody>();
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
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * (isRolling ? 2f : 1f) * Time.deltaTime;
        }
    }

    IEnumerator SkillLoop()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(Random.Range(4f, 7f));

            if (!isRolling && !isSummoning)
            {
                int skill = Random.Range(0, 2);
                if (skill == 0)
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

        // µÈ´ýÅö×²´¥·¢ rollHit = true
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

        if (dumplingMinionPrefab != null)
        {
            Instantiate(dumplingMinionPrefab, transform.position + transform.forward * 1.5f, Quaternion.identity);
        }

        if (summonEffect != null)
        {
            summonEffect.Play();
        }

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
            Destroy(gameObject, 2f);
        }
    }
}
