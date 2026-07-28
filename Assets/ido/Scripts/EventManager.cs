using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ScoreManager scoreManager;

    [Header("ミッションアイテム")]
    [SerializeField] private GameObject missionItemPrefab;

    [Tooltip("ミッションアイテムを出現させる4か所")]
    [SerializeField] private Transform[] missionSpawnPoints = new Transform[4];

    [Header("ミッション設定")]
    [SerializeField] private float missionTimeLimit = 50f;
    [SerializeField] private int requiredItemCount = 4;

    [Header("成功時バフ")]
    [SerializeField] private float movementSpeedMultiplier = 1.5f;
    [SerializeField] private float jumpPowerMultiplier = 1.5f;
    [SerializeField] private float scoreMultiplier = 2f;
    [SerializeField] private float successBuffDuration = 20f;

    [Header("失敗時デバフ")]
    [SerializeField] private GameObject fogPanel;
    [SerializeField] private float fogDuration = 20f;

    private readonly List<GameObject> spawnedMissionItems =
        new List<GameObject>();

    private Coroutine missionCoroutine;
    private Coroutine successBuffCoroutine;
    private Coroutine fogCoroutine;

    private int collectedItemCount;
    private bool isMissionActive;

    public bool IsMissionActive => isMissionActive;

    /// <summary>
    /// ミッションイベント開始
    /// </summary>
    public void missionEvent()
    {
        if (isMissionActive)
        {
            Debug.LogWarning("ミッションはすでに実行中です。");
            return;
        }

        Debug.Log("ミッションイベント発生");

        missionCoroutine = StartCoroutine(MissionSequence());
    }

    /// <summary>
    /// スコアアップイベント
    /// </summary>
    public void scoreUp()
    {
        Debug.Log("スコアアップイベント発生");

        if (successBuffCoroutine != null)
        {
            StopCoroutine(successBuffCoroutine);
        }

        successBuffCoroutine = StartCoroutine(
            ScoreUpEventSequence(30f)
        );
    }

    private IEnumerator MissionSequence()
    {
        isMissionActive = true;
        collectedItemCount = 0;

        SpawnMissionItems();

        Debug.Log(
            $"ミッション開始：{missionTimeLimit}秒以内に" +
            $"{requiredItemCount}個回収してください。"
        );

        float remainingTime = missionTimeLimit;

        while (remainingTime > 0f &&
               collectedItemCount < requiredItemCount)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        bool missionSucceeded =
            collectedItemCount >= requiredItemCount;

        isMissionActive = false;

        RemoveMissionItems();

        if (missionSucceeded)
        {
            Debug.Log("ミッション成功");

            StartSuccessBuff();
        }
        else
        {
            Debug.Log(
                $"ミッション失敗：回収数 " +
                $"{collectedItemCount}/{requiredItemCount}"
            );

            StartFogPenalty();
        }

        missionCoroutine = null;
    }

    /// <summary>
    /// 4か所にミッションアイテムを生成
    /// </summary>
    private void SpawnMissionItems()
    {
        RemoveMissionItems();

        if (missionItemPrefab == null)
        {
            Debug.LogError(
                "Mission Item Prefabが設定されていません。"
            );

            return;
        }

        if (missionSpawnPoints == null ||
            missionSpawnPoints.Length == 0)
        {
            Debug.LogError(
                "Mission Spawn Pointsが設定されていません。"
            );

            return;
        }

        foreach (Transform spawnPoint in missionSpawnPoints)
        {
            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    "未設定のミッションスポーン地点があります。"
                );

                continue;
            }

            GameObject spawnedItem = Instantiate(
                missionItemPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            MissionItem missionItem =
                spawnedItem.GetComponent<MissionItem>();

            if (missionItem == null)
            {
                Debug.LogError(
                    $"{spawnedItem.name}にMissionItemが付いていません。"
                );

                Destroy(spawnedItem);
                continue;
            }

            missionItem.Initialize(this);

            spawnedMissionItems.Add(spawnedItem);
        }
    }

    /// <summary>
    /// MissionItemから呼ばれる回収通知
    /// </summary>
    public void CollectMissionItem(MissionItem missionItem)
    {
        if (!isMissionActive)
        {
            return;
        }

        collectedItemCount++;

        if (missionItem != null)
        {
            spawnedMissionItems.Remove(
                missionItem.gameObject
            );
        }

        Debug.Log(
            $"ミッションアイテム回収：" +
            $"{collectedItemCount}/{requiredItemCount}"
        );
    }

    /// <summary>
    /// フィールドに残っているミッションアイテムを削除
    /// </summary>
    private void RemoveMissionItems()
    {
        foreach (GameObject item in spawnedMissionItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }

        spawnedMissionItems.Clear();
    }

    private void StartSuccessBuff()
    {
        if (successBuffCoroutine != null)
        {
            StopCoroutine(successBuffCoroutine);
        }

        successBuffCoroutine = StartCoroutine(
            SuccessBuffSequence()
        );
    }

    private IEnumerator SuccessBuffSequence()
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovementBuff(
                movementSpeedMultiplier,
                jumpPowerMultiplier
            );
        }

        if (scoreManager != null)
        {
            scoreManager.SetScoreMultiplier(
                scoreMultiplier
            );
        }

        Debug.Log(
            $"成功バフ開始：{successBuffDuration}秒間、" +
            $"移動速度×{movementSpeedMultiplier}、" +
            $"ジャンプ力×{jumpPowerMultiplier}、" +
            $"スコア×{scoreMultiplier}"
        );

        yield return new WaitForSeconds(
            successBuffDuration
        );

        if (playerMovement != null)
        {
            playerMovement.ResetMovementBuff();
        }

        if (scoreManager != null)
        {
            scoreManager.ResetScoreMultiplier();
        }

        Debug.Log("成功バフ終了");

        successBuffCoroutine = null;
    }

    private void StartFogPenalty()
    {
        if (fogCoroutine != null)
        {
            StopCoroutine(fogCoroutine);
        }

        fogCoroutine = StartCoroutine(
            FogPenaltySequence()
        );
    }

    private IEnumerator FogPenaltySequence()
    {
        if (fogPanel != null)
        {
            fogPanel.SetActive(true);
        }

        Debug.Log(
            $"視界不良ペナルティ開始：{fogDuration}秒"
        );

        yield return new WaitForSeconds(fogDuration);

        if (fogPanel != null)
        {
            fogPanel.SetActive(false);
        }

        Debug.Log("視界不良ペナルティ終了");

        fogCoroutine = null;
    }

    private IEnumerator ScoreUpEventSequence(float duration)
    {
        if (scoreManager != null)
        {
            scoreManager.SetScoreMultiplier(2f);
        }

        yield return new WaitForSeconds(duration);

        if (scoreManager != null)
        {
            scoreManager.ResetScoreMultiplier();
        }

        Debug.Log("スコアアップイベント終了");

        successBuffCoroutine = null;
    }

    private void OnDisable()
    {
        RemoveMissionItems();

        if (fogPanel != null)
        {
            fogPanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.ResetMovementBuff();
        }

        if (scoreManager != null)
        {
            scoreManager.ResetScoreMultiplier();
        }
    }
}