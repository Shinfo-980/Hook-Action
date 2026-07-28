using UnityEngine;
using UnityEngine.UI;
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

    [Header("Scripts")]
    [SerializeField] private EventManager EventManager;

    private void Start()
    {
        UpdateTimerDisplay();
        StartCoroutine(EventWait(90,"mission"));
        StartCoroutine(EventWait(210,"scoreUp"));
    }

    private void Update()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;

            if (time < 0)
                time = 0;
        }

        UpdateTimerDisplay();

        
    }

    private void UpdateTimerDisplay()
    {
        int displayTime = Mathf.CeilToInt(time);

        displayTime = Mathf.Clamp(displayTime, 0, 999);

        int hundreds = displayTime / 100;
        int tens = (displayTime / 10) % 10;
        int ones = displayTime % 10;

        hundredsImage.sprite = numberSprites[hundreds];
        tensImage.sprite = numberSprites[tens];
        onesImage.sprite = numberSprites[ones];

        Color currentColor = (displayTime <= 10) ? warningColor : normalColor;

        hundredsImage.color = currentColor;
        tensImage.color = currentColor;
        onesImage.color = currentColor;
    }

    public float GetTime()
    {
        return time;
    }

    public void SetTime(float value)
    {
        time = Mathf.Clamp(value, 0, 999);
        UpdateTimerDisplay();
    }

    public void AddTime(float value)
    {
        time = Mathf.Clamp(time + value, 0, 999);
        UpdateTimerDisplay();
    }

    public void Event(string Event)
    {
        if(Event == "mission")
        {
            EventManager.missionEvent();
        }
        if(Event == "scoreUp")
        {
            EventManager.scoreUp();
        }
    }

    public IEnumerator EventWait(int waitTime,string Event)
    {     
        yield return new WaitForSeconds(waitTime);
        this.Event(Event);
    }
}