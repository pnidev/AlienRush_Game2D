using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathPanel : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f; // bỏ pause
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
