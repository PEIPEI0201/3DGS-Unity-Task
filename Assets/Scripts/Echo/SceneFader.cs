using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景淡黑转场：开场自动从黑淡入；调用 GoToScene 时先淡黑再加载目标场景。
/// 每个场景放一个即可，自动创建全屏黑色遮罩。
/// </summary>
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;
    public float fadeDuration = 1.2f;

    CanvasGroup group;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
    }

    void Start() { StartCoroutine(FadeTo(0f)); } // 开场从全黑淡入

    void BuildCanvas()
    {
        var cgo = new GameObject("FadeCanvas");
        cgo.transform.SetParent(transform, false);
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        group = cgo.AddComponent<CanvasGroup>();
        group.alpha = 1f;              // 初始全黑
        group.blocksRaycasts = false;

        var imgGo = new GameObject("Black");
        imgGo.transform.SetParent(cgo.transform, false);
        var img = imgGo.AddComponent<Image>();
        img.color = Color.black;
        var rt = (RectTransform)img.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    public void GoToScene(string sceneName) => StartCoroutine(GoRoutine(sceneName));

    IEnumerator GoRoutine(string sceneName)
    {
        yield return FadeTo(1f);
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (op != null && !op.isDone)
            yield return null;
    }

    IEnumerator FadeTo(float target)
    {
        float start = group.alpha, t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        group.alpha = target;
    }
}
