using UnityEngine;

// ���ӽű� - �������Ӷ�����
public class LadderClimbing : MonoBehaviour
{
    [Header("Ladder Settings")]
    public float climbSpeed = 3f;
    public bool allowHorizontalMovement = true;
    public float horizontalMoveSpeed = 2f;

    void Start()
    {
        gameObject.tag = "Ladder";
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLadderController ladderController = other.GetComponent<PlayerLadderController>();
            if (ladderController != null)
            {
                ladderController.EnterLadder(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLadderController ladderController = other.GetComponent<PlayerLadderController>();
            if (ladderController != null)
            {
                ladderController.ExitLadder();
            }
        }
    }
}