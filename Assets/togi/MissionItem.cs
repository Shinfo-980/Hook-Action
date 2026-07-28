using UnityEngine;

public class MissionItem : MonoBehaviour
{
    private EventManager eventManager;
    private bool isCollected;

    public void Initialize(EventManager manager)
    {
        eventManager = manager;
        isCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (eventManager == null)
        {
            Debug.LogError(
                "MissionItemにEventManagerが設定されていません。"
            );

            return;
        }

        if (!eventManager.IsMissionActive)
        {
            return;
        }

        isCollected = true;

        eventManager.CollectMissionItem(this);

        // 回収された瞬間に機能を停止して非表示にする
        gameObject.SetActive(false);

        Destroy(gameObject);
    }
}