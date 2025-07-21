using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumplingKing_Born : MonoBehaviour
{
    [System.Serializable]
    public class LimbAnimation
    {
        public string name;
        public Animator animator;
        public AnimationClip animationClip;
    }

    public List<LimbAnimation> limbs;

    public float animationDelay = 0.2f;   // 每个 limb 之间的播放间隔

    public MonoBehaviour combatScript;    // 将这里拖入战斗逻辑脚本（比如 DumplingKingCombat）
    public string enableMethodName = "StartCombat";  // 战斗脚本里定义的方法名

    void Start()
    {
        StartCoroutine(PlayBornAnimation());
    }

    IEnumerator PlayBornAnimation()
    {
        foreach (var limb in limbs)
        {
            if (limb.animator != null && limb.animationClip != null)
            {
                limb.animator.gameObject.SetActive(true);
                limb.animator.Play(limb.animationClip.name);
            }

            yield return new WaitForSeconds(animationDelay);
        }

        // ✅ 动画全部完成，通知开始战斗
        if (combatScript != null && !string.IsNullOrEmpty(enableMethodName))
        {
            combatScript.Invoke(enableMethodName, 0f);
        }
    }
}
