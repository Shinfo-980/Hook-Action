using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public TMP_Text timerText;
    [SerializeField] public float time;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (time >= 0)
        {
            time -= Time.deltaTime;
        }
        if (time <= 10)
        {
            timerText.color = Color.red;
            timerText.fontSize = 80;
        }
        else
        {
            timerText.color = Color.white;
            timerText.fontSize = 60;
        }


         timerText.text = Mathf.CeilToInt(time).ToString("00");
    }
}
