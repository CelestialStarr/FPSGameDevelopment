using UnityEngine;

// ������ݿ����� - ��ӵ�Player����
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

        // ����ԭʼ����
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

        // �ָ�����
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

        // ��ʼ����
        if (Mathf.Abs(vertical) > 0.1f)
        {
            if (!isClimbing)
            {
                StartClimbing();
            }

            // ��ֱ�ƶ�
            Vector3 climbMove = Vector3.up * vertical * currentLadder.climbSpeed * Time.deltaTime;

            // ˮƽ�ƶ����������
            if (currentLadder.allowHorizontalMovement)
            {
                climbMove += transform.right * horizontal * currentLadder.horizontalMoveSpeed * Time.deltaTime;
            }

            charController.Move(climbMove);
        }
        else if (isClimbing)
        {
            // ͣ��������
            if (playerController != null)
            {
                playerController.gravityModifier = 0f;
            }
        }

        // ���ո����������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpOffLadder();
        }
    }

    void StartClimbing()
    {
        isClimbing = true;

        // ���������ƶ�
        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.gravityModifier = 0f;
        }

        // ���뵽�������ģ���ѡ��
        Vector3 ladderCenter = currentLadder.transform.position;
        Vector3 playerPos = transform.position;
        playerPos.x = Mathf.Lerp(playerPos.x, ladderCenter.x, Time.deltaTime * 5f);
        playerPos.z = Mathf.Lerp(playerPos.z, ladderCenter.z, Time.deltaTime * 5f);
        transform.position = playerPos;
    }

    void JumpOffLadder()
    {
        // ��һ����Ծ�ٶ�
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.jumpcount = 0;

            // ʹ�ù�������
            playerController.SetJumpVelocity(playerController.jumpPower);
        }

        ExitLadder();
    }
}