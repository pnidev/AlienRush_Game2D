using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static class để lưu thông tin scene đích khi chuyển qua loading
/// </summary>
public static class SceneTransition
{
    public static string targetScene = "";
    public static float minimumLoadTime = 1.5f;

    /// <summary>
    /// Chuyển đến scene đích qua LoadingScene
    /// </summary>
    public static void LoadSceneWithLoading(string sceneName, float minLoadTime = 1.5f)
    {
        targetScene = sceneName;
        minimumLoadTime = minLoadTime;
        SceneManager.LoadScene("Loading"); // Đổi thành "Loading"
    }
}
