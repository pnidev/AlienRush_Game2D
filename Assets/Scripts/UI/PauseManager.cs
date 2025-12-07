using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;


public class PauseManager : MonoBehaviour
{
    private static PauseManager instance;
    private GameObject pausePanel;
    private GameObject settingsPanel;
    private bool isPaused = false;
    private bool inSettings = false;

    private void Awake()
    {
        // ĐẢM BẢO CHỈ CÓ 1 PAUSEMANAGER DÙ RESTART BAO NHIÊU LẦN
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Gắn lại mọi thứ khi scene mới load
        SceneManager.sceneLoaded += OnSceneLoaded;
        AssignReferencesAndButtons(); // thử lần đầu
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignReferencesAndButtons(); // QUAN TRỌNG: GẮN LẠI MỖI KHI LOAD SCENE MỚI
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) OpenPause();
            else if (inSettings) CloseSettings();
            else Resume();
        }
    }

    public void OpenPause()
    {
        if (pausePanel == null) return;
        isPaused = true;
        AudioManager.Instance?.PauseBGM();
        inSettings = false;
        pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SelectButton("PausePanel/hehe/ContinueButton");
    }

    public void Resume()
    {
        if (pausePanel == null) return;
        isPaused = false;
        AudioManager.Instance?.UnpauseBGM();
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.RestartCurrentBGM();

        // RESET LẠI LƯỢT CHƠI
        LevelRuntimeManager.I?.EndCurrentRun(false);

        // LẤY LEVEL HIỆN TẠI
        int map = LevelRuntimeManager.I.GetMapIndex();
        int level = LevelRuntimeManager.I.GetCurrentLevelIndex();

        // LOAD SCENE HIỆN TẠI
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // KHI SCENE LOAD XONG → START LẠI LEVEL
        StartCoroutine(RestartAfterLoad(map, level));
    }
    private IEnumerator RestartAfterLoad(int map, int level)
    {
        yield return null; // đợi 1 frame cho scene load xong
        yield return null;

        LevelRuntimeManager.I?.StartLevel(level);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null) return;
        inSettings = true;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
        SelectButton("SettingsPanel/Dark/CloseSettingsButton");
    }

    public void CloseSettings()
    {
        inSettings = false;
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        SelectButton("PausePanel/hehe/ContinueButton");
    }

    // TÌM ĐƯỢC CẢ OBJECT BỊ TẮT + HOẠT ĐỘNG MỌI LẦN LOAD SCENE
    private void AssignReferencesAndButtons()
    {
        // Cách duy nhất tìm được object bị SetActive(false)
        pausePanel = FindObjectInScene("PausePanel");
        settingsPanel = FindObjectInScene("SettingsPanel");

        if (pausePanel == null || settingsPanel == null)
        {
            Debug.LogError("PauseManager: Không tìm thấy PausePanel hoặc SettingsPanel! Đảm bảo tên đúng.");
            return;
        }

        var hehe = pausePanel.transform.Find("hehe");
        if (hehe == null) return;

        // Gắn button
        AttachButton(hehe, "ContinueButton", Resume);
        AttachButton(hehe, "choilai", RestartLevel);
        AttachButton(hehe, "Settings", OpenSettings);

        var closeBtn = settingsPanel.transform.Find("Dark/CloseSettingsButton")?.GetComponent<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(CloseSettings);
        }

        // Ẩn panel
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        Debug.Log("PauseManager: ĐÃ GẮN LẠI THÀNH CÔNG SAU KHI RESTART!");
    }

    private GameObject FindObjectInScene(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
        {
            if (go.name == name && go.scene.isLoaded) // chỉ cần cái này là đủ
                return go;
        }
        return null;
    }

    private void AttachButton(Transform parent, string childName, UnityEngine.Events.UnityAction action)
    {
        var btn = parent.Find(childName)?.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    private void SelectButton(string path)
    {
        var obj = GameObject.Find(path);
        if (obj != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(obj);
    }
}