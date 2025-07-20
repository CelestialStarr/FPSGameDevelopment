using Unity.Mathematics;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject bullet;

    public float rangetToTargetPlayer, timeBetweenShots = .5f;
    private float shotCounter;
    public float rotationSpeed;

    public Transform gun, firepoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotCounter = timeBetweenShots;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, PlayerController.instance.transform.position) < rangetToTargetPlayer)
        {
            gun.LookAt(PlayerController.instance.transform.position + new Vector3(0f, 0.3f, 0f));

            shotCounter -= Time.deltaTime;

            if(shotCounter <= 0)
            {
                Instantiate(bullet, firepoint.position, firepoint.rotation);
                shotCounter = timeBetweenShots;
            }
            else
            {
                shotCounter = timeBetweenShots;
                gun.rotation = Quaternion.Lerp(gun.rotation, Quaternion.Euler(0f, gun.rotation.eulerAngles.y + 10f, 0),rotationSpeed * Time.deltaTime); 
            }
        }
    }
}
