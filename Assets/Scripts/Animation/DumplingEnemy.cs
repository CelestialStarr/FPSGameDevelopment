using UnityEngine;
using UnityEngine.UI;

public class DumplingEnemy : MonoBehaviour
{
    public Transform player;                // 玩家对象
    public GameObject doughBall;            // 敌人本体
    public CanvasGroup wrapOverlayUI;       // UI 显示用

    public float detectRange = 15f;         // 触发追击范围
    public float wrapRange = 2f;            // 包裹触发范围
    public float moveSpeed = 1.5f;

    private bool isChasing = false;
    private bool hasWrapped = false;

    void Update()
    {
        // 计算XZ平面距离
        Vector2 enemyXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        float distance = Vector2.Distance(enemyXZ, playerXZ);

        // 玩家进入追击范围
        if (!isChasing && distance <= detectRange)
        {
            isChasing = true;
        }

        // 开始追击
        if (isChasing && !hasWrapped)
        {
            if (distance > wrapRange)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0; // 保持在地面移动
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
            else
            {
                hasWrapped = true;
                ShowWrapOverlay();
            }
        }
    }

    void ShowWrapOverlay()
    {
        if (wrapOverlayUI != null)
        {
            wrapOverlayUI.alpha = 1f;
            wrapOverlayUI.blocksRaycasts = true;
            wrapOverlayUI.interactable = true;
        }
    }
}
