using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Nhi_ScoreManager))]
public class LevelRuntimeManager : MonoBehaviour
{
    public static LevelRuntimeManager I { get; private set; }

    [Header("Config")]
    public MapConfig_SO mapConfig;
    public int currentLevelIndex = 0;  // 0..4 hoặc 0..5 tùy map

    // Runtime
    LevelConfig_SO currentLevelConfig;

    Dictionary<string, int> progress = new Dictionary<string, int>();
    Dictionary<string, int> targets = new Dictionary<string, int>();
    Dictionary<string, bool> done = new Dictionary<string, bool>();

    bool waitingForBoss = false;
    bool levelCompleted = false;

    // ===== SPECIAL TASK RUNTIME STATE =====
    bool forbidPickActive = false;
    bool forbidPickViolated = false;

    bool noCollisionActive = false;
    bool noCollisionViolated = false;

    bool noDamageActive = false;
    bool noDamageViolated = false;

    bool noTurnRightActive = false;
    float noTurnRightDuration = 0f;
    bool noTurnRightViolated = false;
    Coroutine noTurnRightCoroutine = null;

    // runtime additional for "require right turn" behavior
    bool requireTurnRightActive = false;
    bool turnRightSeen = false;


    // ===== EVENTS (UI HOOK) =====
    public event Action<string, int, int> OnTaskProgressChanged;
    public event Action<string> OnTaskCompleted;
    public event Action<int, int> OnLevelStarted;       // map, level
    public event Action<int, int> OnLevelCompletedEvent; // map, level

    public event Action OnNoActiveLevel;

    // ===== BOSS / SCENE SWITCH STATE =====
    string bossReturnScene = null;   // scene gameplay gốc để quay về (ta sẽ không dùng để quay lại trong yêu cầu của bạn, nhưng vẫn lưu)
    string bossSceneToLoad = null;   // scene boss được chọn
    bool isInBossFight = false;      // đang trong boss fight (đang ở scene boss)

    // ============================================================
    // INIT
    // ============================================================
    void Awake()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        this.enabled = true;
        DontDestroyOnLoad(gameObject);

        try
        {
            if (gameObject.scene.name == "DontDestroyOnLoad")
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        }
        catch { }

        if (I == null)
        {
            I = this;
        }
        else if (I != this)
        {
            // trước khi destroy instance cũ, clear event handlers trên nó để tránh dangling delegates
            try
            {
                I.ClearAllEventHandlers();
            }
            catch { }

            Destroy(I.gameObject);
            I = this;
        }
    }



    void Start()
    {
        if (mapConfig == null)
        {
            Debug.LogError("[LRM] mapConfig không được gán!");
            return;
        }

        Debug.Log($"[LRM] Start() this={GetInstanceID()} gameObject={gameObject.name} mapConfig={mapConfig.name} mapIndex={mapConfig.mapIndex} levelsCount={mapConfig.levels.Length} lastPlayed={GameSave.GetLastPlayed()} currentLevelIndex(inspector)={currentLevelIndex}");

        var last = GameSave.GetLastPlayed();
        if (last.map == mapConfig.mapIndex)
        {
            currentLevelIndex = last.level;
            Debug.Log($"[LRM] Applying lastPlayed -> currentLevelIndex={currentLevelIndex}");
        }

        // clamp index
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, mapConfig.levels.Length - 1);

        // NEW: Skip levels đã complete, tìm level đầu tiên chưa done
        int map = mapConfig.mapIndex;
        bool allCompleted = true;
        for (int i = currentLevelIndex; i < mapConfig.levels.Length; i++)
        {
            if (!GameSave.IsLevelCompleted(map, i))
            {
                currentLevelIndex = i;
                allCompleted = false;
                Debug.Log($"[LRM] Skip to uncompleted level {i}");
                break;
            }
        }

        if (allCompleted)
        {
            Debug.Log("[LRM] All levels in map completed! No active level.");
            OnNoActiveLevel?.Invoke();  // Trigger event cho UI handle
            return;  // Không StartLevel
        }

        // bắt đầu level chưa done
        StartLevel(currentLevelIndex);
    }


    // ============================================================
    // START LEVEL
    // ============================================================
    public void StartLevel(int levelIndex)
    {
        if (mapConfig == null)
        {
            Debug.LogError("[LRM] StartLevel called but mapConfig is null.");
            return;
        }
        if (mapConfig.levels == null || mapConfig.levels.Length == 0)
        {
            Debug.LogError($"[LRM] mapConfig.levels is null/empty for map {mapConfig.mapIndex}.");
            return;
        }
        if (levelIndex < 0 || levelIndex >= mapConfig.levels.Length)
        {
            Debug.LogError("[LRM] levelIndex ngoài phạm vi!");
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevelConfig = mapConfig.levels[levelIndex];

        if (currentLevelConfig == null)
        {
            Debug.LogError($"[LRM] currentLevelConfig == null for map {mapConfig.mapIndex} level {levelIndex}.");
            // fallback first non-null
            for (int i = 0; i < mapConfig.levels.Length; i++)
                if (mapConfig.levels[i] != null) { currentLevelIndex = i; currentLevelConfig = mapConfig.levels[i]; Debug.LogWarning($"[LRM] Falling back to levelIndex={i}"); break; }
            if (currentLevelConfig == null) { Debug.LogError("[LRM] No valid LevelConfig found — aborting StartLevel."); return; }
        }

        if (currentLevelConfig.tasks == null || currentLevelConfig.tasks.Length == 0)
            Debug.LogWarning($"[LRM] Level {currentLevelIndex} has no tasks (null/empty). UI will show 'no config'.");

        InitFromConfig(currentLevelConfig);
        CheckAllTasks();
        Nhi_ScoreManager.I?.ResetRunScoreForMap(mapConfig.mapIndex);

        Debug.Log($"[LRM] Bắt đầu Map {mapConfig.mapIndex} - Level {currentLevelIndex}: {currentLevelConfig.displayName}");
        OnLevelStarted?.Invoke(mapConfig.mapIndex, currentLevelIndex);
    }

    public void ClearAllEventHandlers()
    {
        OnTaskProgressChanged = null;
        OnTaskCompleted = null;
        OnLevelStarted = null;
        OnLevelCompletedEvent = null;
        OnNoActiveLevel = null;
    }
    // ============================================================
    // INIT TASK STATE
    // ============================================================
    void InitFromConfig(LevelConfig_SO cfg)
    {
        progress.Clear();
        targets.Clear();
        done.Clear();

        waitingForBoss = false;
        levelCompleted = false;

        // Reset special flags
        forbidPickActive = forbidPickViolated = false;
        noCollisionActive = noCollisionViolated = false;
        noDamageActive = noDamageViolated = false;
        noTurnRightActive = noTurnRightViolated = false;
        noTurnRightDuration = 0f;

        if (noTurnRightCoroutine != null)
        {
            StopCoroutine(noTurnRightCoroutine);
            noTurnRightCoroutine = null;
        }

        if (cfg == null || cfg.tasks == null) return;

        // Init normal tasks
        foreach (var t in cfg.tasks)
        {
            progress[t.id] = 0;

            // For ScoreAtLeast we want the target to be the actual score threshold (e.g. 100).
            // But keep previous behavior: at least 1 if someone mistakenly left target=0.
            targets[t.id] = Mathf.Max(1, t.target);

            done[t.id] = false;
        }

        // Init special tasks
        foreach (var t in cfg.tasks)
        {
            switch (t.type)
            {
                case TaskType.NoPickItems:
                    forbidPickActive = true;
                    forbidPickViolated = false;
                    break;

                case TaskType.NoCollisionRun
:
                    noCollisionActive = true;
                    noCollisionViolated = false;
                    break;

                case TaskType.NoDamageRun:
                    noDamageActive = true;
                    noDamageViolated = false;
                    break;

                case TaskType.NoTurnRightForSeconds:
                    noTurnRightActive = true;
                    noTurnRightDuration = Mathf.Max(1, t.target);
                    noTurnRightViolated = false;

                    if (noTurnRightCoroutine != null)
                        StopCoroutine(noTurnRightCoroutine);

                    noTurnRightCoroutine = StartCoroutine(NoTurnRightTimer(noTurnRightDuration, t.id));
                    break;
            }
        }

        BroadcastAllTaskStates();
    }

    void BroadcastAllTaskStates()
    {
        foreach (var kv in progress)
            OnTaskProgressChanged?.Invoke(kv.Key, kv.Value, targets[kv.Key]);
    }

    // ============================================================
    // ADD PROGRESS
    // ============================================================
    public void AddProgressByType(TaskType type, int amount = 1)
    {
        if (currentLevelConfig == null) return;

        var task = currentLevelConfig.tasks.FirstOrDefault(x => x.type == type);

        if (task.id == null)
        {
            Debug.Log($"[LRM] Level này KHÔNG có task {type}");
            return;
        }

        AddProgress(task.id, amount);
    }

    public void AddProgress(string id, int amount = 1)
    {
        if (levelCompleted) return;
        if (!progress.ContainsKey(id))
        {
            Debug.LogWarning("[LRM] Progress không tồn tại task id: " + id);
            return;
        }

        progress[id] += amount;
        if (progress[id] > targets[id]) progress[id] = targets[id];

        OnTaskProgressChanged?.Invoke(id, progress[id], targets[id]);

        if (!done[id] && progress[id] >= targets[id])
        {
            done[id] = true;
            OnTaskCompleted?.Invoke(id);
            Debug.Log("[LRM] Task hoàn thành: " + id);
        }

        CheckAllTasks();
    }

    // ============================================================
    // SPECIAL TASK HANDLERS
    // ============================================================
    // Gọi khi player "turn" — isRightTurn = true nếu là RightArrow
    public void ReportPlayerTurned(bool isRightTurn)
    {
        // Nếu requirement không active thì ignore hoàn toàn
        if (!requireTurnRightActive) return;

        if (isRightTurn)
        {
            turnRightSeen = true;
            Debug.Log("[LRM] Player turned RIGHT during required window -> mark seen");
        }
        else
        {
            Debug.Log("[LRM] Player turned but not counted as RIGHT (ignored)");
        }
    }

    public void CancelNoTurnRightNow()
    {
        // Stop coroutine nếu đang chạy
        if (noTurnRightCoroutine != null)
        {
            try
            {
                StopCoroutine(noTurnRightCoroutine);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LRM] CancelNoTurnRightNow: StopCoroutine failed: " + ex.Message);
            }
            noTurnRightCoroutine = null;
        }

        // Tắt runtime flag
        requireTurnRightActive = false;

        // reset violation flag để cho phép trigger lại
        noTurnRightViolated = false;

        // reset progress for the task to 0 và notify UI (nếu task tồn tại)
        if (currentLevelConfig != null && currentLevelConfig.tasks != null)
        {
            var t = currentLevelConfig.tasks.FirstOrDefault(x => x.type == TaskType.NoTurnRightForSeconds);
            if (!string.IsNullOrEmpty(t.id))
            {
                if (progress.ContainsKey(t.id))
                {
                    progress[t.id] = 0;
                    int tgt = targets.ContainsKey(t.id) ? targets[t.id] : Mathf.Max(1, t.target);
                    OnTaskProgressChanged?.Invoke(t.id, 0, tgt);
                }
            }
        }

        Debug.Log("[LRM] CancelNoTurnRightNow called: timer stopped and progress reset to 0");
    }

    public void TriggerNoTurnRightNow()
    {
        if (currentLevelConfig == null || currentLevelConfig.tasks == null)
        {
            Debug.LogWarning("[LRM] Không có level config để trigger NoTurnRight.");
            return;
        }

        var t = currentLevelConfig.tasks.FirstOrDefault(x => x.type == TaskType.NoTurnRightForSeconds);
        if (string.IsNullOrEmpty(t.id))
        {
            Debug.Log("[LRM] Level này không có task NoTurnRightForSeconds.");
            return;
        }

        // reset runtime flags
        requireTurnRightActive = true;
        turnRightSeen = false;
        noTurnRightViolated = false;
        noTurnRightDuration = Mathf.Max(1, t.target);

        // đảm bảo task có entry trong progress/targets
        if (!progress.ContainsKey(t.id))
            progress[t.id] = 0;
        if (!targets.ContainsKey(t.id))
            targets[t.id] = Mathf.Max(1, t.target);

        // reset progress cho UI ngay lập tức
        progress[t.id] = 0;
        OnTaskProgressChanged?.Invoke(t.id, 0, targets[t.id]);

        // stop existing coroutine (nếu có)
        if (noTurnRightCoroutine != null)
        {
            try
            {
                StopCoroutine(noTurnRightCoroutine);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LRM] TriggerNoTurnRightNow: StopCoroutine failed: " + ex.Message);
            }
            noTurnRightCoroutine = null;
        }

        Debug.Log($"[LRM] Trigger RequireRightTurn NOW for task {t.id} duration={noTurnRightDuration}s");
        noTurnRightCoroutine = StartCoroutine(NoTurnRightTimer(noTurnRightDuration, t.id));
    }


    public void ReportCollisionWithObstacle()
    {
        if (noCollisionActive)
        {
            noCollisionViolated = true;
            Debug.Log("[LRM] VI PHẠM NoCollision!");
        }
    }

    public void ReportPlayerTookDamage(int amount)
    {
        if (noDamageActive && amount > 0)
        {
            noDamageViolated = true;
            Debug.Log("[LRM] VI PHẠM NoDamageRun!");
        }
    }

    public void ReportPlayerTurnedRight()
    {
        if (noTurnRightActive)
        {
            noTurnRightViolated = true;
            Debug.Log("[LRM] VI PHẠM NoTurnRight!");
        }
    }

    public void ReportPlayerPickup(string itemId = "")
    {
        if (forbidPickActive)
        {
            forbidPickViolated = true;
            Debug.Log("[LRM] VI PHẠM NoPickItems!");
        }
    }

    System.Collections.IEnumerator NoTurnRightTimer(float seconds, string taskId)
    {
        float elapsed = 0f;
        int secondsCounted = 0;
        int target = targets.ContainsKey(taskId) ? targets[taskId] : Mathf.FloorToInt(seconds);

        // Mark coroutine reference locally so we can clear the shared ref reliably on exit
        var thisCoroutine = noTurnRightCoroutine;

        // Ensure UI starts from 0
        OnTaskProgressChanged?.Invoke(taskId, secondsCounted, target);
        Debug.Log($"[LRM] NoTurnRightTimer started for {taskId} target={target}");

        while (elapsed < seconds)
        {
            // Nếu requirement bị hủy thì dừng ngay
            if (!requireTurnRightActive)
            {
                Debug.Log("[LRM] RequireRightTurn no longer active -> stop timer (early).");
                // clear coroutine reference nếu là coroutine hiện tại
                if (noTurnRightCoroutine == thisCoroutine) noTurnRightCoroutine = null;
                yield break;
            }

            // tăng time
            elapsed += Time.deltaTime;

            // mỗi khi đạt 1 giây thêm progress (visual)
            while (elapsed >= secondsCounted + 1f && secondsCounted < target)
            {
                secondsCounted++;
                // tăng progress lên 1 (dùng AddProgress để giữ consistency và event firing)
                AddProgress(taskId, 1);
                Debug.Log($"[LRM] RequireRightTurn timer: {secondsCounted}/{target} (taskId={taskId})");
            }

            yield return null;
        }

        // Hết thời gian
        requireTurnRightActive = false;

        // clear coroutine reference nếu là coroutine hiện tại
        if (noTurnRightCoroutine == thisCoroutine) noTurnRightCoroutine = null;

        if (turnRightSeen)
        {
            Debug.Log("[LRM] RequireRightTurn PASSED (saw right turn)");
            if (progress.ContainsKey(taskId) && progress[taskId] < targets[taskId])
                AddProgress(taskId, targets[taskId] - progress[taskId]);
        }
        else
        {
            Debug.Log("[LRM] RequireRightTurn FAILED (no right turn seen)");
            noTurnRightViolated = true;
        }
    }

    // ============================================================
    // CHECK COMPLETE (REPLACED: ignore KillBoss when computing allDone)
    // ============================================================
    void CheckAllTasks()
    {
        if (currentLevelConfig == null)
        {
            Debug.LogWarning("[LRM] CheckAllTasks called but currentLevelConfig == null");
            return;
        }

        // Auto-complete special tasks (NoDamage / NoPick / NoCollision) when not violated
        foreach (var t in currentLevelConfig.tasks)
        {
            if (t.type == TaskType.NoDamageRun && noDamageActive && !noDamageViolated && !done[t.id])
            {
                done[t.id] = true; OnTaskCompleted?.Invoke(t.id); Debug.Log("[LRM] Auto-complete NoDamageRun");
            }
            if (t.type == TaskType.NoPickItems && forbidPickActive && !forbidPickViolated && !done[t.id])
            {
                done[t.id] = true; OnTaskCompleted?.Invoke(t.id); Debug.Log("[LRM] Auto-complete NoPickItems");
            }
            if (t.type == TaskType.NoCollisionRun && noCollisionActive && !noCollisionViolated && !done[t.id])
            {
                done[t.id] = true; OnTaskCompleted?.Invoke(t.id); Debug.Log("[LRM] Auto-complete NoCollision");
            }
        }

        // Compute whether all *non-boss* tasks are done.
        bool allNonBossDone = true;

        foreach (var t in currentLevelConfig.tasks)
        {
            // Skip KillBoss when deciding whether to start the boss fight.
            if (t.type == TaskType.KillBoss)
                continue;

            // If progress/done dictionary doesn't have entry, treat as not done
            if (!done.ContainsKey(t.id) || !done[t.id])
            {
                allNonBossDone = false;
                break;
            }
        }

        // Debug snapshot to help troubleshooting
        Debug.Log($"[LRM] CheckAllTasks: allNonBossDone={allNonBossDone} (levelCompleted={levelCompleted})");

        if (!allNonBossDone) return;

        // Check violations for special tasks — if any violated, cannot complete (and cannot proceed to boss)
        if ((forbidPickActive && forbidPickViolated) ||
            (noCollisionActive && noCollisionViolated) ||
            (noDamageActive && noDamageViolated))
        {
            Debug.Log("[LRM] Không thể hoàn thành LEVEL do vi phạm điều kiện đặc biệt!");
            return;
        }

        // If this level requires a boss fight, start it now (we have completed all non-boss tasks)
        if (currentLevelConfig.requiresBoss)
        {
            waitingForBoss = true;
            Debug.Log("[LRM] Đã hoàn thành tất cả task non-boss. Bắt đầu Boss Fight...");
            StartBossFight();
            return;
        }

        // No boss required: finalize level immediately
        CompleteLevel();
    }


    // ============================================================
    // BOSS
    // ============================================================
    public void OnBossDefeated()
    {
        Debug.Log("[LRM] OnBossDefeated called.");

        // Nếu không phải chờ boss (edge-case), vẫn cho complete
        if (!waitingForBoss && !isInBossFight)
        {
            Debug.Log("[LRM] Boss bị giết nhưng level không yêu cầu boss — vẫn cho hoàn thành.");
            CompleteLevel();
            return;
        }

        // Đã ở trong boss fight -> xử lý hoàn tất
        waitingForBoss = false;
        isInBossFight = false;

        // Hoàn tất level (ghi điểm, mark level complete)
        CompleteLevel(); // sẽ gọi EndCurrentRun(true) bên trong

        // Thay vì quay lại gameplay, ta load MapSelectScene
        string mapSelectScene = "MapSelectScene"; // <- đổi tên nếu scene của bạn khác
        Debug.Log($"[LRM] Boss defeated -> returning to {mapSelectScene} (level already completed).");

        StartCoroutine(ReturnToMapSelectCoroutine(mapSelectScene));
    }


    // ============================================================
    // COMPLETE LEVEL
    // ============================================================
    void CompleteLevel()
    { 
    //    if (levelCompleted) return;
    //    levelCompleted = true;

    //    int runScore = ScoreManager.I != null ? ScoreManager.I.CurrentRunScore : 0;

    //    // 1) mark level complete (best-per-level + cache)
    //    GameSave.MarkLevelComplete(mapConfig.mapIndex, currentLevelIndex, runScore);

    //    // 2) ADD runScore vào cumulative run totals (luôn add mỗi lượt chơi khi complete)
    //    GameSave.AddRunScore(mapConfig.mapIndex, runScore);

    //    Debug.Log($"[LRM] LEVEL COMPLETED! Map:{mapConfig.mapIndex} Level:{currentLevelIndex} Score+={runScore}");

    //    OnLevelCompletedEvent?.Invoke(mapConfig.mapIndex, currentLevelIndex);
    if (levelCompleted) return;
    levelCompleted = true;

    EndCurrentRun(true); // sẽ tự động gọi AddRunScore + MarkLevelComplete
    }



    // ============================================================
    // NEW: POLL SCORE AT LEAST TASKS (AUTO UPDATE)
    // ============================================================
    void Update()
    {
        if (currentLevelConfig == null || Nhi_ScoreManager.I == null || levelCompleted) return;

        // xử lý ScoreAtLeast
        foreach (var t in currentLevelConfig.tasks)
        {
            if (t.type == TaskType.ScoreAtLeast)
            {
                if (!progress.ContainsKey(t.id) || !targets.ContainsKey(t.id)) continue;
                int currentScore = Nhi_ScoreManager.I.CurrentRunScore;
                int target = targets[t.id];
                int newProgress = Mathf.Min(currentScore, target);
                int curProgress = progress[t.id];
                if (newProgress > curProgress)
                {
                    AddProgress(t.id, newProgress - curProgress);
                }
            }
            else if (t.type == TaskType.CollectMoneyTotal)
            {
                // xử lý tiền (poll từ MoneyManager)
                if (Nhi_MoneyManager.I == null) continue;
                if (!progress.ContainsKey(t.id) || !targets.ContainsKey(t.id)) continue;

                int currentMoney = Nhi_MoneyManager.I.CurrentMoney;
                int target = targets[t.id];
                int newProgress = Mathf.Min(currentMoney, target);
                int curProgress = progress[t.id];
                if (newProgress > curProgress)
                {
                    AddProgress(t.id, newProgress - curProgress);
                }
            }
        }
    }

    // ================= Boss scene helpers =================
    void StartBossFight()
    {
        if (isInBossFight) return;
        if (currentLevelConfig == null)
        {
            Debug.LogWarning("[LRM] StartBossFight: currentLevelConfig null.");
            return;
        }

        // Lấy tên scene boss từ config nếu có, fallback theo map index
        bossSceneToLoad = !string.IsNullOrEmpty(currentLevelConfig.bossSceneName)
                            ? currentLevelConfig.bossSceneName
                            : $"FightingBoss{mapConfig.mapIndex}";

        // Lưu scene hiện tại để có thể quay lại nếu cần (không dùng trong flow hiện tại)
        bossReturnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        Debug.Log($"[LRM] StartBossFight -> loading {bossSceneToLoad}, return to {bossReturnScene}");

        isInBossFight = true;
        waitingForBoss = true;

        StartCoroutine(LoadBossSceneCoroutine(bossSceneToLoad));
    }

    System.Collections.IEnumerator LoadBossSceneCoroutine(string sceneName)
    {
        // đợi 1 frame để mọi event hoàn tất
        yield return null;

        // nếu bạn muốn tạm dừng hoặc xử lý âm thanh: AudioManager.Instance?.StopBGM(0.25f);

        var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        while (!async.isDone) yield return null;

        Debug.Log("[LRM] Boss scene loaded: " + sceneName);
        // LRM vẫn persistent nên có thể chờ boss gọi OnBossDefeated()
    }

    System.Collections.IEnumerator ReturnToMapSelectCoroutine(string targetScene)
    {
        // đợi 1 frame cho mọi thay đổi state được flush
        yield return null;

        // (tùy) nếu bạn có GameSave.SaveNow() gọi để chắc chắn lưu vào disk, gọi ở đây trước khi load:
        // GameSave.SaveNow();

        var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
        while (!async.isDone) yield return null;

        Debug.Log("[LRM] Loaded " + targetScene + " after boss defeat.");

        // reset tạm
        bossReturnScene = null;
        bossSceneToLoad = null;
    }

    // ============================================================
    // HELPERS
    // ============================================================
    public string[] GetTaskIds() => progress.Keys.ToArray();

    public (int cur, int target) GetTaskProgress(string id)
    {
        if (progress.ContainsKey(id))
            return (progress[id], targets[id]);

        return (0, 0);
    }
    // Thêm vào cuối class LevelRuntimeManager
    public void EndCurrentRun(bool forceComplete = false)
    {
        if (levelCompleted && !forceComplete) return; // đã complete rồi thì không add nữa

        int runScore = Nhi_ScoreManager.I != null ? Nhi_ScoreManager.I.CurrentRunScore : 0;

        
        // Luôn cộng runScore vào tổng tích lũy, dù complete hay không
        GameSave.AddRunScore(mapConfig.mapIndex, runScore);

        

        Debug.Log($"[LRM] EndCurrentRun - Added runScore {runScore} to cumulative totals (forceComplete={forceComplete})");

        // Nếu là complete thật sự thì vẫn mark complete như cũ
        if (forceComplete)
        {
            GameSave.MarkLevelComplete(mapConfig.mapIndex, currentLevelIndex, runScore);
            OnLevelCompletedEvent?.Invoke(mapConfig.mapIndex, currentLevelIndex);
        }
    }
    public LevelConfig_SO GetCurrentLevelConfig() => currentLevelConfig;

    public int GetCurrentLevelIndex() => currentLevelIndex;

    public int GetMapIndex() => mapConfig ? mapConfig.mapIndex : 0;
}