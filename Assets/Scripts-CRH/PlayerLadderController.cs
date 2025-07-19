using UnityEngine;

// 玩家爬梯控制器 - 添加到Player对象
public class PlayerLadderController : MonoBehaviour
{
    [Header("States")]
    public bool isOnLadder = false;
    public bool isClimbing = false;

    private LadderClimbing currentLadder;
    private CharacterController charController;
    private PlayerController playerController;
    private float originalGravity;
    private Vector3 climbDirection;

    void Start()
    {
        charController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (isOnLadder)
        {
            HandleLadderMovement();
        }
    }

    public void EnterLadder(LadderClimbing ladder)
    {
        currentLadder = ladder;
        isOnLadder = true;

        // 保存原始重力
        if (playerController != null)
        {
            originalGravity = playerController.gravityModifier;
        }

        Debug.Log("Entered ladder");
    }

    public void ExitLadder()
    {
        isOnLadder = false;
        isClimbing = false;
        currentLadder = null;

        // 恢复重力
        if (playerController != null)
        {
            playerController.gravityModifier = originalGravity;
            playerController.enabled = true;
        }

        Debug.Log("Exited ladder");
    }

    void HandleLadderMovement()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // 开始爬梯
        if (Mathf.Abs(vertical) > 0.1f)
        {
            if (!isClimbing)
            {
                StartClimbing();
            }

            // 垂直移动
            Vector3 climbMove = Vector3.up * vertical * currentLadder.climbSpeed * Time.deltaTime;

            // 水平移动（如果允许）
            if (currentLadder.allowHorizontalMovement)
            {
                climbMove += transform.right * horizontal * currentLadder.horizontalMoveSpeed * Time.deltaTime;
            }

            charController.Move(climbMove);
        }
        else if (isClimbing)
        {
            // 停在梯子上
            if (playerController != null)
            {
                playerController.gravityModifier = 0f;
            }
        }

        // 按空格键跳离梯子
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpOffLadder();
        }
    }

    void StartClimbing()
    {
        isClimbing = true;

        // 禁用正常移动
        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.gravityModifier = 0f;
        }

        // 对齐到梯子中心（可选）
        Vector3 ladderCenter = currentLadder.transform.position;
        Vector3 playerPos = transform.position;
        playerPos.x = Mathf.Lerp(playerPos.x, ladderCenter.x, Time.deltaTime * 5f);
        playerPos.z = Mathf.Lerp(playerPos.z, ladderCenter.z, Time.deltaTime * 5f);
        transform.position = playerPos;
    }

    void JumpOffLadder()
    {
        // 给一个跳跃速度
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.jumpcount = 0;

            // 使用公共方法
            playerController.SetJumpVelocity(playerController.jumpPower);
        }

        ExitLadder();
    }
}