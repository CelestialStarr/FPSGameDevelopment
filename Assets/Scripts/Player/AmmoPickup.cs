using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //increse the ammo
           // PlayerController.instance.activeGun.GetAmmo();
            Destroy(gameObject);
        }
    }
}
