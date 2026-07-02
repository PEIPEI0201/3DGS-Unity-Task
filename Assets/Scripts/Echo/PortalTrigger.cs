using UnityEngine;

/// <summary>
/// 传送门：玩家进入触发区后淡黑切换到目标场景。
/// 可要求“全部记忆已唤醒”才放行（用于尽头门）。
/// </summary>
[RequireComponent(typeof(Collider))]
public class PortalTrigger : MonoBehaviour
{
    public string targetScene;
    [Tooltip("是否需要先唤醒全部记忆才可通过（尽头门用）")]
    public bool requireAllMemories = false;

    bool triggered;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        if (requireAllMemories && EchoGameManager.Instance != null &&
            EchoGameManager.Instance.AwakenedCount < EchoGameManager.Instance.totalMemories)
            return;

        triggered = true;
        if (SceneFader.Instance != null) SceneFader.Instance.GoToScene(targetScene);
    }
}
