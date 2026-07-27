using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private int countScore = 0;
    void Awake()
    {
        instance = this;
    }
    public void AddScore(int point)
    {
        countScore += point;
        Debug.Log("現在のスコア:" + countScore);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
