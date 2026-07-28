using UnityEngine;

public class SpeedUP : MonoBehaviour
{
    [Header("速度上昇量")]
    [SerializeField] private float boostSpeed = 2f;

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
            /*
             * PlayerのColliderが子オブジェクトにある場合にも対応。
             */
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

        playerMovement.SpeedUp(
            boostSpeed
        );

        Debug.Log(
            $"移動速度UPアイテム取得：" +
            $"+{boostSpeed}"
        );

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}