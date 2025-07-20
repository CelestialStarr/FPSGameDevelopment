using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    [Header("Spawn Point Settings")]
    public Transform spawnPoint; // �̶��ĳ�����
    public GameObject respawnEffect; // ������Ч����ѡ��

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �����ڳ����л�ʱ��������
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ���û�����ó����㣬ʹ����ҵ�ǰλ����Ϊ������
        if (spawnPoint == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // ����һ����������Ϊ������
                GameObject spawnPointObj = new GameObject("DefaultSpawnPoint");
                spawnPointObj.transform.position = player.transform.position;
                spawnPointObj.transform.rotation = player.transform.rotation;
                spawnPoint = spawnPointObj.transform;

                Debug.Log("Created default spawn point at player position");
            }
        }
    }

    // �����µĳ����㣨�ؿ���ʼʱ���ã�
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        Debug.Log("Spawn point set to: " + newSpawnPoint.position);
    }

    // ������ң���UIController�ĵ���ʱ���ã�
    public void RespawnPlayer()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("No spawn point set!");
            return;
        }

        GameObject player = PlayerController.instance.gameObject;

        // �ƶ���ҵ�������
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

        // ����������Ч
        if (respawnEffect != null)
        {
            Instantiate(respawnEffect, spawnPoint.position, Quaternion.identity);
        }

        // ����PlayerHealthController����������
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.RespawnPlayer();
        }

        Debug.Log("Player respawned at spawn point: " + spawnPoint.position);
    }

    // ��ȡ��ǰ������λ��
    public Vector3 GetSpawnPosition()
    {
        return spawnPoint != null ? spawnPoint.position : Vector3.zero;
    }
}