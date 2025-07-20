using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    [Header("Checkpoint Settings")]
    public Transform currentCheckpoint;
    public GameObject checkpointEffect;
    public float respawnDelay = 2f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 设置初始出生点
        if (currentCheckpoint == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                currentCheckpoint = player.transform;
            }
        }
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;

        // 播放特效
        if (checkpointEffect != null)
        {
            Instantiate(checkpointEffect, newCheckpoint.position, Quaternion.identity);
        }

        Debug.Log("Checkpoint saved at: " + newCheckpoint.position);
    }

    public void RespawnPlayer()
    {
        StartCoroutine(RespawnSequence());
    }

    System.Collections.IEnumerator RespawnSequence()
    {
        // 获取玩家
        GameObject player = PlayerController.instance.gameObject;

        // 禁用玩家控制
        PlayerController.instance.enabled = false;

        // 可以在这里添加死亡动画或淡出效果

        yield return new WaitForSeconds(respawnDelay);

        // 重置玩家位置
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
            player.transform.position = currentCheckpoint.position;
            player.transform.rotation = currentCheckpoint.rotation;
            charController.enabled = true;
        }
        else
        {
            player.transform.position = currentCheckpoint.position;
            player.transform.rotation = currentCheckpoint.rotation;
        }

        // 重置玩家状态
        PlayerHealthController.instance.currentHealth = PlayerHealthController.instance.maxHealth;
        //PlayerHealthController.instance.Start(); // 更新UI

        PlayerHealthController.instance.UpdateHealthUI(); // 调用新方法

        // 重新启用玩家控制
        PlayerController.instance.enabled = true;

        Debug.Log("Player respawned at checkpoint");
    }
}

// Checkpoint.cs - 放在检查点对象上
public class Checkpoint : MonoBehaviour
{
    public bool isActivated = false;
    public GameObject activatedEffect;
    public Color inactiveColor = Color.red;
    public Color activeColor = Color.green;

    private Renderer checkpointRenderer;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = inactiveColor;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        isActivated = true;
        CheckpointManager.instance.SetCheckpoint(transform);

        // 改变颜色
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = activeColor;
        }

        // 播放特效
        if (activatedEffect != null)
        {
            Instantiate(activatedEffect, transform.position, Quaternion.identity);
        }
    }
}