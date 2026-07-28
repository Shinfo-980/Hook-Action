using UnityEngine;

public class ItemFloat : MonoBehaviour
{
    [Header("上下移動")]
    [SerializeField] private float floatHeight = 0.25f;

    [SerializeField] private float floatSpeed = 2f;

    [Header("回転")]
    [SerializeField] private float rotateSpeed = 90f;

    // 初期位置
    private Vector3 startPosition;

    // 上下運動の位相
    private float phaseOffset;

    private void Start()
    {
        // 初期位置を保存
        startPosition = transform.position;

        // アイテムごとに位相をランダム化
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        Float();

        Rotate();
    }

    /// <summary>
    /// 上下にふわふわ移動
    /// </summary>
    private void Float()
    {
        Vector3 position = startPosition;

        position.y += Mathf.Sin(
            Time.time * floatSpeed + phaseOffset
        ) * floatHeight;

        transform.position = position;
    }

    /// <summary>
    /// Y軸を中心に回転
    /// </summary>
    private void Rotate()
    {
        transform.Rotate(
            Vector3.up,
            rotateSpeed * Time.deltaTime,
            Space.World
        );
    }
}