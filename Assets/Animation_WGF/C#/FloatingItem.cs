using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FloatingRotatingPickup : MonoBehaviour
{
    [Header("浮动+旋转设置")]
    public float floatAmplitude = 0.5f;   // 浮动幅度
    public float floatFrequency = 1f;     // 浮动速度
    public float rotationSpeed = 45f;     // 自转速度（度/秒）

    [Header("回血设置")]
    public int healAmount = 10;           // 拾取后回血量

    [Header("发光设置")]
    public Color emissionColor = Color.white; // 发光颜色
    [Range(0, 5)] public float emissionIntensity = 2f; // 发光强度

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        EnableGlow();
    }

    void Update()
    {
        // 上下浮动
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // 绕 Y 轴自转
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealthController.instance.HealPlayer(healAmount);
            Destroy(gameObject);
        }
    }

    // 通过代码给所有 Renderer 材质开启发光
    void EnableGlow()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            // 对每个材质都开启 Emission
            foreach (var mat in r.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            }
        }
    }
}
