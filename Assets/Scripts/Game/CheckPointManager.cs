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
        // ���ó�ʼ������
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

        // ������Ч
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
        // ��ȡ���
        GameObject player = PlayerController.instance.gameObject;

        // ������ҿ���
        PlayerController.instance.enabled = false;

        // ����������������������򵭳�Ч��

        yield return new WaitForSeconds(respawnDelay);

        // �������λ��
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

        // �������״̬
        PlayerHealthController.instance.currentHealth = PlayerHealthController.instance.maxHealth;
        //PlayerHealthController.instance.Start(); // ����UI

        PlayerHealthController.instance.UpdateHealthUI(); // �����·���

        // ����������ҿ���
        PlayerController.instance.enabled = true;

        Debug.Log("Player respawned at checkpoint");
    }
}

// Checkpoint.cs - ���ڼ��������
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

        // �ı���ɫ
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = activeColor;
        }

        // ������Ч
        if (activatedEffect != null)
        {
            Instantiate(activatedEffect, transform.position, Quaternion.identity);
        }
    }
}