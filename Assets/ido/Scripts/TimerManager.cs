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
        time -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(time).ToString();

        if(time <= 10)
        {
            timerText.color = Color.red;
        }
    }
}
