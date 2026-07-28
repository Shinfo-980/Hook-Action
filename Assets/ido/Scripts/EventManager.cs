using System.Collections;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private TimerManager TimerManager;
    public void missionEvent()
    {
        Debug.Log("ミッションイベント発生");

        StartCoroutine(EventTime(50));
    }

    public void scoreUp()
    {
        Debug.Log("スコアアップイベント発生");

        StartCoroutine(EventTime(30));
    }

    

    public IEnumerator EventTime(int wait)
    {
        yield return new WaitForSeconds(wait);
        Debug.Log("イベント終了");
    }
    
}
