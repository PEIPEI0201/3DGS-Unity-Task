using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public GameObject canvasUI;

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入触发器的是不是玩家
        if (other.CompareTag("Player"))
        {
            if (canvasUI != null) canvasUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvasUI != null) canvasUI.SetActive(false);
        }
    }
}