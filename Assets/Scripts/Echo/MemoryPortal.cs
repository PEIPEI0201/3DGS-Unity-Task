using UnityEngine;

/// <summary>
/// 记忆之门：唤醒本场景记忆后才显现的光门。玩家走近按 E 穿过 → 淡黑切换到下一场景。
/// 运行时自动生成光晕粒子与点光源，初始隐藏。
/// </summary>
[DisallowMultipleComponent]
public class MemoryPortal : MonoBehaviour
{
    [Header("目标场景")]
    public string targetScene;

    [Header("交互")]
    public float interactRadius = 3f;
    public KeyCode interactKey = KeyCode.E;
    public string promptText = "按 E  穿过光门";

    [Tooltip("需要本场景全部记忆唤醒后才显现")]
    public bool appearAfterAllMemories = true;

    [Header("外观")]
    public Color portalColor = new Color(1f, 0.9f, 0.65f, 1f);
    public float lightIntensity = 2.5f;

    bool revealed;
    bool used;
    bool playerInRange;
    Transform player;
    ParticleSystem ps;
    Light glow;

    void Awake()
    {
        BuildVisuals();
        SetVisible(false);
    }

    void Start()
    {
        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo != null) player = pgo.transform;

        if (appearAfterAllMemories)
        {
            if (EchoGameManager.Instance != null)
                EchoGameManager.Instance.OnAllAwakened += Reveal;
        }
        else Reveal();
    }

    void OnDestroy()
    {
        if (EchoGameManager.Instance != null)
            EchoGameManager.Instance.OnAllAwakened -= Reveal;
    }

    void Reveal()
    {
        if (revealed) return;
        revealed = true;
        SetVisible(true);
    }

    void Update()
    {
        if (!revealed || used || player == null) return;

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
            Use();
    }

    void Use()
    {
        used = true;
        NarrativeManager.Instance?.HidePrompt();
        if (SceneFader.Instance != null) SceneFader.Instance.GoToScene(targetScene);
    }

    void SetVisible(bool v)
    {
        if (ps != null)
        {
            var em = ps.emission; em.enabled = v;
            if (v) ps.Play(); else { ps.Stop(); ps.Clear(); }
        }
        if (glow != null) glow.enabled = v;
    }

    void BuildVisuals()
    {
        ps = gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2.5f;
        main.startSpeed = 0.3f;
        main.startSize = 0.12f;
        main.startColor = portalColor;
        main.maxParticles = 300;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.05f;

        var em = ps.emission; em.rateOverTime = 40f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.6f;

        var col = ps.colorOverLifetime; col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.3f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var pr = GetComponent<ParticleSystemRenderer>();
        if (pr != null) pr.material = MemoryLight.BuildGlowMaterial();

        var lgo = new GameObject("PortalGlow");
        lgo.transform.SetParent(transform, false);
        glow = lgo.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.range = interactRadius * 3f;
        glow.intensity = lightIntensity;
        glow.color = portalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
