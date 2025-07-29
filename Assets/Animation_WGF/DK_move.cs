using UnityEngine;

public class DumplingKing_Move : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 1.5f;
    public bool enableFollow = false;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (!enableFollow || player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
    }
}
