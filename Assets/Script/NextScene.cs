using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    public void GoToScene2()
    {
        SceneManager.LoadScene("Scene2");
    }
}