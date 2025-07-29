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

        switch (state)
        {
            case BossState.Born:
                PlayAll("Born");
                break;
            case BossState.Walk:
                PlayAll("Walk");
                break;
            case BossState.Roll:
                PlayOnlyBody("Roll");
                break;
            case BossState.Summon:
                PlayAll("Summon");
                break;
            case BossState.Dead:
                PlayAll("Dead");
                break;
        }
    }

    void PlayAll(string name)
    {
        bodyAnimator?.Play(name);
        leftHandAnimator?.Play(name);
        rightHandAnimator?.Play(name);
        leftLegAnimator?.Play(name);
        rightLegAnimator?.Play(name);

        leftHandAnimator?.gameObject.SetActive(true);
        rightHandAnimator?.gameObject.SetActive(true);
        leftLegAnimator?.gameObject.SetActive(true);
        rightLegAnimator?.gameObject.SetActive(true);
    }

    void PlayOnlyBody(string name)
    {
        bodyAnimator?.Play(name);
        leftHandAnimator?.gameObject.SetActive(false);
        rightHandAnimator?.gameObject.SetActive(false);
        leftLegAnimator?.gameObject.SetActive(false);
        rightLegAnimator?.gameObject.SetActive(false);
    }
}
