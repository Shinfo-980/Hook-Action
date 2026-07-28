using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveScene : MonoBehaviour
{
    [SerializeField] private string sceneName;
    public void LoadScene()
    {
        Debug.Log("指定のシーンへ移動");
        SceneManager.LoadScene(sceneName);
    }
}
