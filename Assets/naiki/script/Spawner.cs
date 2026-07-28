using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    private class SpawnItem
    {
        [Header("生成するアイテム")]
        public GameObject prefab;

        [Header("出現しやすさ")]
        [Min(0)]
        public int weight = 1;
    }

    [Header("スポーン候補")]
    [SerializeField] private SpawnItem[] spawnItems;

    [Header("再出現までの時間")]
    [SerializeField] private float spawnTime = 3f;

    private GameObject currentItem;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (currentItem == null)
            {
                yield return new WaitForSeconds(spawnTime);

                GameObject selectedPrefab = GetRandomPrefab();

                if (selectedPrefab != null)
                {
                    currentItem = Instantiate(
                        selectedPrefab,
                        transform.position,
                        transform.rotation
                    );
                }
            }

            yield return null;
        }
    }

    private GameObject GetRandomPrefab()
    {
        if (spawnItems == null || spawnItems.Length == 0)
        {
            Debug.LogError(
                $"{gameObject.name}のSpawnerにアイテムが登録されていません。"
            );

            return null;
        }

        int totalWeight = 0;

        foreach (SpawnItem item in spawnItems)
        {
            if (item.prefab == null || item.weight <= 0)
            {
                continue;
            }

            totalWeight += item.weight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogError(
                $"{gameObject.name}のSpawnerに抽選可能なアイテムがありません。"
            );

            return null;
        }

        int randomValue = Random.Range(0, totalWeight);

        foreach (SpawnItem item in spawnItems)
        {
            if (item.prefab == null || item.weight <= 0)
            {
                continue;
            }

            if (randomValue < item.weight)
            {
                return item.prefab;
            }

            randomValue -= item.weight;
        }

        return null;
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