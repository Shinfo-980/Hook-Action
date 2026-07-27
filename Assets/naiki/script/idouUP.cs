using UnityEngine;

public class idouUP : MonoBehaviour
{
    [SerializeField] private float boostSpeed = 20f;//ブーストする速度
    [SerializeField] private float boostTime = 5f;//ブーストする時間
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("移動速度UP");
        if (other.CompareTag("Player"))
        { 
            PlayerTest player = other.GetComponent<PlayerTest>();
            if (player != null)
            {
                player.SpeedUp(boostSpeed, boostTime);

            }
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
