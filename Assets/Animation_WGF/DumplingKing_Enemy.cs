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
    public List<AnimationState> animations;
    public float animationDelay = 0.1f;
    public string initialState = "born";
    public bool canMove = false;

    [Header("玩家")]
    public Transform player;

    [Header("移动参数")]
    public float moveSpeed = 1.5f;

    [Header("技能参数")]
    public float skillCheckInterval = 5f;
    public float skillTriggerProbability = 0.4f;

    [Header("召唤设置")]
    public GameObject dumplingMinionPrefab;
    public Vector3 summonOffset = new Vector3(0, 0.5f, 1.5f);

    [Header("翻滚设置")]
    public float rollSpeed = 6f;
    public float rollDuration = 1f;
    public float rollHitRadius = 1f;
    public int rollDamage = 10;

    private float skillCheckTimer = 0f;
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
            canMove = true;
            StartCoroutine(PlayAnimationState("walk", true));
        }));
    }

    void Update()
    {
        if (!canMove || isUsingSkill || player == null) return;

        // 朝向玩家
        Vector3 targetPos = player.position;
        targetPos.y = transform.position.y;
        transform.LookAt(targetPos);

        // 追踪玩家
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // 技能检测
        skillCheckTimer += Time.deltaTime;
        if (skillCheckTimer >= skillCheckInterval)
        {
            skillCheckTimer = 0f;
            if (Random.value < skillTriggerProbability)
                StartCoroutine(TriggerRandomSkill());
        }
    }

    IEnumerator PlayAnimationState(string stateName, bool loop, System.Action onComplete = null)
    {
        AnimationState state = animations.Find(s => s.stateName == stateName);
        if (state == null)
        {
            Debug.LogWarning($"未找到动画状态：{stateName}");
            yield break;
        }

        currentState = stateName;

        // 确保所有部件激活再播放
        foreach (var limb in state.limbAnimations)
            if (limb.animator && limb.animator.gameObject)
                limb.animator.gameObject.SetActive(true);

        // 播放所有 limb 动画
        foreach (var limb in state.limbAnimations)
            if (limb.animator && limb.clip)
                limb.animator.Play(limb.clip.name);

        if (!loop)
        {
            float maxDuration = 0f;
            foreach (var limb in state.limbAnimations)
                if (limb.clip && limb.clip.length > maxDuration)
                    maxDuration = limb.clip.length;

            yield return new WaitForSeconds(maxDuration + animationDelay);

            if (stateName == "roll")
                ShowAllLimbs();  // 恢复 limb

            onComplete?.Invoke();
        }
    }

    IEnumerator TriggerRandomSkill()
    {
        isUsingSkill = true;
        canMove = false;

        int index = Random.Range(0, 2); // 0-roll, 1-summon
        if (index == 0)
        {
            HideLimbsDuringRoll();
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
                canMove = true;
            }));
        }
    }

    IEnumerator RollTowardPlayer()
    {
        Debug.Log("开始翻滚追踪");
        float timer = 0f;
        hasDealtDamageThisRoll = false;

        while (timer < rollDuration)
        {
            if (player == null) break;

            Vector3 dir = player.position - transform.position;
            dir.y = 0f;

            if (dir.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
                transform.position += dir.normalized * rollSpeed * Time.deltaTime;
            }

            if (!hasDealtDamageThisRoll && Vector3.Distance(transform.position, player.position) <= rollHitRadius)
            {
                hasDealtDamageThisRoll = true;
                Debug.Log($"翻滚命中玩家，造成 {rollDamage} 点伤害！");
            }

            timer += Time.deltaTime;
            yield return null;
        }

        ShowAllLimbs();
        StartCoroutine(PlayAnimationState("walk", true));
        isUsingSkill = false;
        canMove = true;
    }

    void SummonMinion()
    {
        if (dumplingMinionPrefab)
        {
            Vector3 spawnPos = transform.position + transform.forward * summonOffset.z + Vector3.up * summonOffset.y;
            Instantiate(dumplingMinionPrefab, spawnPos, Quaternion.identity);
            Debug.Log("召唤小饺子！");
        }
    }

    void HideLimbsDuringRoll()
    {
        foreach (var state in animations)
        {
            foreach (var limb in state.limbAnimations)
            {
                if (limb.animator && limb.animator.gameObject.name != "Body")
                    limb.animator.gameObject.SetActive(false);
            }
        }
    }

    void ShowAllLimbs()
    {
        foreach (var state in animations)
        {
            foreach (var limb in state.limbAnimations)
            {
                if (limb.animator)
                    limb.animator.gameObject.SetActive(true);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (player)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, player.position + Vector3.up * 0.5f);
        }
    }
}
