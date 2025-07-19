using UnityEngine;

// ����ű� - �������������
public class TrapDamage : MonoBehaviour
{
    [Header("Trap Settings")]
    public TrapType trapType = TrapType.InstantKill;
    public int damage = 100;
    public float damageInterval = 1f; // �����˺����

    [Header("Visual Effects")]
    public GameObject deathEffect;
    public bool showWarning = true;
    public Color warningColor = Color.red;

    public enum TrapType
    {
        InstantKill,    // ��������������
        DamageOverTime, // �����˺����͹��������
        OneTimeDamage   // һ�����˺�
    }

    private float nextDamageTime = 0f;
    private bool playerInTrap = false;

    // ��Ϊprotected virtual���������������д
    protected virtual void Start()
    {
        // ���������ǩ
        gameObject.tag = "Trap";

        // �Ӿ���ʾ
        if (showWarning)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = warningColor;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrap = true;

            switch (trapType)
            {
                case TrapType.InstantKill:
                    InstantKillPlayer();
                    break;

                case TrapType.OneTimeDamage:
                    DamagePlayer();
                    break;

                case TrapType.DamageOverTime:
                    // ��һ���˺�
                    DamagePlayer();
                    break;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && trapType == TrapType.DamageOverTime)
        {
            if (Time.time >= nextDamageTime)
            {
                DamagePlayer();
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrap = false;
        }
    }

    void InstantKillPlayer()
    {
        // ����������Ч
        if (deathEffect != null)
        {
            Instantiate(deathEffect, PlayerController.instance.transform.position, Quaternion.identity);
        }

        // ֱ����������ֵΪ0
        PlayerHealthController.instance.currentHealth = 0;

        // ��������
        CheckpointManager.instance.RespawnPlayer();

        Debug.Log("Player killed by " + gameObject.name);
    }

    void DamagePlayer()
    {
        PlayerHealthController.instance.DamagePlayer(damage);

        // ���������ˣ���������
        if (PlayerHealthController.instance.currentHealth <= 0)
        {
            CheckpointManager.instance.RespawnPlayer();
        }
    }
}

// �ض�����ʾ�� - �͹�
public class OilPotTrap : TrapDamage
{
    [Header("Oil Pot Settings")]
    public ParticleSystem bubbleEffect;
    public AudioSource sizzleSound;

    // ��д�����Start����
    protected override void Start()
    {
        // ���ø����Start
        base.Start();

        // ����Ϊ�����˺�
        trapType = TrapType.DamageOverTime;
        damage = 20;
        damageInterval = 0.5f;

        // ��ʼð����Ч
        if (bubbleEffect != null)
        {
            bubbleEffect.Play();
        }

        // ����������
        if (sizzleSound != null)
        {
            sizzleSound.Play();
        }
    }
}