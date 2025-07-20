using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform followTarget;
    public float followSpeed = 10f;

    public float normalFOV = 60f;
    public float zoomFOV = 30f;
    public float zoomSpeed = 10f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        cam.fieldOfView = normalFOV;
    }

    void LateUpdate()
    {
        // ���λ�ø���
        if (followTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, followTarget.position, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, followTarget.rotation, followSpeed * Time.deltaTime);
        }

        // �Ҽ�������׼
        if (Input.GetMouseButton(1)) // ��ס�Ҽ�
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomFOV, zoomSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalFOV, zoomSpeed * Time.deltaTime);
        }
    }
}
