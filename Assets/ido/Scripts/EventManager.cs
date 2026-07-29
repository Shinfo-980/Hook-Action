using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private MissionUIManager missionUIManager;

    [Header("ミッションアイテム")]
    [SerializeField] private GameObject missionItemPrefab;

    [Tooltip("ミッションアイテムを出現させる4か所")]
    [SerializeField]
    private Transform[] missionSpawnPoints =
        new Transform[4];

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
    private Coroutine scoreUpCoroutine;

    private int collectedItemCount;
    private bool isMissionActive;

    public bool IsMissionActive =>
        isMissionActive;

    public int CollectedItemCount =>
        collectedItemCount;

    public int RequiredItemCount =>
        requiredItemCount;

    /// <summary>
    /// ミッションイベント開始。
    /// </summary>
    public void missionEvent()
    {
        if (isMissionActive)
        {
            Debug.LogWarning(
                "ミッションはすでに実行中です。"
            );

            return;
        }

        Debug.Log(
            "ミッションイベント発生"
        );

        missionCoroutine =
            StartCoroutine(
                MissionSequence()
            );
    }

    /// <summary>
    /// スコアアップイベント。
    /// </summary>
    public void scoreUp()
    {
        Debug.Log(
            "スコアアップイベント発生"
        );

        if (scoreUpCoroutine != null)
        {
            StopCoroutine(
                scoreUpCoroutine
            );
        }

        scoreUpCoroutine =
            StartCoroutine(
                ScoreUpEventSequence(30f)
            );
    }

    /// <summary>
    /// ミッション全体の進行。
    /// </summary>
    private IEnumerator MissionSequence()
    {
        isMissionActive = true;
        collectedItemCount = 0;

        SpawnMissionItems();

        if (missionUIManager != null)
        {
            missionUIManager.ShowMission(
                requiredItemCount,
                missionTimeLimit
            );
        }
        else
        {
            Debug.LogWarning(
                "Mission UI Managerが設定されていません。"
            );
        }

        Debug.Log(
            $"ミッション開始：" +
            $"{missionTimeLimit}秒以内に" +
            $"{requiredItemCount}個回収してください。"
        );

        float remainingTime =
            missionTimeLimit;

        while (remainingTime > 0f &&
               collectedItemCount < requiredItemCount)
        {
            remainingTime -=
                Time.deltaTime;

            remainingTime =
                Mathf.Max(
                    remainingTime,
                    0f
                );

            if (missionUIManager != null)
            {
                missionUIManager.SetRemainingTime(
                    remainingTime
                );
            }

            yield return null;
        }

        bool missionSucceeded =
            collectedItemCount >=
            requiredItemCount;

        isMissionActive = false;

        RemoveMissionItems();

        if (missionUIManager != null)
        {
            missionUIManager.HideMission();
        }

        if (missionSucceeded)
        {
            Debug.Log(
                "ミッション成功"
            );

            StartSuccessBuff();
        }
        else
        {
            Debug.Log(
                $"ミッション失敗：回収数 " +
                $"{collectedItemCount}/" +
                $"{requiredItemCount}"
            );

            StartFogPenalty();
        }

        missionCoroutine = null;
    }

    /// <summary>
    /// ミッションアイテムを生成する。
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

        foreach (
            Transform spawnPoint
            in missionSpawnPoints
        )
        {
            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    "未設定のミッションスポーン地点があります。"
                );

                continue;
            }

            GameObject spawnedItem =
                Instantiate(
                    missionItemPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

            MissionItem missionItem =
                spawnedItem.GetComponent<MissionItem>();

            if (missionItem == null)
            {
                Debug.LogError(
                    $"{spawnedItem.name}に" +
                    "MissionItemが付いていません。"
                );

                Destroy(
                    spawnedItem
                );

                continue;
            }

            missionItem.Initialize(
                this
            );

            spawnedMissionItems.Add(
                spawnedItem
            );
        }
    }

    /// <summary>
    /// MissionItemから呼ばれる回収通知。
    /// </summary>
    public void CollectMissionItem(
        MissionItem missionItem
    )
    {
        if (!isMissionActive)
        {
            return;
        }

        collectedItemCount++;

        collectedItemCount =
            Mathf.Clamp(
                collectedItemCount,
                0,
                requiredItemCount
            );

        if (missionItem != null)
        {
            spawnedMissionItems.Remove(
                missionItem.gameObject
            );
        }

        if (missionUIManager != null)
        {
            missionUIManager.SetItemCount(
                collectedItemCount
            );
        }

        Debug.Log(
            $"ミッションアイテム回収：" +
            $"{collectedItemCount}/" +
            $"{requiredItemCount}"
        );
    }

    /// <summary>
    /// 残っているミッションアイテムを削除する。
    /// </summary>
    private void RemoveMissionItems()
    {
        foreach (
            GameObject item
            in spawnedMissionItems
        )
        {
            if (item != null)
            {
                Destroy(
                    item
                );
            }
        }

        spawnedMissionItems.Clear();
    }

    /// <summary>
    /// 成功バフを開始する。
    /// </summary>
    private void StartSuccessBuff()
    {
        if (successBuffCoroutine != null)
        {
            StopCoroutine(
                successBuffCoroutine
            );

            successBuffCoroutine = null;
        }

        successBuffCoroutine =
            StartCoroutine(
                SuccessBuffSequence()
            );
    }

    /// <summary>
    /// 成功パネル表示と成功バフの進行。
    /// </summary>
    private IEnumerator SuccessBuffSequence()
    {
        /*
         * 成功パネルを表示する。
         * 元のコードではこの呼び出しがなかった。
         */
        if (missionUIManager != null)
        {
            missionUIManager.ShowSuccess(
                successBuffDuration
            );
        }
        else
        {
            Debug.LogWarning(
                "Mission UI Managerが設定されていないため、" +
                "成功パネルを表示できません。"
            );
        }

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

        float remainingTime =
            successBuffDuration;

        while (remainingTime > 0f)
        {
            remainingTime -=
                Time.deltaTime;

            remainingTime =
                Mathf.Max(
                    remainingTime,
                    0f
                );

            if (missionUIManager != null)
            {
                missionUIManager.SetSuccessRemainingTime(
                    remainingTime
                );
            }

            yield return null;
        }

        if (playerMovement != null)
        {
            playerMovement.ResetMovementBuff();
        }

        if (scoreManager != null)
        {
            scoreManager.ResetScoreMultiplier();
        }

        if (missionUIManager != null)
        {
            missionUIManager.HideSuccess();
        }

        Debug.Log(
            "成功バフ終了"
        );

        successBuffCoroutine = null;
    }

    /// <summary>
    /// 失敗ペナルティを開始する。
    /// </summary>
    private void StartFogPenalty()
    {
        if (fogCoroutine != null)
        {
            StopCoroutine(
                fogCoroutine
            );

            fogCoroutine = null;
        }

        fogCoroutine =
            StartCoroutine(
                FogPenaltySequence()
            );
    }

    /// <summary>
    /// 失敗パネル表示と霧ペナルティの進行。
    /// </summary>
    private IEnumerator FogPenaltySequence()
    {
        if (fogPanel != null)
        {
            fogPanel.SetActive(true);
        }

        if (missionUIManager != null)
        {
            missionUIManager.ShowFailure(
                fogDuration
            );
        }

        Debug.Log(
            $"視界不良ペナルティ開始：{fogDuration}秒"
        );

        float remainingTime =
            fogDuration;

        while (remainingTime > 0f)
        {
            remainingTime -=
                Time.deltaTime;

            remainingTime =
                Mathf.Max(
                    remainingTime,
                    0f
                );

            if (missionUIManager != null)
            {
                missionUIManager.SetFailureRemainingTime(
                    remainingTime
                );
            }

            yield return null;
        }

        if (fogPanel != null)
        {
            fogPanel.SetActive(false);
        }

        if (missionUIManager != null)
        {
            missionUIManager.HideFailure();
        }

        Debug.Log(
            "視界不良ペナルティ終了"
        );

        fogCoroutine = null;
    }

    /// <summary>
    /// 単独のスコアアップイベント。
    /// </summary>
    private IEnumerator ScoreUpEventSequence(
        float duration
    )
    {
        if (scoreManager != null)
        {
            scoreManager.SetScoreMultiplier(
                2f
            );
        }

        yield return new WaitForSeconds(
            duration
        );

        if (scoreManager != null)
        {
            scoreManager.ResetScoreMultiplier();
        }

        Debug.Log(
            "スコアアップイベント終了"
        );

        scoreUpCoroutine = null;
    }

    private void OnDisable()
    {
        RemoveMissionItems();

        isMissionActive = false;

        if (missionUIManager != null)
        {
            missionUIManager.HideMission();
            missionUIManager.HideSuccess();
            missionUIManager.HideFailure();
        }

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