using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionUIManager : MonoBehaviour
{
    [Header("ミッションUI全体")]
    [SerializeField] private GameObject missionPanel;

    [Header("アイテム収集数")]
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("ミッション制限時間の数字Image")]
    [Tooltip("ミッション時間の十の位")]
    [SerializeField] private Image missionTimerTensImage;

    [Tooltip("ミッション時間の一の位")]
    [SerializeField] private Image missionTimerOnesImage;

    [Header("成功UI全体")]
    [Tooltip("ミッション成功時に表示するパネル")]
    [SerializeField] private GameObject successPanel;

    [Header("成功バフ時間の数字Image")]
    [Tooltip("成功バフ時間の十の位")]
    [SerializeField] private Image successTimerTensImage;

    [Tooltip("成功バフ時間の一の位")]
    [SerializeField] private Image successTimerOnesImage;

    [Header("失敗UI全体")]
    [Tooltip("ミッション失敗時に表示するパネル")]
    [SerializeField] private GameObject failurePanel;

    [Header("失敗ペナルティ時間の数字Image")]
    [Tooltip("失敗ペナルティ時間の十の位")]
    [SerializeField] private Image failureTimerTensImage;

    [Tooltip("失敗ペナルティ時間の一の位")]
    [SerializeField] private Image failureTimerOnesImage;

    [Header("数字スプライト")]
    [Tooltip("Element 0～9に、それぞれ対応する数字スプライトを設定")]
    [SerializeField]
    private Sprite[] numberSprites =
        new Sprite[10];

    private int currentItemCount;
    private int requiredItemCount;

    private bool isMissionDisplayed;
    private bool isSuccessDisplayed;
    private bool isFailureDisplayed;

    public bool IsMissionDisplayed =>
        isMissionDisplayed;

    public bool IsSuccessDisplayed =>
        isSuccessDisplayed;

    public bool IsFailureDisplayed =>
        isFailureDisplayed;

    public int CurrentItemCount =>
        currentItemCount;

    public int RequiredItemCount =>
        requiredItemCount;

    private void Awake()
    {
        /*
         * MissionUIManager自身が非表示にするパネルへ
         * 付いている場合、そのパネルを非表示にした瞬間に
         * MissionUIManagerも停止してしまう。
         */
        if (missionPanel == gameObject ||
            successPanel == gameObject ||
            failurePanel == gameObject)
        {
            Debug.LogError(
                "MissionUIManagerをMission Panel、" +
                "Success Panel、Failure Panel自身に" +
                "付けないでください。" +
                "常に有効なCanvasまたは空のGameObjectに" +
                "付けてください。"
            );
        }

        isMissionDisplayed = false;
        isSuccessDisplayed = false;
        isFailureDisplayed = false;

        currentItemCount = 0;
        requiredItemCount = 0;

        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        if (failurePanel != null)
        {
            failurePanel.SetActive(false);
        }
    }

    /// <summary>
    /// ミッション開始時に呼び出す。
    /// </summary>
    public void ShowMission(
        int requiredCount,
        float startingTime
    )
    {
        if (!ValidateMissionSettings())
        {
            return;
        }

        HideSuccess();
        HideFailure();

        requiredItemCount =
            Mathf.Max(
                1,
                requiredCount
            );

        currentItemCount = 0;
        isMissionDisplayed = true;

        missionPanel.SetActive(true);

        UpdateProgressDisplay();
        SetRemainingTime(startingTime);

        Debug.Log(
            $"ミッションパネルを表示しました。" +
            $"必要数：{requiredItemCount}、" +
            $"制限時間：{startingTime}秒"
        );
    }

    /// <summary>
    /// ミッションアイテムを1個取得したときに呼び出す。
    /// </summary>
    public void AddItem()
    {
        if (!isMissionDisplayed)
        {
            Debug.LogWarning(
                "ミッションが表示されていないため、" +
                "アイテム数を追加できませんでした。"
            );

            return;
        }

        SetItemCount(
            currentItemCount + 1
        );
    }

    /// <summary>
    /// 現在のアイテム取得数を設定する。
    /// </summary>
    public void SetItemCount(int count)
    {
        if (!isMissionDisplayed)
        {
            return;
        }

        currentItemCount =
            Mathf.Clamp(
                count,
                0,
                requiredItemCount
            );

        UpdateProgressDisplay();

        Debug.Log(
            $"ミッション進行度：" +
            $"{currentItemCount}/{requiredItemCount}"
        );
    }

    /// <summary>
    /// アイテム取得数の表示を更新する。
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.text =
            $"{currentItemCount}/{requiredItemCount}";
    }

    /// <summary>
    /// ミッションの残り時間を更新する。
    /// </summary>
    public void SetRemainingTime(
        float remainingTime
    )
    {
        if (!isMissionDisplayed)
        {
            return;
        }

        if (!ValidateMissionTimerSettings())
        {
            return;
        }

        SetTimerImages(
            remainingTime,
            missionTimerTensImage,
            missionTimerOnesImage
        );
    }

    /// <summary>
    /// ミッションパネルを非表示にする。
    /// </summary>
    public void HideMission()
    {
        isMissionDisplayed = false;

        currentItemCount = 0;
        requiredItemCount = 0;

        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ミッション成功パネルを表示する。
    /// </summary>
    public void ShowSuccess(
        float startingTime
    )
    {
        /*
         * タイマーの設定とは分離し、
         * Success Panelだけ設定されていれば
         * パネル自体は表示する。
         */
        if (successPanel == null)
        {
            Debug.LogError(
                "MissionUIManagerのSuccess Panelが" +
                "設定されていません。"
            );

            return;
        }

        HideMission();
        HideFailure();

        isSuccessDisplayed = true;

        successPanel.SetActive(true);

        Debug.Log(
            $"成功パネルを表示しました。" +
            $"成功バフ時間：{startingTime}秒"
        );

        /*
         * タイマーImageや数字スプライトに問題があっても、
         * 成功パネル自体は表示されたままにする。
         */
        if (!ValidateSuccessTimerSettings())
        {
            Debug.LogWarning(
                "成功パネルは表示されましたが、" +
                "成功タイマーの設定が不足しています。"
            );

            return;
        }

        SetSuccessRemainingTime(
            startingTime
        );
    }

    /// <summary>
    /// 成功バフの残り時間を更新する。
    /// </summary>
    public void SetSuccessRemainingTime(
        float remainingTime
    )
    {
        if (!isSuccessDisplayed)
        {
            return;
        }

        if (!ValidateSuccessTimerSettings())
        {
            return;
        }

        SetTimerImages(
            remainingTime,
            successTimerTensImage,
            successTimerOnesImage
        );
    }

    /// <summary>
    /// 成功パネルを非表示にする。
    /// </summary>
    public void HideSuccess()
    {
        isSuccessDisplayed = false;

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ミッション失敗パネルを表示する。
    /// </summary>
    public void ShowFailure(
        float startingTime
    )
    {
        /*
         * 成功パネルと同じように、
         * タイマーの設定とは分離して表示する。
         */
        if (failurePanel == null)
        {
            Debug.LogError(
                "MissionUIManagerのFailure Panelが" +
                "設定されていません。"
            );

            return;
        }

        HideMission();
        HideSuccess();

        isFailureDisplayed = true;

        failurePanel.SetActive(true);

        Debug.Log(
            $"失敗パネルを表示しました。" +
            $"ペナルティ時間：{startingTime}秒"
        );

        if (!ValidateFailureTimerSettings())
        {
            Debug.LogWarning(
                "失敗パネルは表示されましたが、" +
                "失敗タイマーの設定が不足しています。"
            );

            return;
        }

        SetFailureRemainingTime(
            startingTime
        );
    }

    /// <summary>
    /// 失敗ペナルティの残り時間を更新する。
    /// </summary>
    public void SetFailureRemainingTime(
        float remainingTime
    )
    {
        if (!isFailureDisplayed)
        {
            return;
        }

        if (!ValidateFailureTimerSettings())
        {
            return;
        }

        SetTimerImages(
            remainingTime,
            failureTimerTensImage,
            failureTimerOnesImage
        );
    }

    /// <summary>
    /// 失敗パネルを非表示にする。
    /// </summary>
    public void HideFailure()
    {
        isFailureDisplayed = false;

        if (failurePanel != null)
        {
            failurePanel.SetActive(false);
        }
    }

    /// <summary>
    /// すべてのミッション関連UIを非表示にする。
    /// </summary>
    public void HideAll()
    {
        HideMission();
        HideSuccess();
        HideFailure();
    }

    /// <summary>
    /// 2枚のImageに残り時間を表示する共通処理。
    /// </summary>
    private void SetTimerImages(
        float remainingTime,
        Image tensImage,
        Image onesImage
    )
    {
        if (!ValidateNumberSprites())
        {
            return;
        }

        /*
         * 29.1秒の場合は30秒と表示する。
         */
        int displayTime =
            Mathf.CeilToInt(
                remainingTime
            );

        /*
         * Imageが2枚なので0～99秒に制限する。
         */
        displayTime =
            Mathf.Clamp(
                displayTime,
                0,
                99
            );

        int tensNumber =
            displayTime / 10;

        int onesNumber =
            displayTime % 10;

        if (tensImage != null)
        {
            tensImage.sprite =
                numberSprites[tensNumber];

            tensImage.gameObject.SetActive(true);
        }

        if (onesImage != null)
        {
            onesImage.sprite =
                numberSprites[onesNumber];

            onesImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 必要なアイテムをすべて集めたか。
    /// </summary>
    public bool IsTargetReached()
    {
        return
            requiredItemCount > 0 &&
            currentItemCount >=
            requiredItemCount;
    }

    /// <summary>
    /// ミッションUI全体の設定確認。
    /// </summary>
    private bool ValidateMissionSettings()
    {
        bool isValid = true;

        if (missionPanel == null)
        {
            Debug.LogError(
                "MissionUIManagerのMission Panelが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (progressText == null)
        {
            Debug.LogError(
                "MissionUIManagerのProgress Textが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (!ValidateMissionTimerSettings())
        {
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// ミッションタイマーの設定確認。
    /// </summary>
    private bool ValidateMissionTimerSettings()
    {
        bool isValid = true;

        if (missionTimerTensImage == null)
        {
            Debug.LogError(
                "MissionUIManagerの" +
                "Mission Timer Tens Imageが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (missionTimerOnesImage == null)
        {
            Debug.LogError(
                "MissionUIManagerの" +
                "Mission Timer Ones Imageが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (!ValidateNumberSprites())
        {
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 成功タイマーの設定確認。
    /// </summary>
    private bool ValidateSuccessTimerSettings()
    {
        bool isValid = true;

        if (successTimerTensImage == null)
        {
            Debug.LogError(
                "MissionUIManagerの" +
                "Success Timer Tens Imageが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (successTimerOnesImage == null)
        {
            Debug.LogError(
                "MissionUIManagerの" +
                "Success Timer Ones Imageが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (!ValidateNumberSprites())
        {
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 失敗タイマーの設定確認。
    /// </summary>
    private bool ValidateFailureTimerSettings()
    {
        bool isValid = true;

        if (failureTimerTensImage == null)
        {
            Debug.LogError(
                "MissionUIManagerの" +
                "Failure Timer Tens Imageが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (failureTimerOnesImage == null)
        {
            Debug.LogError(
                "MissionUIManagerの" +
                "Failure Timer Ones Imageが" +
                "設定されていません。"
            );

            isValid = false;
        }

        if (!ValidateNumberSprites())
        {
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 数字スプライトの設定確認。
    /// </summary>
    private bool ValidateNumberSprites()
    {
        if (numberSprites == null ||
            numberSprites.Length < 10)
        {
            Debug.LogError(
                "MissionUIManagerのNumber Spritesに" +
                "0～9の数字スプライトを設定してください。"
            );

            return false;
        }

        for (int i = 0; i < 10; i++)
        {
            if (numberSprites[i] == null)
            {
                Debug.LogError(
                    $"MissionUIManagerのNumber Sprites " +
                    $"Element {i}が設定されていません。"
                );

                return false;
            }
        }

        return true;
    }

    private void OnDisable()
    {
        isMissionDisplayed = false;
        isSuccessDisplayed = false;
        isFailureDisplayed = false;
    }
}