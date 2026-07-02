using System.Collections;
using UnityEngine;

/// <summary>
/// 尽头收束：全部记忆唤醒后，延迟片刻 → 爆发“光海”粒子 + 渐亮 + 播放老人写给年轻自己的信(字幕)。
/// 挂在场景里一个空物体上，监听 EchoGameManager.OnAllAwakened。
/// </summary>
public class FinaleController : MonoBehaviour
{
    [Header("信件(逐行字幕)")]
    [TextArea] public string[] letterLines;
    public AudioClip letterVoice;

    [Header("收束 BGM")]
    public AudioClip finaleBgm;

    [Header("节奏")]
    public float startDelay = 2.5f;

    [Header("光海")]
    public Color seaColor = new Color(1f, 0.88f, 0.6f, 1f);
    public int burstCount = 400;

    bool started;
    AudioSource bgmSource;

    void Start()
    {
        if (EchoGameManager.Instance != null)
            EchoGameManager.Instance.OnAllAwakened += Trigger;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = 0.7f;
    }

    void OnDestroy()
    {
        if (EchoGameManager.Instance != null)
            EchoGameManager.Instance.OnAllAwakened -= Trigger;
    }

    void Trigger()
    {
        if (started) return;
        started = true;
        StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        yield return new WaitForSeconds(startDelay);

        SpawnLightSea();

        if (finaleBgm != null)
        {
            bgmSource.clip = finaleBgm;
            bgmSource.Play();
        }

        if (NarrativeManager.Instance != null)
            NarrativeManager.Instance.PlayMonologue(letterVoice, letterLines);
    }

    // 在玩家周围爆发一片缓缓升起的金色光尘
    void SpawnLightSea()
    {
        var center = Camera.main != null ? Camera.main.transform.position : transform.position;

        var go = new GameObject("LightSea");
        go.transform.position = center;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 6f;
        main.startSpeed = 0.4f;
        main.startSize = 0.1f;
        main.startColor = seaColor;
        main.maxParticles = 2000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.03f;

        var emission = ps.emission;
        emission.rateOverTime = 60f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 6f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.9f, 0.25f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = grad;

        var pr = go.GetComponent<ParticleSystemRenderer>();
        if (pr != null) pr.material = MemoryLight.BuildGlowMaterial();

        ps.Emit(burstCount);
    }
}
