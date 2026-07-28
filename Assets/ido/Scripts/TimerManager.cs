using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    [Header("Time")]
    [SerializeField] private float time = 240f;

    [Header("Digit Images")]
    [SerializeField] private Image hundredsImage;
    [SerializeField] private Image tensImage;
    [SerializeField] private Image onesImage;

    [Header("Number Sprites (0～9)")]
    [SerializeField] private Sprite[] numberSprites = new Sprite[10];

    [Header("Warning")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;

    [Header("Result Scene")]
    [SerializeField] private string resultSceneName = "ResultScene";

    [Header("Scripts")]
    [SerializeField] private EventManager eventManager;

    private bool isTimeUp = false;

    private void Start()
    {
        time = Mathf.Clamp(time, 0f, 999f);

        UpdateTimerDisplay();

        StartCoroutine(EventWait(90, "mission"));
        StartCoroutine(EventWait(210, "scoreUp"));
    }

    private void Update()
    {
        if (isTimeUp)
        {
            return;
        }

        if (time > 0f)
        {
            time -= Time.deltaTime;

            if (time <= 0f)
            {
                time = 0f;

                UpdateTimerDisplay();
                TimeUp();

                return;
            }
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (numberSprites == null ||
            numberSprites.Length < 10)
        {
            Debug.LogError(
                "TimerManagerのNumber Spritesに0～9の画像を設定してください。"
            );

            return;
        }

        if (hundredsImage == null ||
            tensImage == null ||
            onesImage == null)
        {
            Debug.LogError(
                "TimerManagerのDigit Imagesが設定されていません。"
            );

            return;
        }

        int displayTime = Mathf.CeilToInt(time);

        displayTime = Mathf.Clamp(
            displayTime,
            0,
            999
        );

        int hundreds = displayTime / 100;
        int tens = (displayTime / 10) % 10;
        int ones = displayTime % 10;

        hundredsImage.sprite =
            numberSprites[hundreds];

        tensImage.sprite =
            numberSprites[tens];

        onesImage.sprite =
            numberSprites[ones];

        Color currentColor =
            displayTime <= 10
                ? warningColor
                : normalColor;

        hundredsImage.color = currentColor;
        tensImage.color = currentColor;
        onesImage.color = currentColor;
    }

    private void TimeUp()
    {
        if (isTimeUp)
        {
            return;
        }

        isTimeUp = true;

        StopAllCoroutines();

        int finalScore = 0;

        if (ScoreManager.instance != null)
        {
            finalScore =
                ScoreManager.instance.GetScore();
        }
        else
        {
            Debug.LogError(
                "Scene内にScoreManagerが存在しないため、最終スコアを取得できませんでした。"
            );
        }

        ResultData.SetFinalScore(finalScore);

        Debug.Log(
            $"タイムアップ。最終スコア：{finalScore}"
        );

        if (string.IsNullOrWhiteSpace(resultSceneName))
        {
            Debug.LogError(
                "Result Scene Nameが設定されていません。"
            );

            return;
        }

        SceneManager.LoadScene(resultSceneName);
    }

    public float GetTime()
    {
        return time;
    }

    public void SetTime(float value)
    {
        if (isTimeUp)
        {
            return;
        }

        time = Mathf.Clamp(
            value,
            0f,
            999f
        );

        UpdateTimerDisplay();

        if (time <= 0f)
        {
            TimeUp();
        }
    }

    public void AddTime(float value)
    {
        if (isTimeUp)
        {
            return;
        }

        time = Mathf.Clamp(
            time + value,
            0f,
            999f
        );

        UpdateTimerDisplay();

        if (time <= 0f)
        {
            TimeUp();
        }
    }

    public void Event(string eventName)
    {
        if (isTimeUp)
        {
            return;
        }

        if (eventManager == null)
        {
            Debug.LogError(
                "TimerManagerにEventManagerが設定されていません。"
            );

            return;
        }

        if (eventName == "mission")
        {
            eventManager.missionEvent();
        }
        else if (eventName == "scoreUp")
        {
            eventManager.scoreUp();
        }
    }

    public IEnumerator EventWait(
        int waitTime,
        string eventName
    )
    {
        yield return new WaitForSeconds(waitTime);

        if (isTimeUp)
        {
            yield break;
        }

        Event(eventName);
    }
}