public static class ResultData
{
    private static int finalScore = 0;

    /// <summary>
    /// リザルト画面に渡す最終スコアを保存する。
    /// </summary>
    public static void SetFinalScore(int score)
    {
        finalScore = score;
    }

    /// <summary>
    /// 保存されている最終スコアを取得する。
    /// </summary>
    public static int GetFinalScore()
    {
        return finalScore;
    }

    /// <summary>
    /// 保存されている最終スコアを初期化する。
    /// </summary>
    public static void ResetFinalScore()
    {
        finalScore = 0;
    }
}