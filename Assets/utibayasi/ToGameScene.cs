using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ToGameScene : MonoBehaviour
{
    [SerializeField] private string gameScene;

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            Debug.Log("キー入力を検知");

            SceneManager.LoadScene(gameScene);
        }
    }
}