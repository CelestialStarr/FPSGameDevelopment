using UnityEngine;

public class BossDamageDealer : MonoBehaviour
{
    public int rollDamage = 30; // 撞击伤害
    public ParticleSystem hitEffect; // 白色粒子特效（爆炸面粉）
    public Transform effectSpawnPoint; // 粒子生成的位置（例如身体中心）

    private bool isRolling = false;

    // 开始翻滚攻击（在播放翻滚动画时调用）
    public void StartRollAttack()
    {
        isRolling = true;
    }

    // 停止翻滚攻击（可以在动画结束或撞击后调用）
    public void StopRollAttack()
    {
        isRolling = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isRolling && collision.gameObject.CompareTag("Player"))
        {
            // 播放粒子效果
            if (hitEffect != null && effectSpawnPoint != null)
            {
                Instantiate(hitEffect, effectSpawnPoint.position, Quaternion.identity);
            }

            // 造成玩家伤害
            PlayerHealthController playerHealth = collision.gameObject.GetComponent<PlayerHealthController>();
            if (playerHealth != null && !playerHealth.IsDead())
            {
                playerHealth.DamagePlayer(rollDamage);
                Debug.Log("💥 Player hit by roll! -" + rollDamage + " HP");
            }

            StopRollAttack(); // 撞到玩家后停止翻滚（根据你设定，也可以不停止）
        }
    }
}
