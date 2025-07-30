using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("飞行参数")]
    public float speed = 12f;       // 飞行速度
    public float lifetime = 5f;     // 存在时长（秒）

    [Header("伤害参数")]
    public int damage = 10;         // 对玩家造成的伤害

    void Start()
    {
        // 超时后自动销毁
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 沿着子物体的 forward 方向匀速前进
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // 碰到玩家
        if (other.CompareTag("Player"))
        {
            // 使用你的 PlayerHealthController.instance 或 GetComponent 调用扣血
            var ph = other.GetComponent<PlayerHealthController>();
            if (ph != null)
            {
                ph.DamagePlayer(damage);
            }
            Destroy(gameObject);
            return;
        }

        // 碰到任何非触发的障碍物也销毁（可选）
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
