using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 每个肢体的动画信息（如身体、左手、右腿等）
[System.Serializable]
public class LimbAnimation
{
    public string name;                // 肢体名称（用于识别，非必要）
    public Animator animator;          // 控制该肢体的 Animator 组件
    public AnimationClip clip;         // 播放的动画片段
}

// 一个完整的动画状态（如出生、走路、攻击）包含所有肢体的动作
[System.Serializable]
public class AnimationState
{
    public string stateName;                   // 状态名称（如 "walk", "born"）
    public List<LimbAnimation> limbAnimations; // 属于该状态的所有肢体动画
}

public class DumplingKing_Enemy : MonoBehaviour
{
    [Header("动画状态集合（如出生、走路等）")]
    public List<AnimationState> animations;

    [Header("非循环动画后延迟播放下一个（秒）")]
    public float animationDelay = 0.1f;

    [Header("初始状态名称")]
    public string initialState = "born";

    [Header("玩家对象（用于追踪）")]
    public Transform player;

    [Header("追踪速度")]
    public float moveSpeed = 1.5f;

    // 出生动画是否已经播放完成
    private bool bornPlayed = false;

    // 当前动画状态名称
    private string currentState = "";

    void Start()
    {
        // 第一步：播放出生动画（只播一次）
        StartCoroutine(PlayAnimationState(initialState, false, () =>
        {
            bornPlayed = true; // 出生动画播放完毕标记
            // 第二步：进入走路状态（循环播放）
            StartCoroutine(PlayAnimationState("walk", true));
        }));
    }

    void Update()
    {
        // 如果出生动画完成且玩家对象存在，则持续面向并靠近玩家
        if (bornPlayed && player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0; // 只在XZ平面追踪

            if (direction.magnitude > 0.1f)
            {
                // 平滑旋转朝向玩家
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                // 移动靠近玩家
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 播放指定动画状态
    /// </summary>
    /// <param name="stateName">动画状态名称，如 "walk"</param>
    /// <param name="loop">是否循环播放（true为持续播放）</param>
    /// <param name="onComplete">非循环播放完成后的回调</param>
    IEnumerator PlayAnimationState(string stateName, bool loop, System.Action onComplete = null)
    {
        // 找到匹配状态
        AnimationState state = animations.Find(s => s.stateName == stateName);
        if (state == null)
        {
            Debug.LogWarning($"找不到动画状态：{stateName}");
            yield break;
        }

        currentState = stateName;

        // 播放该状态下所有 limb 的动画
        foreach (var limb in state.limbAnimations)
        {
            if (limb.animator && limb.clip)
            {
                limb.animator.Play(limb.clip.name);
            }
        }

        // 如果是非循环动画（如出生），等待结束后调用下一步
        if (!loop)
        {
            float duration = 0f;
            foreach (var limb in state.limbAnimations)
            {
                if (limb.clip && limb.clip.length > duration)
                    duration = limb.clip.length;
            }

            yield return new WaitForSeconds(duration + animationDelay);
            onComplete?.Invoke(); // 播放完成后执行下一步（如播放走路动画）
        }
    }
}
