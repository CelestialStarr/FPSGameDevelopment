using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("子弹设定")]
    public float speed = 10f;       // 飞行速度
    public int damage = 10;         // 对玩家造成的伤害
    public float lifetime = 5f;     // 存在时长（秒）

    private Vector3 _dir;

    void Start()
    {
        // 发射方向：Z+ 轴
        _dir = transform.forward;
        // 在 lifetime 秒后自动销毁
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 单纯直线移动
        transform.position += _dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // 击中玩家
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealthController>();
            if (ph != null)
            {
                ph.DamagePlayer(damage);
            }
            Destroy(gameObject);
            return;
        }

        // 如果你希望子弹撞到场景的其它物体也销毁：
        if (!other.CompareTag("Enemy") && !other.CompareTag("Minion") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
