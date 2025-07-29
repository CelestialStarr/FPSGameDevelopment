using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DumplingEnemy : MonoBehaviour
{
    [Header("Enemy Movement")]
    public Transform player;
    public GameObject doughBall;
    public float detectRange = 15f;
    public float wrapRange = 2f;
    public float moveSpeed = 1.5f;

    [Header("Wrap Overlay Settings")]
    public CanvasGroup wrapOverlayUI;
    public float overlayDuration = 5f;      // Overlay持续时间（可在Inspector调整）
    public float damageToPlayerRate = 1f;   // 每秒扣血频率
    public int damageToPlayerAmount = 25;   // 每次扣血量

    private bool isChasing = false;
    private bool hasWrapped = false;
    private bool isPlayerTrapped = false;
    private Coroutine overlayCoroutine;
    private Coroutine damageCoroutine;

    void Start()
    {
        if (player == null)
            player = PlayerController.instance.transform;
    }

    void Update()
    {
        if (hasWrapped) return;

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
        if (isChasing)
        {
            if (distance > wrapRange)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0;
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
            else
            {
                hasWrapped = true;
                isPlayerTrapped = true;
                ShowWrapOverlay();
                StartDamageToPlayer();
                StartOverlayTimer();
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
        Debug.Log("Player is wrapped by dumpling!");
    }

    void HideWrapOverlay()
    {
        if (wrapOverlayUI != null)
        {
            wrapOverlayUI.alpha = 0f;
            wrapOverlayUI.blocksRaycasts = false;
            wrapOverlayUI.interactable = false;
        }
        isPlayerTrapped = false;
        Debug.Log("Overlay removed!");
    }

    void StartOverlayTimer()
    {
        if (overlayCoroutine == null)
        {
            overlayCoroutine = StartCoroutine(RemoveOverlayAfterTime());
        }
    }

    IEnumerator RemoveOverlayAfterTime()
    {
        yield return new WaitForSeconds(overlayDuration);

        // 时间到了，移除overlay
        StopDamageToPlayer();
        HideWrapOverlay();
    }

    void StartDamageToPlayer()
    {
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DamagePlayerOverTime());
        }
    }

    void StopDamageToPlayer()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    IEnumerator DamagePlayerOverTime()
    {
        while (isPlayerTrapped)
        {
            yield return new WaitForSeconds(damageToPlayerRate);

            if (!isPlayerTrapped) break;

            // 扣玩家血量
            PlayerHealthController playerHealth = player.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer(damageToPlayerAmount);
                Debug.Log($"Dumpling overlay damaged player for {damageToPlayerAmount}");
            }
            else
            {
                // 如果找不到PlayerHealthController，尝试查找
                playerHealth = FindObjectOfType<PlayerHealthController>();
                if (playerHealth != null)
                {
                    playerHealth.DamagePlayer(damageToPlayerAmount);
                    Debug.Log($"Dumpling overlay damaged player for {damageToPlayerAmount}");
                }
            }
        }
    }

    // 当敌人死亡时调用（可以从EnemyHealthController的Die方法中调用）
    public void OnEnemyDeath()
    {
        // 停止所有协程
        if (overlayCoroutine != null)
        {
            StopCoroutine(overlayCoroutine);
        }

        StopDamageToPlayer();
        HideWrapOverlay();
    }

    // 供小刀破坏overlay使用（如果你想要小刀也能破坏overlay的话）
    public void BreakOverlay()
    {
        if (isPlayerTrapped)
        {
            Debug.Log("Overlay broken by knife!");

            // 停止协程
            if (overlayCoroutine != null)
            {
                StopCoroutine(overlayCoroutine);
            }

            StopDamageToPlayer();
            HideWrapOverlay();
        }
    }

    // 检查玩家是否被困
    public bool IsPlayerTrapped()
    {
        return isPlayerTrapped;
    }
}