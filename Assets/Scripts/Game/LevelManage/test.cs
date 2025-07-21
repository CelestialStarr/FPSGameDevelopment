using UnityEngine;

public class test: MonoBehaviour
{
    public GameObject levelCompleteUI;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            levelCompleteUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
