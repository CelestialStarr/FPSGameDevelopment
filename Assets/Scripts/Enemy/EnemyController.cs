using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private Vector3 targetPoint, startPoint;
    private bool chasing;
    public float distanceToCHase = 10f, distanceToLose = 15f, distanceToStop = 2f;

    private NavMeshAgent agent;

    public float keepChasingTime = 5f;
    private float chaseCounter;

    [Header("Bullet Section")]
    public GameObject bullet;
    public Transform firePoint;
    public float fireRate = 1f, waitBetweenShots = 1f, timeToShoot = 2f;
    private float fireCount, shootWaitCounter, shootTimeCounter;

    public Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPoint = transform.position;

        shootTimeCounter = timeToShoot;
        shootWaitCounter = waitBetweenShots;

        // 检查必需的引用
        ValidateReferences();
    }

    void ValidateReferences()
    {
        if (bullet == null)
        {
            Debug.LogError($"{gameObject.name}: Bullet prefab is not assigned!");
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: FirePoint is not assigned! Creating default firePoint.");
            CreateDefaultFirePoint();
        }

        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent component missing!");
        }
    }

    void CreateDefaultFirePoint()
    {
        // 创建一个默认的firePoint
        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(transform);
        firePointObj.transform.localPosition = Vector3.forward + Vector3.up; // 前方偏上一点
        firePoint = firePointObj.transform;
    }

    void Update()
    {
        // 安全检查
        if (PlayerController.instance == null || agent == null)
        {
            return;
        }

        targetPoint = PlayerController.instance.transform.position;
        targetPoint.y = transform.position.y;

        if (!chasing)
        {
            HandleNotChasingState();
        }
        else
        {
            HandleChasingState();
        }
    }

    void HandleNotChasingState()
    {
        if (Vector3.Distance(transform.position, targetPoint) < distanceToCHase)
        {
            chasing = true;
        }

        if (chaseCounter > 0)
        {
            chaseCounter -= Time.deltaTime;

            if (chaseCounter <= 0)
            {
                SafeSetDestination(startPoint);
            }
        }

        UpdateMovementAnimation();
    }

    void HandleChasingState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, targetPoint);

        if (distanceToPlayer > distanceToStop)
        {
            SafeSetDestination(targetPoint);
        }
        else
        {
            SafeSetDestination(transform.position);
        }

        if (distanceToPlayer > distanceToLose)
        {
            chasing = false;
            chaseCounter = keepChasingTime;
        }

        HandleShooting(distanceToPlayer);
    }

    void HandleShooting(float distanceToPlayer)
    {
        if (shootWaitCounter > 0)
        {
            shootWaitCounter -= Time.deltaTime;

            if (shootWaitCounter <= 0)
            {
                shootTimeCounter = timeToShoot;
            }

            SetMovementAnimation(true);
        }
        else
        {
            shootTimeCounter -= Time.deltaTime;

            if (shootTimeCounter > 0)
            {
                fireCount -= Time.deltaTime;
                if (fireCount <= 0)
                {
                    TryShoot();
                    fireCount = fireRate;
                }

                SafeSetDestination(transform.position); // 射击时停止移动
            }
            else
            {
                shootWaitCounter = waitBetweenShots;
            }

            SetMovementAnimation(false);
        }
    }

    void TryShoot()
    {
        // 安全检查所有必需的引用
        if (bullet == null)
        {
            Debug.LogWarning($"{gameObject.name}: Cannot shoot - bullet prefab is null!");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: Cannot shoot - firePoint is null!");
            return;
        }

        if (PlayerController.instance == null)
        {
            Debug.LogWarning($"{gameObject.name}: Cannot shoot - player not found!");
            return;
        }

        // 瞄准玩家
        Vector3 aimTarget = targetPoint + new Vector3(0f, 0.1f, 0f);
        firePoint.LookAt(aimTarget);

        // 检查瞄准角度
        Vector3 targetDir = PlayerController.instance.transform.position - transform.position;
        float angle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);

        if (Mathf.Abs(angle) < 30f)
        {
            // 安全的实例化
            try
            {
                GameObject bulletInstance = Instantiate(bullet, firePoint.position, firePoint.rotation);

                // 可选：给子弹设置发射者，避免自己打自己
                BullerController bulletScript = bulletInstance.GetComponent<BullerController>();
                if (bulletScript != null)
                {
                    // 如果子弹脚本有设置发射者的方法，可以在这里调用
                }

                Debug.Log($"{gameObject.name} fired a bullet!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{gameObject.name}: Failed to instantiate bullet - {e.Message}");
            }

            // 播放射击动画
            if (anim != null)
            {
                anim.SetTrigger("fireShot");
            }
        }
        else
        {
            shootWaitCounter = waitBetweenShots;
        }
    }

    void SafeSetDestination(Vector3 destination)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            try
            {
                agent.SetDestination(destination);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"{gameObject.name}: Failed to set destination - {e.Message}");
            }
        }
    }

    void UpdateMovementAnimation()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            SetMovementAnimation(agent.remainingDistance > 0.25f);
        }
    }

    void SetMovementAnimation(bool isMoving)
    {
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
        }
    }

    // 在编辑器中显示firePoint位置
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 2f);
        }
    }
}