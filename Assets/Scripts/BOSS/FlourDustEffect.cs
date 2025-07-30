using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FlourDustEffect : MonoBehaviour
{
    private ParticleSystem ps;

    void Awake()
    {
        // 添加或获取 ParticleSystem
        ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = gameObject.AddComponent<ParticleSystem>();

        // 主模块
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startColor = Color.white;
        main.maxParticles = 200;

        // 发射模块：一次性爆发
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] {
            new ParticleSystem.Burst(0f, 30, 50, 1, 0f)
        });

        // 形状：向外发散的圆锥
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.5f;

        // 速度随生命周期变化，制造轻微抖动
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
        vel.y = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        vel.z = new ParticleSystem.MinMaxCurve(-1f, 1f);

        // 透明度渐变：从1到0
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // 渲染器：使用默认 Unlit 粒子 Shader
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
    }

    /// <summary>
    /// 在需要时调用，播放一次面粉粉尘
    /// </summary>
    public void Play()
    {
        if (ps != null)
            ps.Play();
    }
}
