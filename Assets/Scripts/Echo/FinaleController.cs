using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 尽头收束：全部记忆唤醒后，延迟片刻 → 画面缓缓化入纯白 → 老人写给年轻自己的信逐行浮现 → BGM 收束。
/// 自建一个高层画布(白幕 + 信件文字)，信件文字始终在白幕之上，保证可读。
/// </summary>
public class FinaleController : MonoBehaviour
{
    [Header("信件(逐行)")]
    [TextArea] public string[] letterLines;
    public AudioClip letterVoice;
    public TMP_FontAsset font;

    [Header("收束 BGM")]
    public AudioClip finaleBgm;

    [Header("节奏(秒)")]
    public float startDelay = 2.5f;
    public float whiteFadeDuration = 6f;
    public float lineFadeDuration = 1f;
    public float lineHold = 3.5f;

    [Header("信件文字颜色(白底上用深色)")]
    public Color letterColor = new Color(0.15f, 0.13f, 0.12f, 1f);

    bool started;
    AudioSource bgmSource;
    CanvasGroup whiteGroup;
    CanvasGroup textGroup;
    TMP_Text letterText;

    void Start()
    {
        if (EchoGameManager.Instance != null)
            EchoGameManager.Instance.OnAllAwakened += Trigger;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = 0.7f;

        BuildUI();
    }

    void OnDestroy()
    {
        if (EchoGameManager.Instance != null)
            EchoGameManager.Instance.OnAllAwakened -= Trigger;
    }

    void BuildUI()
    {
        var cgo = new GameObject("FinaleCanvas");
        cgo.transform.SetParent(transform, false);
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800; // 场景之上；低于淡黑转场(999)
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 白幕
        var wGo = new GameObject("White");
        wGo.transform.SetParent(cgo.transform, false);
        whiteGroup = wGo.AddComponent<CanvasGroup>();
        whiteGroup.alpha = 0f;
        whiteGroup.blocksRaycasts = false;
        var img = wGo.AddComponent<Image>();
        img.color = Color.white;
        var wrt = (RectTransform)wGo.transform;
        wrt.anchorMin = Vector2.zero; wrt.anchorMax = Vector2.one;
        wrt.offsetMin = Vector2.zero; wrt.offsetMax = Vector2.zero;

        // 信件文字(白幕之上)
        var tGo = new GameObject("Letter");
        tGo.transform.SetParent(cgo.transform, false);
        textGroup = tGo.AddComponent<CanvasGroup>();
        textGroup.alpha = 0f;
        letterText = tGo.AddComponent<TextMeshProUGUI>();
        letterText.alignment = TextAlignmentOptions.Center;
        letterText.fontSize = 42;
        letterText.color = letterColor;
        if (font != null) letterText.font = font;
        var trt = (RectTransform)tGo.transform;
        trt.anchorMin = new Vector2(0.15f, 0.32f); trt.anchorMax = new Vector2(0.85f, 0.68f);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
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

        if (finaleBgm != null)
        {
            bgmSource.clip = finaleBgm;
            bgmSource.Play();
        }

        // 缓缓化入纯白
        yield return Fade(whiteGroup, 1f, whiteFadeDuration);

        if (letterVoice != null)
            bgmSource.PlayOneShot(letterVoice);

        // 信件逐行浮现
        if (letterLines != null)
        {
            foreach (var line in letterLines)
            {
                letterText.text = line;
                yield return Fade(textGroup, 1f, lineFadeDuration);
                yield return new WaitForSeconds(lineHold);
                yield return Fade(textGroup, 0f, lineFadeDuration);
            }
        }
    }

    IEnumerator Fade(CanvasGroup g, float target, float dur)
    {
        float start = g.alpha, t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        g.alpha = target;
    }
}
