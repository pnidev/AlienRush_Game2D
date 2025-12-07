using System;
using UnityEngine;

public enum TaskType
{
    EatAndUseBread,
    EatAndUseCoffee,
    ScoreAtLeast,
    DashThroughCount,
    OpenMysteryBoxCount,
    KillBoss,

    BuyAfterDieCount,
    CollectMoneyTotal,

    NoDamageRun,
    NoPickItems,
    NoCollisionRun,
    NoTurnRightForSeconds,
}

[CreateAssetMenu(menuName = "Game/LevelConfig")]
public class LevelConfig_SO : ScriptableObject
{
    [Header("Level Info")]
    public int mapIndex;         // 1..3
    public int levelIndex;       // 0..4 trong 1 map
    public string displayName;

    [Header("Boss Info")]
    public bool requiresBoss = false;

    [Tooltip("Tên scene boss. VD: FightingBoss1. Nếu để trống sẽ dùng fallback FightingBoss{mapIndex}.")]
    public string bossSceneName;

    [Serializable]
    public struct Task
    {
        public string id;        // unique string (nhiệm vụ)
        public TaskType type;    // loại nhiệm vụ
        public int target;       // số lượng / giây / điểm
        public string note;      // mô tả hiển thị UI
    }

    public Task[] tasks;
}
