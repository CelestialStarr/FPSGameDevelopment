using UnityEngine;

public class DumplingEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject doughBall;               // 面团模型
    public GameObject dumplingSkin;            // 饺子形态模型
    public CanvasGroup wrapOverlayUI;          // UI画面控制
    public GameObject healingDumplingPrefab;   // 掉落的小饺子
    public Transform dropPoint;                // 小饺子掉落位置

    [Header("Parameters")]
    public float detectRange = 20f;
    public float wrapRange = 2f;
    public float moveSpeed = 2f;
    public int hitsToTransform = 3;    // 面团被打几次才转饺子
    public int dumplingHealth = 3;     // 饺子阶段生命值

    private int currentHitCount = 0;
    private bool isChasing = false;
    private bool isWrapped = false;
    private bool isTransformed = false;

    void Update()
    {
        if (player == null || isWrapped) return;

        float distance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(player.position.x, player.position.z)
        );

        if (!isTransformed)
        {
            if (!isChasing && distance <= detectRange)
                isChasing = true;

            if (isChasing)
            {
                if (distance > wrapRange)
                {
                    // 追击移动
                    Vector3 dir = player.position - transform.position;
                    dir.y = 0;
                    transform.position += dir.normalized * moveSpeed * Time.deltaTime;
                }
                else
                {
                    // 靠近未击杀，触发包裹UI
                    isWrapped = true;
                    doughBall.SetActive(false);
                    ShowWrapUI();
                }
            }
        }
        else
        {
            // 第二阶段逻辑可在此添加远程攻击
        }
    }

    public void TakeHit()
    {
        if (isWrapped) return;

        if (!isTransformed)
        {
            currentHitCount++;
            if (currentHitCount >= hitsToTransform)
            {
                TransformToDumpling();
            }
        }
        else
        {
            dumplingHealth--;
            if (dumplingHealth <= 0)
            {
                DieAndDropHealing();
            }
        }
    }

    void TransformToDumpling()
    {
        isTransformed = true;
        doughBall.SetActive(false);
        dumplingSkin.SetActive(true);
    }

    void DieAndDropHealing()
    {
        if (healingDumplingPrefab != null && dropPoint != null)
        {
            Instantiate(healingDumplingPrefab, dropPoint.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    void ShowWrapUI()
    {
        if (wrapOverlayUI != null)
        {
            wrapOverlayUI.alpha = 1f;
            wrapOverlayUI.blocksRaycasts = true;
            wrapOverlayUI.interactable = true;
        }
    }
}
