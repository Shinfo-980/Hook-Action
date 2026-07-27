using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Score")]
    [SerializeField] private int score = 0;

    [Header("Digit Images（左から右）")]
    [SerializeField] private Image[] digitImages = new Image[8];

    [Header("Number Sprites（0～9）")]
    [SerializeField] private Sprite[] numberSprites = new Sprite[10];

    private const int MaxScore = 99999999;

    private void Awake()
    {
        // ScoreManagerが複数存在していないか確認
        if (instance != null && instance != this)
        {
            Debug.LogWarning("ScoreManagerが複数存在するため、重複した方を削除します。");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        UpdateScoreDisplay();
    }

    public void AddScore(int value)
    {
        score = Mathf.Clamp(score + value, 0, MaxScore);
        UpdateScoreDisplay();
    }

    public void SetScore(int value)
    {
        score = Mathf.Clamp(value, 0, MaxScore);
        UpdateScoreDisplay();
    }

    public int GetScore()
    {
        return score;
    }

    private void UpdateScoreDisplay()
    {
        if (digitImages == null || digitImages.Length != 8)
        {
            Debug.LogError("Digit Imagesには8個のImageを設定してください。");
            return;
        }

        if (numberSprites == null || numberSprites.Length < 10)
        {
            Debug.LogError("Number Spritesには0～9のSpriteを設定してください。");
            return;
        }

        int tempScore = score;

        // 右端から1桁ずつ画像を設定
        for (int i = digitImages.Length - 1; i >= 0; i--)
        {
            int digit = tempScore % 10;

            if (digitImages[i] != null)
            {
                digitImages[i].sprite = numberSprites[digit];
            }

            tempScore /= 10;
        }
    }
}