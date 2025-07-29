using UnityEngine;

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

        // 把动作名转为字符串，比如 Born -> "Born"
        string action = state.ToString();

        // 拼接成 Body_Born、LeftHand_Born ...
        PlayNamedClip(bodyAnimator, "Body_" + action);
        PlayNamedClip(leftHandAnimator, "LeftHand_" + action);
        PlayNamedClip(rightHandAnimator, "RightHand_" + action);
        PlayNamedClip(leftLegAnimator, "LeftLeg_" + action);
        PlayNamedClip(rightLegAnimator, "RightLeg_" + action);

        // 特别处理：如果是 Roll，隐藏 limb
        if (state == BossState.Roll)
        {
            leftHandAnimator?.gameObject.SetActive(false);
            rightHandAnimator?.gameObject.SetActive(false);
            leftLegAnimator?.gameObject.SetActive(false);
            rightLegAnimator?.gameObject.SetActive(false);
        }
        else
        {
            leftHandAnimator?.gameObject.SetActive(true);
            rightHandAnimator?.gameObject.SetActive(true);
            leftLegAnimator?.gameObject.SetActive(true);
            rightLegAnimator?.gameObject.SetActive(true);
        }
    }

    void PlayNamedClip(Animator animator, string clipName)
    {
        if (animator != null)
        {
            animator.Play(clipName);
        }
    }
}
