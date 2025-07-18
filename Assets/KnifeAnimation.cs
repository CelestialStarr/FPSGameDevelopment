using UnityEngine;
using System.Collections;

public class SimpleKnifeAnimation : MonoBehaviour
{
    public float swingSpeed = 10f;
    public float swingAngle = 90f;

    private bool isSwinging = false;
    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.localRotation;
    }

    public void PlaySwingAnimation()
    {
        if (!isSwinging)
        {
            StartCoroutine(SwingAnimation());
        }
    }

    IEnumerator SwingAnimation()
    {
        isSwinging = true;

        // �ӵ�����
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // ���������
            float angle = Mathf.Lerp(45f, -45f, progress);
            transform.localRotation = originalRotation * Quaternion.Euler(0, angle, 0);

            yield return null;
        }

        // �ص�ԭλ
        transform.localRotation = originalRotation;
        isSwinging = false;
    }
}
