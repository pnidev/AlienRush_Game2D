using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Nhi_UIManagerInMap : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI runScoreText;

    bool subscribed = false;
    LevelRuntimeManager subscribedInstance = null;

    public TextMeshProUGUI bestScoreText;

    void OnEnable()
    {
        // ensure we try to subscribe whenever this object becomes enabled
        SceneManager.sceneLoaded += OnSceneLoaded;
        TrySubscribe();
    }

    void OnDisable()
    {
        // unsubscribe when disabled to avoid dangling delegates
        SceneManager.sceneLoaded -= OnSceneLoaded;
        TryUnsubscribe();
    }

    void Start()
    {
        // initial UI refresh
        UpdateBestScore();

        // If LRM is created after UI Start, TrySubscribe will be invoked again via OnSceneLoaded / OnEnable
        if (subscribed)
            RefreshAllTasksUI();
    }

    void Update()
    {
        if (Nhi_ScoreManager.I != null && runScoreText != null)
            runScoreText.text = $"{Nhi_ScoreManager.I.CurrentRunScore}";

        // if not yet subscribed but LRM exists, try to subscribe (safety net)
        if (!subscribed && LevelRuntimeManager.I != null)
            TrySubscribe();
    }

    // Called when any scene finishes loading (helps re-subscribe after scene switches)
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // small safeguard: try re-subscribing if needed
        if (!subscribed)
            TrySubscribe();
    }

    // ----- subscribe helpers -----
    void TrySubscribe()
    {
        if (subscribed) return;

        if (LevelRuntimeManager.I == null)
        {
            Debug.Log("[UI] TrySubscribe: LevelRuntimeManager.I is null (will retry on scene load)");
            return;
        }

        subscribedInstance = LevelRuntimeManager.I;

        // defensive: remove first to avoid double-subscribe
        subscribedInstance.OnTaskProgressChanged -= OnTaskProgressChanged;
        subscribedInstance.OnTaskCompleted -= OnTaskCompleted;
        subscribedInstance.OnLevelStarted -= OnLevelStarted;
        subscribedInstance.OnLevelCompletedEvent -= OnLevelCompleted;
        subscribedInstance.OnNoActiveLevel -= OnNoActiveLevelHandler;

        // then add
        subscribedInstance.OnTaskProgressChanged += OnTaskProgressChanged;
        subscribedInstance.OnTaskCompleted += OnTaskCompleted;
        subscribedInstance.OnLevelStarted += OnLevelStarted;
        subscribedInstance.OnLevelCompletedEvent += OnLevelCompleted;
        subscribedInstance.OnNoActiveLevel += OnNoActiveLevelHandler;

        subscribed = true;
        Debug.Log("[UI] Subscribed to LevelRuntimeManager events");

        RefreshAllTasksUI();
    }

    void TryUnsubscribe()
    {
        if (!subscribed) return;

        if (subscribedInstance != null)
        {
            subscribedInstance.OnTaskProgressChanged -= OnTaskProgressChanged;
            subscribedInstance.OnTaskCompleted -= OnTaskCompleted;
            subscribedInstance.OnLevelStarted -= OnLevelStarted;
            subscribedInstance.OnLevelCompletedEvent -= OnLevelCompleted;
            subscribedInstance.OnNoActiveLevel -= OnNoActiveLevelHandler;
        }

        subscribedInstance = null;
        subscribed = false;
        Debug.Log("[UI] Unsubscribed from LevelRuntimeManager events");
    }

    void OnNoActiveLevelHandler()
    {
        if (taskText != null) taskText.gameObject.SetActive(false);
        Debug.Log("[UI] OnNoActiveLevel -> hide task UI");
    }

    // ===== EVENT HANDLERS =====
    void OnTaskProgressChanged(string id, int cur, int target)
    {
        if (taskText == null) return;

        string display = id;
        var cfg = LevelRuntimeManager.I?.GetCurrentLevelConfig();
        if (cfg != null && cfg.tasks != null)
        {
            var t = cfg.tasks.FirstOrDefault(x => x.id == id);
            if (t.id != null)
                display = !string.IsNullOrEmpty(t.note) ? t.note : t.id;
        }

        taskText.text = $"{display}: {cur}/{target}";
        Debug.Log($"[UI] OnTaskProgressChanged -> {display}: {cur}/{target}");
    }

    void OnTaskCompleted(string id)
    {
        if (taskText == null) return;

        string display = id;
        var cfg = LevelRuntimeManager.I?.GetCurrentLevelConfig();
        if (cfg != null && cfg.tasks != null)
        {
            var t = cfg.tasks.FirstOrDefault(x => x.id == id);
            if (t.id != null)
                display = !string.IsNullOrEmpty(t.note) ? t.note : t.id;
        }
        taskText.text = $"{display} - Completed";
        UpdateBestScore();
        Debug.Log($"[UI] OnTaskCompleted -> {display} completed");
    }

    void OnLevelStarted(int map, int level)
    {
        if (level == -1)
        {
            OnNoActiveLevelHandler();
            return;
        }

        if (taskText != null) taskText.gameObject.SetActive(true);
        RefreshAllTasksUI();
        UpdateBestScore();
        Debug.Log($"[UI] OnLevelStarted map={map} level={level}");
    }

    void OnLevelCompleted(int map, int level)
    {
        if (taskText != null) taskText.text = "Level Completed!";
        UpdateBestScore();
        Debug.Log($"[UI] OnLevelCompleted map={map} level={level}");
    }

    // ----- UI helpers -----
    void RefreshAllTasksUI()
    {
        if (taskText == null) return;
        var lrm = LevelRuntimeManager.I;
        if (lrm == null) { taskText.text = "No LevelRuntimeManager"; return; }

        var cfg = lrm.GetCurrentLevelConfig();
        if (cfg == null)
        {
            if (taskText != null) taskText.gameObject.SetActive(false);
            return;
        }

        if (cfg.tasks == null || cfg.tasks.Length == 0) { taskText.text = "No tasks"; return; }

        var sb = new System.Text.StringBuilder();
        foreach (var task in cfg.tasks)
        {
            var p = lrm.GetTaskProgress(task.id);
            var friendly = !string.IsNullOrEmpty(task.note) ? task.note : (string.IsNullOrEmpty(task.id) ? "<no-id>" : task.id);
            sb.AppendLine($"{friendly}: {p.cur}/{p.target}");
        }
        taskText.text = sb.ToString().TrimEnd();
        Debug.Log("[UI] RefreshAllTasksUI written to taskText");
    }

    void UpdateBestScore()
    {
        if (bestScoreText == null) return;
        if (LevelRuntimeManager.I == null)
        {
            bestScoreText.text = "";
            return;
        }

        int map = LevelRuntimeManager.I.GetMapIndex();
        int best = GameSave.GetBestScoreForMap(map);
        bestScoreText.text = $"Best (Map {map}): {best}";
    }
}
