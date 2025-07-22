using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 每个肢体的动画信息（如身体、左手、右腿等）
[System.Serializable]
public class LimbAnimation
{
    public string name;
    public Animator animator;
    public AnimationClip clip;
}

// 一个完整的动画状态（如出生、走路、翻滚）包含所有肢体的动作
[System.Serializable]
public class AnimationState
{
    public string stateName;
    public List<LimbAnimation> limbAnimations;
}

public class DumplingKing_Enemy : MonoBehaviour
{
    [Header("动画状态集合（如出生、走路、翻滚）")]
    public List<AnimationState> animations;

    [Header("非循环动画后延迟播放下一个（秒）")]
    public float animationDelay = 0.1f;

    [Header("初始状态名称")]
    public string initialState = "born";

    [Header("玩家对象（用于追踪）")]
    public Transform player;

    [Header("追踪速度")]
    public float moveSpeed = 1.5f;

    [Header("翻滚攻击参数")]
    public float rollSpeed = 6f;
    public float rollDuration = 1f;
    public float rollHitRadius = 1f;
    public int rollDamage = 10;

    [Header("技能触发设置")]
    public float skillCheckInterval = 5f;
    public float skillTriggerProbability = 0.3f;

    private float skillCheckTimer = 0f;
    private bool bornPlayed = false;
    private bool isUsingSkill = false;
    private bool hasDealtDamageThisRoll = false;
    private string currentState = "";

    void Start()
    {
        // 自动寻找玩家（如果未赋值）
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        StartCoroutine(PlayAnimationState(initialState, false, () =>
        {
            bornPlayed = true;
            StartCoroutine(PlayAnimationState("walk", true));
        }));
    }

    void Update()
    {
        if (bornPlayed && player != null && !isUsingSkill)
        {
            // 追踪并靠近玩家
            Vector3 direction = player.position - transform.position;
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }

            // 技能触发检查
            skillCheckTimer += Time.deltaTime;
            if (skillCheckTimer >= skillCheckInterval)
            {
                skillCheckTimer = 0f;
                if (Random.value < skillTriggerProbability)
                {
                    StartCoroutine(TriggerRollSkill());
                }
            }
        }
    }

    /// <summary>
    /// 播放指定动画状态
    /// </summary>
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
            {
                limb.animator.Play(limb.clip.name);
            }
        }

        if (!loop)
        {
            float duration = 0f;
            foreach (var limb in state.limbAnimations)
            {
                if (limb.clip && limb.clip.length > duration)
                    duration = limb.clip.length;
            }

            yield return new WaitForSeconds(duration + animationDelay);
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 触发翻滚技能（动画 + 冲刺）
    /// </summary>
    IEnumerator TriggerRollSkill()
    {
        isUsingSkill = true;

        // 播放翻滚动画后 → 执行移动逻辑
        yield return StartCoroutine(PlayAnimationState("roll", false, () =>
        {
            StartCoroutine(RollTowardPlayer());
        }));
    }

    /// <summary>
    /// 翻滚冲刺向玩家方向，期间可造成一次伤害
    /// </summary>
    IEnumerator RollTowardPlayer()
    {
        float timer = 0f;
        hasDealtDamageThisRoll = false;

        while (timer < rollDuration)
        {
            if (player == null) break;

            // 动态追踪方向
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            direction.Normalize();

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            // 高速移动
            transform.position += direction * rollSpeed * Time.deltaTime;

            // 命中检测（只触发一次）
            if (!hasDealtDamageThisRoll)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= rollHitRadius)
                {
                    hasDealtDamageThisRoll = true;
                    Debug.Log($"翻滚命中玩家！造成 {rollDamage} 点伤害！");
                    // 可调用玩家受伤函数（你自己加）：
                    // player.GetComponent<PlayerHealth>().TakeDamage(rollDamage);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 翻滚结束后回到走路
        StartCoroutine(PlayAnimationState("walk", true));
        isUsingSkill = false;
    }

    // 可选：在 Scene 中可视化追踪方向
    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, player.position + Vector3.up * 0.5f);
        }
    }
}
