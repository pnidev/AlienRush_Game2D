using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUIController : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject winUIPanel;

    [Header("Scene Settings")]
    public string mapSelectSceneName = "MapSelectScene";

    private void Start()
    {
        if (winUIPanel != null)
            winUIPanel.SetActive(false);
    }

    // Hàm Chest sẽ gọi
    public void ShowWinUI()
    {
        if (winUIPanel != null)
            winUIPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // Hàm gọi từ Button Back
    public void BackToMapSelect()
    {
        Time.timeScale = 1f;

        // Tắt script cursor trong gameplay
        CursorManagement cursorMgr = FindObjectOfType<CursorManagement>();
        if (cursorMgr != null)
            cursorMgr.enabled = false;

        // Trả lại chuột mặc định
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Load scene
        if (LevelRuntimeManager.I != null)
        {
            LevelRuntimeManager.I.GoToMapSelect();
            return;
        }

        SceneManager.LoadScene(mapSelectSceneName);
    }

}
