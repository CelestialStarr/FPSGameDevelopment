using UnityEngine;

// 陷阱脚本 - 放在陷阱对象上
public class TrapDamage : MonoBehaviour
{
    [Header("Trap Settings")]
    public TrapType trapType = TrapType.InstantKill;
    public int damage = 100;
    public float damageInterval = 1f; // 持续伤害间隔

    [Header("Visual Effects")]
    public GameObject deathEffect;
    public bool showWarning = true;
    public Color warningColor = Color.red;

    public enum TrapType
    {
        InstantKill,    // 立即死亡（刀）
        DamageOverTime, // 持续伤害（油锅、煮锅）
        OneTimeDamage   // 一次性伤害
    }

    private float nextDamageTime = 0f;
    private bool playerInTrap = false;

    // 改为protected virtual，这样子类可以重写
    protected virtual void Start()
    {
        // 设置陷阱标签
        gameObject.tag = "Trap";

        // 视觉提示
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
                    // 第一次伤害
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
        // 播放死亡特效
        if (deathEffect != null)
        {
            Instantiate(deathEffect, PlayerController.instance.transform.position, Quaternion.identity);
        }

        // 直接设置生命值为0
        PlayerHealthController.instance.currentHealth = 0;

        // 触发复活
        CheckpointManager.instance.RespawnPlayer();

        Debug.Log("Player killed by " + gameObject.name);
    }

    void DamagePlayer()
    {
        PlayerHealthController.instance.DamagePlayer(damage);

        // 如果玩家死了，触发复活
        if (PlayerHealthController.instance.currentHealth <= 0)
        {
            CheckpointManager.instance.RespawnPlayer();
        }
    }
}

// 特定陷阱示例 - 油锅
public class OilPotTrap : TrapDamage
{
    [Header("Oil Pot Settings")]
    public ParticleSystem bubbleEffect;
    public AudioSource sizzleSound;

    // 重写父类的Start方法
    protected override void Start()
    {
        // 调用父类的Start
        base.Start();

        // 设置为持续伤害
        trapType = TrapType.DamageOverTime;
        damage = 20;
        damageInterval = 0.5f;

        // 开始冒泡特效
        if (bubbleEffect != null)
        {
            bubbleEffect.Play();
        }

        // 播放滋滋声
        if (sizzleSound != null)
        {
            sizzleSound.Play();
        }
    }
}