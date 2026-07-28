using UnityEngine;

public class SpeedUP : MonoBehaviour
{
    [SerializeField] private float boostSpeed = 2f;//ブーストする速度
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("移動速度UP");
        if (other.CompareTag("Player"))
        {
            PlayerMovementTest player = other.GetComponent<PlayerMovementTest>();
            if (player != null)
            {
                player.SpeedUp(boostSpeed);

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
