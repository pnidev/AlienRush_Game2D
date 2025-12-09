using UnityEngine;

public class TaskUIAutoHideOnMapComplete : MonoBehaviour
{
    [Tooltip("Root của UI nhiệm vụ. Nếu để trống sẽ dùng chính GameObject này.")]
    public GameObject tasksRoot;

    void Start()
    {
        if (tasksRoot == null)
            tasksRoot = gameObject;

        if (LevelRuntimeManager.I != null)
        {
            LevelRuntimeManager.I.OnLevelStarted += HandleLevelStarted;
            LevelRuntimeManager.I.OnNoActiveLevel += HandleNoActiveLevel;
        }

        RefreshNow();
    }

    // =========================
    // HÀM CHECK: MAP ĐÃ CLEAR HẾT CHƯA?
    // =========================
    bool IsMapFullyCleared()
    {
        if (LevelRuntimeManager.I == null)
            return false;

        int mapIndex = LevelRuntimeManager.I.GetMapIndex();
        if (mapIndex <= 0)
            return false;

        // dùng cùng logic unlock map: level cuối của map
        int finalLevelIndex = LevelConstants.GetFinalLevelIndexForMap(mapIndex);
        if (finalLevelIndex < 0)
            return false;

        // nếu TẤT CẢ level 0..finalLevelIndex đều done => map clear
        for (int i = 0; i <= finalLevelIndex; i++)
        {
            if (!GameSave.IsLevelCompleted(mapIndex, i))
                return false;
        }

        return true;
    }

    void RefreshNow()
    {
        // Nếu map đã clear hết => ẨN luôn task
        if (IsMapFullyCleared())
        {
            tasksRoot.SetActive(false);
            return;
        }

        // Ngược lại, map chưa clear hết => cho hiện task
        // (còn việc level nào đang active thì LRM xử lý)
        tasksRoot.SetActive(true);
    }

    void HandleLevelStarted(int map, int level)
    {
        // Nếu map đã clear hết mà vẫn gọi OnLevelStarted (edge-case) thì vẫn tắt
        if (IsMapFullyCleared())
        {
            tasksRoot.SetActive(false);
        }
        else
        {
            tasksRoot.SetActive(true);
        }
    }

    void HandleNoActiveLevel()
    {
        // LRM báo rõ là "No active level" => map đã clear
        tasksRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (LevelRuntimeManager.I != null)
        {
            LevelRuntimeManager.I.OnLevelStarted -= HandleLevelStarted;
            LevelRuntimeManager.I.OnNoActiveLevel -= HandleNoActiveLevel;
        }
    }
}
