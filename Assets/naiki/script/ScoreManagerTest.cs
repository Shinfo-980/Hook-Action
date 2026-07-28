using System.Collections;//←追加しました
using UnityEngine;
using UnityEngine.UI;

public class ScoreManagerTest : MonoBehaviour
{
    public static ScoreManagerTest instance;

    [Header("Score")]
    [SerializeField] private int score = 0;

    private int scoreMultiplier = 1;//←追加しました

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
        Debug.Log("現在の倍率:" + scoreMultiplier);
        score = Mathf.Clamp(score + value * scoreMultiplier, 0, MaxScore);
        //↑score = Mathf.Clamp(score + value, 0, MaxScore);から
        //  score = Mathf.Clamp(score + value * scoreMultiplier, 0, MaxScore);
        //* scoreMultiplier,を追加しました
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
    public void DoubleScore(float time)//←ここから下追加しました
    {
        Debug.Log("二倍開始");
        StartCoroutine(DoubleScoreCoroutine(time));
    }
    IEnumerator DoubleScoreCoroutine(float time)
    {
        scoreMultiplier = 2;
        yield return new WaitForSeconds(time);
        scoreMultiplier = 1;
    }//←ここまで追加しました
}