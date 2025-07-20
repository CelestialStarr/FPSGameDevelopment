using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    [Header("Spawn Point Settings")]
    public Transform spawnPoint; // 固定的出生点
    public GameObject respawnEffect; // 重生特效（可选）

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 保持在场景切换时不被销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 如果没有设置出生点，使用玩家当前位置作为出生点
        if (spawnPoint == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 创建一个空物体作为出生点
                GameObject spawnPointObj = new GameObject("DefaultSpawnPoint");
                spawnPointObj.transform.position = player.transform.position;
                spawnPointObj.transform.rotation = player.transform.rotation;
                spawnPoint = spawnPointObj.transform;

                Debug.Log("Created default spawn point at player position");
            }
        }
    }

    // 设置新的出生点（关卡开始时调用）
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        Debug.Log("Spawn point set to: " + newSpawnPoint.position);
    }

    // 重生玩家（由UIController的倒计时调用）
    public void RespawnPlayer()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("No spawn point set!");
            return;
        }

        GameObject player = PlayerController.instance.gameObject;

        // 移动玩家到出生点
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
            charController.enabled = true;
        }
        else
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }

        // 播放重生特效
        if (respawnEffect != null)
        {
            Instantiate(respawnEffect, spawnPoint.position, Quaternion.identity);
        }

        // 调用PlayerHealthController的重生方法
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.RespawnPlayer();
        }

        Debug.Log("Player respawned at spawn point: " + spawnPoint.position);
    }

    // 获取当前出生点位置
    public Vector3 GetSpawnPosition()
    {
        return spawnPoint != null ? spawnPoint.position : Vector3.zero;
    }
}