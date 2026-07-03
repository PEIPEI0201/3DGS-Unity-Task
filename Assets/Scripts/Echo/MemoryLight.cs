using System.Collections;
using UnityEngine;

/// <summary>
/// “记忆之光”：一团沉睡的光尘。玩家走近显示提示、按 E 唤醒——
/// 粒子绽放、点光源渐亮，并播放老人的一段独白。
/// 直接挂在空物体上即可，粒子与光会在运行时自动生成，无需手动配置。
/// </summary>
[DisallowMultipleComponent]
public class MemoryLight : MonoBehaviour
{
    [Header("交互")]
    [Tooltip("玩家进入此水平半径内可按键唤醒")]
    public float interactRadius = 2.5f;
    public KeyCode interactKey = KeyCode.E;
    public string promptText = "按 E  唤醒记忆";

    [Header("独白 (可留空，先跑逻辑)")]
    public AudioClip voiceClip;
    [TextArea] public string[] subtitleLines;

    [Header("外观")]
    public Color sleepingColor = new Color(0.50f, 0.55f, 0.70f, 1f);
    public Color awakenedColor = new Color(1.00f, 0.85f, 0.55f, 1f);
    [Tooltip("唤醒后点光源的最大强度")]
    public float awakenedLightIntensity = 3f;

    bool awakened;
    bool playerInRange;
    Transform player;
    ParticleSystem ps;
    Light glow;

    void Awake()
    {
        BuildVisuals();
        ApplySleepingLook();
    }

    void Start()
    {
        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo != null) player = pgo.transform;
    }

    void Update()
    {
        if (awakened || player == null) return;

        // 水平距离判断（忽略高度差），更贴近“走近”的直觉
        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = player.position;    b.y = 0f;
        bool inRange = Vector3.Distance(a, b) <= interactRadius;

        if (inRange && !playerInRange)
        {
            playerInRange = true;
            NarrativeManager.Instance?.ShowPrompt(promptText);
        }
        else if (!inRange && playerInRange)
        {
            playerInRange = false;
            NarrativeManager.Instance?.HidePrompt();
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
            Awaken();
    }

    void Awaken()
    {
        awakened = true;
        playerInRange = false;
        if (NarrativeManager.Instance != null)
        {
            NarrativeManager.Instance.HidePrompt();
            NarrativeManager.Instance.PlayMonologue(voiceClip, subtitleLines);
        }
        StartCoroutine(BloomRoutine());
        EchoGameManager.Instance?.NotifyAwakened();

        var splat = FindFirstObjectByType<GaussianSplatting.Runtime.GaussianSplatRenderer>();
        splat?.PlayRipple(transform.position);
    }

    IEnumerator BloomRoutine()
    {
        // 一次绽放爆发
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = awakenedColor;
            var emission = ps.emission;
            emission.rateOverTime = 30f;
            ps.Emit(60);
        }

        // 点光源强度、颜色渐变
        float t = 0f, dur = 2f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            if (glow != null)
            {
                glow.intensity = Mathf.Lerp(0.2f, awakenedLightIntensity, k);
                glow.color = Color.Lerp(sleepingColor, awakenedColor, k);
            }
            yield return null;
        }
    }

    // ---- 运行时自动生成粒子与光 ----
    void BuildVisuals()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = gameObject.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 3f;
        main.startSpeed = 0.15f;
        main.startSize = 0.08f;
        main.startColor = sleepingColor;
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.02f; // 缓缓上升

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = grad;

        // 粒子材质：柔和圆形光点 + Sprites/Default（稳妥不丢材质，粒子颜色由 startColor 决定）
        var pr = GetComponent<ParticleSystemRenderer>();
        if (pr != null)
        {
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            pr.material = BuildGlowMaterial();
        }

        // 子物体点光源
        var lgo = new GameObject("Glow");
        lgo.transform.SetParent(transform, false);
        glow = lgo.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.range = interactRadius * 2.5f;
        glow.intensity = 0.2f;
        glow.color = sleepingColor;
    }

    // 生成一张柔和的圆形光点贴图（全场共享一次），让粒子呈柔和光晕而非硬方块
    static Texture2D _dotTex;
    public static Material BuildGlowMaterial()
    {
        if (_dotTex == null)
        {
            const int s = 64;
            _dotTex = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            Vector2 c = new Vector2(s * 0.5f, s * 0.5f);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c) / (s * 0.5f);
                    float a = Mathf.Clamp01(1f - d);
                    a *= a; // 更柔和的边缘衰减
                    _dotTex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            _dotTex.Apply();
        }
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = _dotTex;
        return mat;
    }

    void ApplySleepingLook()
    {
        if (glow != null) { glow.intensity = 0.2f; glow.color = sleepingColor; }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
