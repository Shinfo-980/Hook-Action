using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    [Header("獲得スコア")]
    [SerializeField] private int point = 1;

    private bool isCollected = false;

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

        if (ScoreManager.instance == null)
        {
            Debug.LogError("Scene内にScoreManagerが存在しません。");
            return;
        }

        isCollected = true;

        ScoreManagerTest.instance.AddScore(point);

        Destroy(gameObject);
    }
}