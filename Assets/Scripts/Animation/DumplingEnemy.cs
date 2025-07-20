using UnityEngine;
using UnityEngine.UI;

public class DumplingEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;                // 玩家对象
    public GameObject doughBall;            // 跳动的面团对象
    public CanvasGroup wrapOverlayUI;       // 面皮 UI（CanvasGroup 控制透明）

    [Header("Config")]
    public float detectRange = 15f;         // 追击启动范围
    public float wrapRange = 2f;            // 包裹触发距离
    public float moveSpeed = 1.5f;          // 敌人移动速度

    private bool isChasing = false;
    private bool hasWrapped = false;

    void Update()
    {
        // 计算平面距离
        Vector2 enemyXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        float distance = Vector2.Distance(enemyXZ, playerXZ);

        // 开始追击
        if (!isChasing && distance <= detectRange)
        {
            isChasing = true;
        }

        // 正在追击但未包裹
        if (isChasing && !hasWrapped)
        {
            if (distance > wrapRange)
            {
                // 朝玩家方向移动（XZ平面）
                Vector3 direction = player.position - transform.position;
                direction.y = 0;
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
            else
            {
                // 包裹触发逻辑
                hasWrapped = true;
                doughBall.SetActive(false);           // 面团消失
                wrapOverlayUI.alpha = 1f;             // UI 显示
                wrapOverlayUI.blocksRaycasts = true;
                wrapOverlayUI.interactable = true;
            }
        }
    }
}
