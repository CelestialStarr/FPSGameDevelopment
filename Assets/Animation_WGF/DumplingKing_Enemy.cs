using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LimbAnimation
{
    public string name;
    public Animator animator;
    public AnimationClip clip;
}

[System.Serializable]
public class AnimationState
{
    public string stateName;
    public List<LimbAnimation> limbAnimations;
}

public class DumplingKing_Enemy : MonoBehaviour
{
    [Header("动画状态集合（如 born, walk, roll, summon）")]
    public List<AnimationState> animations;

    [Header("非循环动画播放完后延迟（秒）")]
    public float animationDelay = 0.1f;

    [Header("初始状态")]
    public string initialState = "born";

    [Header("玩家对象")]
    public Transform player;

    [Header("追踪参数")]
    public float moveSpeed = 1.5f;

    [Header("技能触发控制")]
    public float skillCheckInterval = 5f;
    public float skillTriggerProbability = 0.4f;

    [Header("翻滚攻击设置")]
    public float rollSpeed = 6f;
    public float rollDuration = 1f;
    public float rollHitRadius = 1f;
    public int rollDamage = 10;

    [Header("召唤技能设置")]
    public GameObject dumplingMinionPrefab;   // 小饺子 prefab
    public Vector3 summonOffset = new Vector3(0, 0.5f, 1.5f); // 生成位置偏移

    private float skillCheckTimer = 0f;
    private bool bornPlayed = false;
    private bool isUsingSkill = false;
    private bool hasDealtDamageThisRoll = false;
    private string currentState = "";

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        StartCoroutine(PlayAnimationState(initialState, false, () =>
        {
            bornPlayed = true; // ✅ 播放完出生动画才开始移动
            StartCoroutine(PlayAnimationState("walk", true));
        }));
    }


    void Update()
    {
        if (!bornPlayed || player == null || isUsingSkill) return;

        // 直接看向玩家
        Vector3 targetPos = player.position;
        targetPos.y = transform.position.y; // 保持 y 不变，避免仰头低头
        transform.LookAt(targetPos);

        // 直接向前移动（按自身 forward 方向）
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // 技能触发检查
        skillCheckTimer += Time.deltaTime;
        if (skillCheckTimer >= skillCheckInterval)
        {
            skillCheckTimer = 0f;
            if (Random.value < skillTriggerProbability)
            {
                StartCoroutine(TriggerRandomSkill());
            }
        }
    }


    IEnumerator PlayAnimationState(string stateName, bool loop, System.Action onComplete = null)
    {
        AnimationState state = animations.Find(s => s.stateName == stateName);
        if (state == null)
        {
            Debug.LogWarning($"找不到动画状态：{stateName}");
            yield break;
        }

        currentState = stateName;

        foreach (var limb in state.limbAnimations)
        {
            if (limb.animator && limb.clip)
                limb.animator.Play(limb.clip.name);
        }

        if (!loop)
        {
            float duration = 0f;
            foreach (var limb in state.limbAnimations)
                if (limb.clip && limb.clip.length > duration)
                    duration = limb.clip.length;

            yield return new WaitForSeconds(duration + animationDelay);
            onComplete?.Invoke();
        }
    }

    IEnumerator TriggerRandomSkill()
    {
        isUsingSkill = true;

        int skillIndex = Random.Range(0, 2); // 0: roll, 1: summon

        if (skillIndex == 0)
        {
            yield return StartCoroutine(PlayAnimationState("roll", false, () =>
            {
                StartCoroutine(RollTowardPlayer());
            }));
        }
        else
        {
            yield return StartCoroutine(PlayAnimationState("summon", false, () =>
            {
                SummonMinion();
                StartCoroutine(PlayAnimationState("walk", true));
                isUsingSkill = false;
            }));
        }
    }

    IEnumerator RollTowardPlayer()
    {
        float timer = 0f;
        hasDealtDamageThisRoll = false;

        while (timer < rollDuration)
        {
            if (player == null) break;

            Vector3 dir = player.position - transform.position;
            Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
            if (flatDir.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(flatDir);
                transform.position += flatDir.normalized * rollSpeed * Time.deltaTime;
            }

            if (!hasDealtDamageThisRoll)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= rollHitRadius)
                {
                    hasDealtDamageThisRoll = true;
                    Debug.Log($"翻滚命中玩家！造成 {rollDamage} 点伤害！");
                    // player.GetComponent<PlayerHealth>()?.TakeDamage(rollDamage);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(PlayAnimationState("walk", true));
        isUsingSkill = false;
    }

    void SummonMinion()
    {
        if (dumplingMinionPrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.forward * summonOffset.z + Vector3.up * summonOffset.y;
            Instantiate(dumplingMinionPrefab, spawnPos, Quaternion.identity);
            Debug.Log("召唤小饺子！");
        }
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, player.position + Vector3.up * 0.5f);
        }
    }
}
