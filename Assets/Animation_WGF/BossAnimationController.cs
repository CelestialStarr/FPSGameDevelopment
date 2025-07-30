using UnityEngine;
using System.Collections;

public class BossAnimationController : MonoBehaviour
{
    public Animator bodyAnimator;
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;
    public Animator leftLegAnimator;
    public Animator rightLegAnimator;

    public enum BossState { Born, Walk, Roll, Summon, Dead }
    private BossState currentState;
    public BossState CurrentState => currentState;

    public void PlayState(BossState state)
    {
        currentState = state;
        string action = state.ToString();

        switch (state)
        {
            case BossState.Born:
            case BossState.Walk:
            case BossState.Dead:
                PlayAll(action);
                break;

            case BossState.Roll:
                PlaySingle(bodyAnimator, "Body_" + action);
                leftHandAnimator?.gameObject.SetActive(false);
                rightHandAnimator?.gameObject.SetActive(false);
                leftLegAnimator?.gameObject.SetActive(false);
                rightLegAnimator?.gameObject.SetActive(false);
                break;

            case BossState.Summon:
                PlaySingle(bodyAnimator, "Body_" + action);
                PlaySingle(leftHandAnimator, "LeftHand_" + action);
                leftHandAnimator?.gameObject.SetActive(true);
                rightHandAnimator?.gameObject.SetActive(false);
                leftLegAnimator?.gameObject.SetActive(false);
                rightLegAnimator?.gameObject.SetActive(false);
                break;
        }
    }

    void PlayAll(string action)
    {
        PlaySingle(bodyAnimator, "Body_" + action);
        PlaySingle(leftHandAnimator, "LeftHand_" + action);
        PlaySingle(rightHandAnimator, "RightHand_" + action);
        PlaySingle(leftLegAnimator, "LeftLeg_" + action);
        PlaySingle(rightLegAnimator, "RightLeg_" + action);

        leftHandAnimator?.gameObject.SetActive(true);
        rightHandAnimator?.gameObject.SetActive(true);
        leftLegAnimator?.gameObject.SetActive(true);
        rightLegAnimator?.gameObject.SetActive(true);
    }

    void PlaySingle(Animator animator, string clipName)
    {
        if (animator != null)
        {
            animator.Play(clipName);
        }
    }

    public void FadeOutAll(float duration = 1f)
    {
        StartCoroutine(FadeOutLimb(bodyAnimator.gameObject, duration));
        StartCoroutine(FadeOutLimb(leftHandAnimator.gameObject, duration));
        StartCoroutine(FadeOutLimb(rightHandAnimator.gameObject, duration));
        StartCoroutine(FadeOutLimb(leftLegAnimator.gameObject, duration));
        StartCoroutine(FadeOutLimb(rightLegAnimator.gameObject, duration));
    }

    IEnumerator FadeOutLimb(GameObject obj, float duration)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) yield break;

        Material mat = rend.material;
        Color original = mat.color;

        float t = 0f;
        while (t < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / duration);
            mat.color = new Color(original.r, original.g, original.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        mat.color = new Color(original.r, original.g, original.b, 0f);
        Destroy(obj); // »ò setActive(false)
    }
}
