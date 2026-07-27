using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private int score = 0;

    [Header("Digit Images (左→右)")]
    [SerializeField] private Image[] digitImages = new Image[8];

    [Header("Number Sprites (0～9)")]
    [SerializeField] private Sprite[] numberSprites = new Sprite[10];

    private void Start()
    {
        UpdateScoreDisplay();
    }

    /// <summary>
    /// スコア加算
    /// </summary>
    public void AddScore(int value)
    {
        score += value;
        score = Mathf.Clamp(score, 0, 99999999);

        UpdateScoreDisplay();
    }

    /// <summary>
    /// スコア設定
    /// </summary>
    public void SetScore(int value)
    {
        score = Mathf.Clamp(value, 0, 99999999);

        UpdateScoreDisplay();
    }

    /// <summary>
    /// スコア取得
    /// </summary>
    public int GetScore()
    {
        return score;
    }

    private void UpdateScoreDisplay()
    {
        int temp = score;

        // 右端から1桁ずつ設定
        for (int i = digitImages.Length - 1; i >= 0; i--)
        {
            int digit = temp % 10;

            digitImages[i].sprite = numberSprites[digit];

            temp /= 10;
        }
    }
}