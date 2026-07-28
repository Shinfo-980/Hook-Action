using UnityEngine;

public class JumpUP : MonoBehaviour
{
    [SerializeField] private float jumpAmount = 0.5f;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerMovementTest player = other.GetComponent<PlayerMovementTest>();
                if(player != null)
            {
                player.jumpUp(jumpAmount);
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
