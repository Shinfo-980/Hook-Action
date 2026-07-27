using UnityEngine;
using UnityEngine.SceneManagement;

public class ToGameScene : MonoBehaviour
{
    [SerializeField] private string GameScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {

        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(GameScene);
        }
    }
}
