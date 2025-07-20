using UnityEngine;

public class WrapUIHandler : MonoBehaviour
{
    [Header("Knife打击次数上限")]
    public int requiredHits = 3;

    private int currentHits = 0;

    private void OnEnable()
    {
        currentHits = 0;  // 每次启用都重置计数
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "knife")
        {
            currentHits++;
            Debug.Log("Knife Hit UI: " + currentHits);

            if (currentHits >= requiredHits)
            {
                Debug.Log("Wrap UI removed!");
                gameObject.SetActive(false);
            }
        }
    }
}
