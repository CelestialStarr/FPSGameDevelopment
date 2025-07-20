using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BullerController : MonoBehaviour
{

    public float moveSpeed, lifeTime;
    private Rigidbody rb;

    public GameObject laserImpact;
    public int damage = 2;

    public bool damageEnemy, damagePlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = transform.forward * moveSpeed;//linear velocity means constant speed

        lifeTime -= Time.deltaTime;//counting down

        if(lifeTime <=0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy") && damageEnemy)
        {
            other.gameObject.GetComponent<EnemyHealthController>().DamageEnemy(damage);
           // Destroy(other.transform.parent.gameObject);
        }

        if (other.CompareTag("headShot") && damageEnemy)
        {
            other.transform.parent.GetComponent<EnemyHealthController>().DamageEnemy(damage*2);
        }

        if(other.CompareTag("Player") && damagePlayer)
        {
            Debug.Log("Hit the player!!!");
            PlayerHealthController.instance.DamagePlayer(damage);
        }

        float offset = 0.7f;
        Vector3 newPosition = transform.position - transform.forward * offset;

        Instantiate(laserImpact, transform.position, transform.rotation);
        Destroy(gameObject);
    }

}
