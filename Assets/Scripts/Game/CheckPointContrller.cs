using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPointContrller : MonoBehaviour
{
    public string cpName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(PlayerPrefs.HasKey(SceneManager.GetActiveScene().name + "_cp"))//checking the value inside samplescene_cp, yes or no
        {
            if(PlayerPrefs.GetString(SceneManager.GetActiveScene().name + "_cp") == cpName)//cp2 == himself
            {
                PlayerController.instance.transform.position = transform.position; //teleport th eplayer onto himself( cp1 / cp2 )
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            PlayerPrefs.SetString(SceneManager.GetActiveScene().name + "_cp", cpName);//sample_cp = cp1/cp2
            Debug.Log("Touching " + cpName);
        }
    }
}
