using UnityEngine;

public class WrapUIHandler : MonoBehaviour
{
    [Header("Knife�����������")]
    public int requiredHits = 3;

    private int currentHits = 0;

    private void OnEnable()
    {
        currentHits = 0;  // ÿ�����ö����ü���
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
