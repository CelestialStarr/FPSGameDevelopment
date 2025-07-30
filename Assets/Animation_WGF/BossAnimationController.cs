using UnityEngine;
using System.Collections;

public class BossAnimationController : MonoBehaviour
{
    public Animator bodyAnim;
    public Animator leftHandAnim;
    public Animator rightHandAnim;
    public Animator leftLegAnim;
    public Animator rightLegAnim;

    [Header("State Durations")]
    public float bornDuration = 3f;
    public float rollDuration = 2f;
    public float summonDuration = 2f;

    public enum BossState { Born, Walk, Roll, Summon, Dead }
    public BossState CurrentState { get; private set; }

    void Start()
    {
        PlayState(BossState.Born);
        StartCoroutine(TransitionAfter(bornDuration, BossState.Walk));
    }

    public void PlayState(BossState state)
    {
        CurrentState = state;
        string s = state.ToString();

        // 先显示/隐藏
        bool roll = state == BossState.Roll;
        leftHandAnim.gameObject.SetActive(!roll);
        rightHandAnim.gameObject.SetActive(!roll);
        leftLegAnim.gameObject.SetActive(true);
        rightLegAnim.gameObject.SetActive(true);

        // 播放
        bodyAnim.Play("Body_" + s);
        leftHandAnim.Play("LeftHand_" + s);
        rightHandAnim.Play("RightHand_" + s);
        leftLegAnim.Play("LeftLeg_" + s);
        rightLegAnim.Play("RightLeg_" + s);
    }

    IEnumerator TransitionAfter(float sec, BossState next)
    {
        yield return new WaitForSeconds(sec);
        PlayState(next);
    }

    /// <summary>
    /// 用于行为脚本中手动回到Walk，重新开启随机触发
    /// </summary>
    public void PlayWalk()
    {
        PlayState(BossState.Walk);
    }

    public void PlayRoll()
    {
        PlayState(BossState.Roll);
    }

    public void PlaySummon()
    {
        PlayState(BossState.Summon);
    }

    public void FadeOutAll(float duration = 1f)
    {
        StartCoroutine(FadeOut(bodyAnim.gameObject, duration));
        StartCoroutine(FadeOut(leftHandAnim.gameObject, duration));
        StartCoroutine(FadeOut(rightHandAnim.gameObject, duration));
        StartCoroutine(FadeOut(leftLegAnim.gameObject, duration));
        StartCoroutine(FadeOut(rightLegAnim.gameObject, duration));
    }

    IEnumerator FadeOut(GameObject go, float duration)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        float t = 0f;
        while (t < duration)
        {
            foreach (var r in rends)
            {
                Color c = r.material.color;
                c.a = Mathf.Lerp(1, 0, t / duration);
                r.material.color = c;
            }
            t += Time.deltaTime;
            yield return null;
        }
    }
}
