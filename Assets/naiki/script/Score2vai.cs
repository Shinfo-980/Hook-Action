using System.Collections;
using UnityEngine;

public class Score2vai : MonoBehaviour
{
    [SerializeField] private float doubleTime = 15f;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("スコア２倍");
            ScoreManagerTest.instance.DoubleScore(doubleTime);
            Destroy(gameObject);
        }
       
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
