using System;
using UnityEngine;

/// <summary>
/// 《回响》流程管理：记录已唤醒的“记忆之光”数量，全部唤醒后触发收束事件。
/// 目前为单场景版本；后续扩展跨场景时再加 DontDestroyOnLoad。
/// </summary>
public class EchoGameManager : MonoBehaviour
{
    public static EchoGameManager Instance { get; private set; }

    [Tooltip("本流程需要唤醒的记忆总数")]
    public int totalMemories = 3;

    public int AwakenedCount { get; private set; }

    /// <summary>每唤醒一段记忆触发一次，参数为当前累计数量。</summary>
    public event Action<int> OnMemoryAwakened;
    /// <summary>全部记忆唤醒后触发一次。</summary>
    public event Action OnAllAwakened;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void NotifyAwakened()
    {
        AwakenedCount++;
        OnMemoryAwakened?.Invoke(AwakenedCount);
        if (AwakenedCount >= totalMemories)
            OnAllAwakened?.Invoke();
    }
}
