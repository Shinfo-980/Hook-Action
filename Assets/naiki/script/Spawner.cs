using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    private class SpawnItem
    {
        [Header("通常アイテム")]
        public GameObject normalPrefab;

        [Header("上位アイテム")]
        public GameObject upgradedPrefab;

        [Header("出現しやすさ")]
        [Min(0)]
        public int weight = 1;
    }

    [Header("スポーン候補")]
    [SerializeField] private SpawnItem[] spawnItems;

    [Header("再出現までの時間")]
    [SerializeField] private float spawnTime = 3f;

    [Header("上位アイテムへ切り替わる残り時間")]
    [SerializeField] private float upgradeRemainingTime = 30f;

    [Header("Timer")]
    [SerializeField] private TimerManager timerManager;

    // 現在フィールド上に存在しているアイテム
    private GameObject currentItem;

    // 現在出現しているアイテムのセット
    private SpawnItem currentSpawnItem;

    private Coroutine spawnCoroutine;

    // trueになると、以降は上位アイテムのみ出現する
    private bool isUpgradedMode;

    

    private void Start()
    {
        if (timerManager == null)
        {
            Debug.LogError(
                $"{gameObject.name}のSpawnerにTimerManagerが設定されていません。"
            );
        }

        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        CheckUpgradeMode();
    }

    /// <summary>
    /// 残り時間を確認して、上位アイテムモードへ切り替える
    /// </summary>
    private void CheckUpgradeMode()
    {
        // 一度切り替わった後は処理しない
        if (isUpgradedMode)
        {
            return;
        }

        if (timerManager == null)
        {
            return;
        }

        if (timerManager.GetTime() > upgradeRemainingTime)
        {
            return;
        }

        isUpgradedMode = true;

        UpgradeCurrentItem();
    }

    /// <summary>
    /// 現在フィールド上に存在している通常アイテムを
    /// 対応する上位アイテムへ置き換える
    /// </summary>
    private void UpgradeCurrentItem()
    {
        if (currentItem == null)
        {
            return;
        }

        if (currentSpawnItem == null)
        {
            Debug.LogError(
                $"{gameObject.name}で現在のアイテム情報を取得できませんでした。"
            );

            return;
        }

        if (currentSpawnItem.upgradedPrefab == null)
        {
            Debug.LogError(
                $"{gameObject.name}の現在のアイテムに上位Prefabが設定されていません。"
            );

            return;
        }

        Vector3 itemPosition =
            currentItem.transform.position;

        Quaternion itemRotation =
            currentItem.transform.rotation;

        Transform itemParent =
            currentItem.transform.parent;

        Destroy(currentItem);

        currentItem = Instantiate(
            currentSpawnItem.upgradedPrefab,
            itemPosition,
            itemRotation,
            itemParent
        );
    }

    /// <summary>
    /// アイテムが存在しない場合、一定時間後に再出現させる
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (currentItem == null)
            {
                // 取得直後などに何度も待機処理が始まらないよう、
                // このコルーチン内で待機してから生成する
                yield return new WaitForSeconds(spawnTime);

                // 待機中に別の処理からアイテムが生成された場合は
                // 重複生成しない
                if (currentItem == null)
                {
                    CreateItem();
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// 重み付き抽選を行い、アイテムを生成する
    /// </summary>
    private void CreateItem()
    {
        SpawnItem selectedItem =
            GetRandomSpawnItem();

        if (selectedItem == null)
        {
            return;
        }

        GameObject prefabToSpawn;

        if (isUpgradedMode)
        {
            prefabToSpawn =
                selectedItem.upgradedPrefab;
        }
        else
        {
            prefabToSpawn =
                selectedItem.normalPrefab;
        }

        if (prefabToSpawn == null)
        {
            string modeName =
                isUpgradedMode
                    ? "上位アイテム"
                    : "通常アイテム";

            Debug.LogError(
                $"{gameObject.name}で選ばれたセットに{modeName}のPrefabが設定されていません。"
            );

            return;
        }

        currentSpawnItem = selectedItem;

        currentItem = Instantiate(
            prefabToSpawn,
            transform.position,
            transform.rotation
        );
    }

    /// <summary>
    /// SpawnItemのセットを重み付きで抽選する
    /// </summary>
    private SpawnItem GetRandomSpawnItem()
    {
        if (spawnItems == null ||
            spawnItems.Length == 0)
        {
            Debug.LogError(
                $"{gameObject.name}のSpawnerにアイテムが登録されていません。"
            );

            return null;
        }

        int totalWeight = 0;

        foreach (SpawnItem item in spawnItems)
        {
            if (!CanSpawnItem(item))
            {
                continue;
            }

            totalWeight += item.weight;
        }

        if (totalWeight <= 0)
        {
            string modeName =
                isUpgradedMode
                    ? "上位アイテム"
                    : "通常アイテム";

            Debug.LogError(
                $"{gameObject.name}のSpawnerに抽選可能な{modeName}がありません。"
            );

            return null;
        }

        int randomValue =
            Random.Range(0, totalWeight);

        foreach (SpawnItem item in spawnItems)
        {
            if (!CanSpawnItem(item))
            {
                continue;
            }

            if (randomValue < item.weight)
            {
                return item;
            }

            randomValue -= item.weight;
        }

        return null;
    }

    /// <summary>
    /// 現在のモードで抽選可能なセットか確認する
    /// </summary>
    private bool CanSpawnItem(SpawnItem item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.weight <= 0)
        {
            return false;
        }

        if (isUpgradedMode)
        {
            return item.upgradedPrefab != null;
        }

        return item.normalPrefab != null;
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
}