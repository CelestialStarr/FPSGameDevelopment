using UnityEngine;

public class HealthPickUp : MonoBehaviour
{
    public int healAmount = 10;
    private bool isPlayerNearby = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UIController.Instance.ShowPickupHint($"°´ F ³ÔÏÂ +{healAmount} HP");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UIController.Instance.HidePickupHint();
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            PlayerHealthController.instance.HealPlayer(healAmount);
            UIController.Instance.HidePickupHint();
            Destroy(gameObject);
        }
    }
}
