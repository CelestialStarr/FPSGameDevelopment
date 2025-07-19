using UnityEngine;
using System.Collections;

public class SimpleKnifeAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationType animationType = AnimationType.Stab;

    [Header("Swing Settings")]
    public float swingAngle = 30f;
    public float swingDuration = 0.2f;
    public float returnDuration = 0.15f;

    [Header("Stab Settings")]
    public float stabDistance = 0.3f;
    public float stabDuration = 0.15f;
    public float stabReturnDuration = 0.1f;

    [Header("Slash Settings")]
    public float slashAngleX = 45f;
    public float slashAngleZ = 20f;

    [Header("Interrupt Settings")]
    public bool allowInterrupt = true;  // �Ƿ������ж�
    public float interruptBlendTime = 0.05f;  // �ж�ʱ�Ļ��ʱ��

    public enum AnimationType
    {
        Swing,
        Stab,
        Slash,
        Uppercut
    }

    private bool isAnimating = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine currentAnimation;  // �洢��ǰ����Э��

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    public void PlayAnimation()
    {
        // ��������ж������ڲ��Ŷ���
        if (allowInterrupt && isAnimating && currentAnimation != null)
        {
            // ֹͣ��ǰ����
            StopCoroutine(currentAnimation);

            // �������õ�ԭʼλ�ã���ѡ��ƽ�����ɣ�
            if (interruptBlendTime > 0)
            {
                StartCoroutine(QuickReset());
            }
            else
            {
                // ��������
                transform.localPosition = originalPosition;
                transform.localRotation = originalRotation;
                isAnimating = false;
            }
        }

        // ��ʼ�¶���
        if (!isAnimating || allowInterrupt)
        {
            switch (animationType)
            {
                case AnimationType.Swing:
                    currentAnimation = StartCoroutine(SwingAnimation());
                    break;
                case AnimationType.Stab:
                    currentAnimation = StartCoroutine(StabAnimation());
                    break;
                case AnimationType.Slash:
                    currentAnimation = StartCoroutine(SlashAnimation());
                    break;
                case AnimationType.Uppercut:
                    currentAnimation = StartCoroutine(UppercutAnimation());
                    break;
            }
        }
    }

    // �������ö���
    IEnumerator QuickReset()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        while (elapsed < interruptBlendTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / interruptBlendTime;

            transform.localPosition = Vector3.Lerp(startPos, originalPosition, t);
            transform.localRotation = Quaternion.Slerp(startRot, originalRotation, t);

            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isAnimating = false;
    }

    // �Ľ��ĻӶ�����
    IEnumerator SwingAnimation()
    {
        isAnimating = true;

        // �ӳ�
        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / swingDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            float angle = Mathf.Lerp(swingAngle * 0.5f, -swingAngle, smoothProgress);
            transform.localRotation = originalRotation * Quaternion.Euler(0, angle, 0);

            yield return null;
        }

        // ����
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / returnDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            float angle = Mathf.Lerp(-swingAngle, 0, smoothProgress);
            transform.localRotation = originalRotation * Quaternion.Euler(0, angle, 0);

            yield return null;
        }

        transform.localRotation = originalRotation;
        isAnimating = false;
        currentAnimation = null;
    }

    // ǰ�̶���
    IEnumerator StabAnimation()
    {
        isAnimating = true;

        // �̳�
        float elapsed = 0f;
        while (elapsed < stabDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / stabDuration;
            float easeProgress = 1f - Mathf.Cos(progress * Mathf.PI * 0.5f);  // EaseOut

            float distance = Mathf.Lerp(0, stabDistance, easeProgress);
            transform.localPosition = originalPosition + transform.forward * distance;

            yield return null;
        }

        // �ջ�
        elapsed = 0f;
        while (elapsed < stabReturnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / stabReturnDuration;
            float easeProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);  // EaseIn

            float distance = Mathf.Lerp(stabDistance, 0, easeProgress);
            transform.localPosition = originalPosition + transform.forward * distance;

            yield return null;
        }

        transform.localPosition = originalPosition;
        isAnimating = false;
        currentAnimation = null;
    }

    // б������
    IEnumerator SlashAnimation()
    {
        isAnimating = true;

        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / swingDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            float xAngle = Mathf.Lerp(slashAngleX, -slashAngleX * 0.5f, smoothProgress);
            float zAngle = Mathf.Lerp(-slashAngleZ, slashAngleZ, smoothProgress);

            transform.localRotation = originalRotation * Quaternion.Euler(xAngle, 0, zAngle);

            float moveProgress = Mathf.Sin(smoothProgress * Mathf.PI);
            transform.localPosition = originalPosition + transform.forward * (stabDistance * 0.5f * moveProgress);

            yield return null;
        }

        // ����
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / returnDuration;

            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, progress);
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, progress);

            yield return null;
        }

        transform.localRotation = originalRotation;
        transform.localPosition = originalPosition;
        isAnimating = false;
        currentAnimation = null;
    }

    // ��������
    IEnumerator UppercutAnimation()
    {
        isAnimating = true;

        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / swingDuration;
            float easeProgress = 1f - Mathf.Cos(progress * Mathf.PI * 0.5f);

            float xAngle = Mathf.Lerp(-30f, 60f, easeProgress);
            transform.localRotation = originalRotation * Quaternion.Euler(xAngle, 0, 0);

            float yMove = Mathf.Sin(progress * Mathf.PI) * 0.2f;
            transform.localPosition = originalPosition + Vector3.up * yMove;

            yield return null;
        }

        // ����
        elapsed = 0f;
        while (elapsed < returnDuration * 1.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (returnDuration * 1.5f);

            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, progress);
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, progress);

            yield return null;
        }

        transform.localRotation = originalRotation;
        transform.localPosition = originalPosition;
        isAnimating = false;
        currentAnimation = null;
    }

    // ǿ��ֹͣ���ж���
    public void ForceStop()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isAnimating = false;
        currentAnimation = null;
    }
}