using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [System.Serializable]
    public class ScoreDigitPattern
    {
        [Header("表示パターンの親オブジェクト")]
        public GameObject patternObject;

        [Header("左から順番に数字Imageを設定")]
        public Image[] digitImages;
    }

    [Header("0点用 + 3桁～8桁用の表示パターン")]
    [SerializeField] private ScoreDigitPattern[] scoreDigitPatterns;

    [Header("数字スプライト 0～9")]
    [SerializeField] private Sprite[] numberSprites = new Sprite[10];

    [Header("表示可能な最大スコア")]
    [SerializeField] private int maximumScore = 99999999;

    private int finalScore;

    private void Start()
    {
        // リザルト画面ではマウスカーソルを表示する
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisableAllPatterns();

        finalScore = ResultData.GetFinalScore();

        DisplayFinalScore(finalScore);
    }

    private void DisplayFinalScore(int score)
    {
        if (!ValidateSettings())
        {
            return;
        }

        score = Mathf.Clamp(score, 0, maximumScore);

        string scoreText = score.ToString();

        int patternIndex = GetPatternIndex(score);

        if (patternIndex < 0 ||
            patternIndex >= scoreDigitPatterns.Length)
        {
            Debug.LogError(
                $"スコア {score} に対応する表示パターンがありません。"
            );

            return;
        }

        DisableAllPatterns();

        ScoreDigitPattern selectedPattern =
            scoreDigitPatterns[patternIndex];

        if (selectedPattern == null)
        {
            Debug.LogError(
                $"Score Digit PatternsのElement {patternIndex}が設定されていません。"
            );

            return;
        }

        if (selectedPattern.patternObject == null)
        {
            Debug.LogError(
                $"Score Digit PatternsのElement {patternIndex}にPattern Objectが設定されていません。"
            );

            return;
        }

        if (selectedPattern.digitImages == null ||
            selectedPattern.digitImages.Length < scoreText.Length)
        {
            Debug.LogError(
                $"スコア {score} を表示するためのDigit Imagesが不足しています。"
            );

            return;
        }

        selectedPattern.patternObject.SetActive(true);

        for (int i = 0; i < scoreText.Length; i++)
        {
            int number = scoreText[i] - '0';

            Image digitImage =
                selectedPattern.digitImages[i];

            if (digitImage == null)
            {
                Debug.LogError(
                    $"Element {patternIndex}のDigit Images Element {i}が設定されていません。"
                );

                continue;
            }

            digitImage.gameObject.SetActive(true);
            digitImage.sprite = numberSprites[number];
        }

        Debug.Log(
            $"リザルトスコアを表示しました：{score}"
        );
    }

    private int GetPatternIndex(int score)
    {
        if (score == 0)
        {
            return 0;
        }

        int digitCount = score.ToString().Length;

        switch (digitCount)
        {
            case 3:
                return 1;

            case 4:
                return 2;

            case 5:
                return 3;

            case 6:
                return 4;

            case 7:
                return 5;

            case 8:
                return 6;

            default:
                Debug.LogError(
                    $"スコア {score} は対応範囲外です。0点または3桁～8桁のスコアにしてください。"
                );

                return -1;
        }
    }

    private void DisableAllPatterns()
    {
        if (scoreDigitPatterns == null)
        {
            return;
        }

        for (int i = 0;
             i < scoreDigitPatterns.Length;
             i++)
        {
            ScoreDigitPattern pattern =
                scoreDigitPatterns[i];

            if (pattern == null)
            {
                continue;
            }

            if (pattern.patternObject != null)
            {
                pattern.patternObject.SetActive(false);
            }
        }
    }

    private bool ValidateSettings()
    {
        if (numberSprites == null ||
            numberSprites.Length < 10)
        {
            Debug.LogError(
                "ResultManagerのNumber Spritesに0～9のスプライトを設定してください。"
            );

            return false;
        }

        if (scoreDigitPatterns == null ||
            scoreDigitPatterns.Length < 7)
        {
            Debug.LogError(
                "ResultManagerのScore Digit Patternsを7個用意してください。"
            );

            return false;
        }

        return true;
    }

    public void SetResultScore(int score)
    {
        finalScore = Mathf.Clamp(
            score,
            0,
            maximumScore
        );

        ResultData.SetFinalScore(finalScore);

        DisplayFinalScore(finalScore);
    }

    public int GetFinalScore()
    {
        return finalScore;
    }
}