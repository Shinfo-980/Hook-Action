using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float spawnTime = 3f;

    private GameObject currentItem;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (currentItem == null)
            {
                yield return new WaitForSeconds(spawnTime);

                currentItem = Instantiate(
                    prefab,
                    transform.position,
                    transform.rotation
                );
            }

            yield return null;
        }
    }
}