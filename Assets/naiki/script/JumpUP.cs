using UnityEngine;

public class JumpUP : MonoBehaviour
{
    [Header("ジャンプ上昇量")]
    [SerializeField] private float jumpAmount = 0.5f;

    private bool isCollected;

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

        PlayerMovement playerMovement =
            other.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            playerMovement =
                other.GetComponentInParent<PlayerMovement>();
        }

        if (playerMovement == null)
        {
            Debug.LogError(
                "PlayerMovementが見つかりませんでした。"
            );

            return;
        }

        isCollected = true;

        playerMovement.JumpUp(jumpAmount);

        Debug.Log(
            $"ジャンプUPアイテム取得：+{jumpAmount}"
        );

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}