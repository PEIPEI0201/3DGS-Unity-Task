using System.Collections;
using GaussianSplatting.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 3DGS 点云溶解转场：离开场景时 splat 碎成闪粒，进入场景时从闪粒拼回。
/// 挂到含 GaussianSplatRenderer 的物体（或任意物体并指定 splat）。
/// SceneFader 会在切场景前自动调用（若本组件存在）。
/// </summary>
[DisallowMultipleComponent]
public class SplatSceneTransition : MonoBehaviour
{
    public static SplatSceneTransition Instance { get; private set; }

    [Header("目标")]
    public GaussianSplatRenderer splat;
    [Tooltip("转场结束后恢复的日常特效模式")]
    public SplatVisualEffectMode idleMode = SplatVisualEffectMode.FocusTunnel;

    [Header("溶解转场")]
    public float exitDuration = 2f;
    public float enterDuration = 2.2f;
    [Range(0.15f, 0.9f)] public float sparkleCoverage = 0.55f;
    [Range(2f, 18f)] public float sparkleIntensity = 10f;

    static bool s_EnterDissolvePending;

    float m_SavedCoverage;
    float m_SavedIntensity;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (splat == null) splat = GetComponent<GaussianSplatRenderer>();
        if (splat == null) splat = FindFirstObjectByType<GaussianSplatRenderer>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (s_EnterDissolvePending)
            StartCoroutine(EnterDissolveRoutine());
    }

    /// <summary>供 SceneFader 在淡出黑屏前 await。</summary>
    public IEnumerator PlayExitDissolve()
    {
        if (splat == null || !splat.HasValidAsset)
            yield break;

        PushSparkleTuning();
        splat.SetVisualEffectMode(SplatVisualEffectMode.DissolveSparkle);
        splat.SetDissolveProgress(0f);
        splat.PlayDissolve(exitDuration, reverse: false);

        yield return new WaitForSeconds(exitDuration);
        PopSparkleTuning();
    }

    /// <summary>标记下一场景进入时需要反向溶解（各场景用自己的 idleMode）。</summary>
    public static void MarkEnterDissolve()
    {
        s_EnterDissolvePending = true;
    }

    IEnumerator EnterDissolveRoutine()
    {
        s_EnterDissolvePending = false;
        if (splat == null || !splat.HasValidAsset)
            yield break;

        PushSparkleTuning();
        splat.SetVisualEffectMode(SplatVisualEffectMode.DissolveSparkle);
        splat.SetDissolveProgress(1f);
        splat.PlayDissolve(enterDuration, reverse: true);

        yield return new WaitForSeconds(enterDuration);

        splat.SetVisualEffectMode(idleMode);
        splat.SetDissolveProgress(0f);
        PopSparkleTuning();
    }

    void PushSparkleTuning()
    {
        m_SavedCoverage = splat.DissolveEffects.sparkleCoverage;
        m_SavedIntensity = splat.DissolveEffects.sparkleIntensity;
        splat.DissolveEffects.sparkleCoverage = sparkleCoverage;
        splat.DissolveEffects.sparkleIntensity = sparkleIntensity;
        splat.DissolveEffects.dissolveMode = DissolveMode.DistanceFromCamera;
    }

    void PopSparkleTuning()
    {
        if (splat == null) return;
        splat.DissolveEffects.sparkleCoverage = m_SavedCoverage;
        splat.DissolveEffects.sparkleIntensity = m_SavedIntensity;
    }

#if UNITY_EDITOR
    [ContextMenu("Preview Exit Dissolve")]
    void PreviewExit() => StartCoroutine(PlayExitDissolve());

    [ContextMenu("Preview Enter Dissolve")]
    void PreviewEnter() => StartCoroutine(EnterDissolveRoutine());
#endif
}
