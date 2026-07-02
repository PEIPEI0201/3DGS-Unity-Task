using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 叙事表现层：交互提示 + 老人独白配音 + 字幕淡入淡出。全场景唯一。
/// </summary>
public class NarrativeManager : MonoBehaviour
{
    public static NarrativeManager Instance { get; private set; }

    [Header("交互提示 (如 “按 E 唤醒记忆”)")]
    public CanvasGroup promptGroup;
    public TMP_Text promptText;

    [Header("字幕")]
    public CanvasGroup subtitleGroup;
    public TMP_Text subtitleText;
    public float fadeDuration = 0.5f;
    [Tooltip("没有配音时，每行字幕停留秒数")]
    public float lineHoldSeconds = 3.5f;

    [Header("配音")]
    public AudioSource voiceSource;

    Coroutine subtitleRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SetAlpha(promptGroup, 0f);
        SetAlpha(subtitleGroup, 0f);
    }

    // ---- 交互提示 ----
    public void ShowPrompt(string text)
    {
        if (promptText != null) promptText.text = text;
        SetAlpha(promptGroup, 1f);
    }

    public void HidePrompt() => SetAlpha(promptGroup, 0f);

    // ---- 独白 ----
    public void PlayMonologue(AudioClip clip, string[] lines)
    {
        if (subtitleRoutine != null) StopCoroutine(subtitleRoutine);
        subtitleRoutine = StartCoroutine(MonologueRoutine(clip, lines));
    }

    IEnumerator MonologueRoutine(AudioClip clip, string[] lines)
    {
        if (voiceSource != null && clip != null)
        {
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        if (lines != null && lines.Length > 0)
        {
            // 有配音则按配音时长均分显示，否则用固定停留时间
            float per = (clip != null && clip.length > 0f)
                ? clip.length / lines.Length
                : lineHoldSeconds;

            foreach (var line in lines)
            {
                if (subtitleText != null) subtitleText.text = line;
                yield return Fade(subtitleGroup, 1f, fadeDuration);
                yield return new WaitForSeconds(Mathf.Max(per - fadeDuration * 2f, 0.5f));
                yield return Fade(subtitleGroup, 0f, fadeDuration);
            }
        }
        subtitleRoutine = null;
    }

    // ---- 工具 ----
    static void SetAlpha(CanvasGroup g, float a) { if (g != null) g.alpha = a; }

    IEnumerator Fade(CanvasGroup g, float target, float dur)
    {
        if (g == null) yield break;
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
