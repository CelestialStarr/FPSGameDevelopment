using UnityEngine;

public class LockRelativeAnimatorSafe : MonoBehaviour
{
    private Transform parent;
    private Vector3 initialLocalOffset;

    void Start()
    {
        parent = transform.parent;
        if (parent == null)
        {
            Debug.LogError($"{gameObject.name} 没有父物体");
            enabled = false;
            return;
        }

        // 记录初始 localPosition
        initialLocalOffset = transform.localPosition;
    }

    void LateUpdate()
    {
        if (parent == null) return;

        // 当前实际位置 = 父物体位置 + 父物体方向 * 初始偏移 + 当前动画偏移（保留 Animator 控制 localPosition）
        Vector3 currentAnimOffset = transform.localPosition - initialLocalOffset;
        transform.position = parent.TransformPoint(initialLocalOffset + currentAnimOffset);
    }
}
