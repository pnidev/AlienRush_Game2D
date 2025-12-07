using UnityEngine;

public class Nhi_ScoreManager : MonoBehaviour
{
    public static Nhi_ScoreManager I { get; private set; }
    // FLOAT score nội bộ
    private float runScoreFloat = 0f;

    // int cho UI + LRM
    public int CurrentRunScore => Mathf.FloorToInt(runScoreFloat);

    public int currentMap = 1;

    // nếu bạn muốn score tự tăng chậm theo thời gian
    public float scorePerSecond = 0f; // để 0 nếu score do playerMovement tính

    void Awake()
    {
        if (I == null)
        {
            I = this;
            
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        if (scorePerSecond > 0f)
            AddScore(scorePerSecond * Time.deltaTime);
    }

    public void ResetRunScoreForMap(int mapIndex)
    {
        currentMap = mapIndex;
        runScoreFloat = 0f;
    }

    public void AddScore(float amount)
    {
        if (amount > 0f)
            runScoreFloat += amount;
    }

    public void AddScore(int amount)
    {
        if (amount > 0)
            runScoreFloat += amount;
    }

    /// <summary>
    /// Gọi khi kết thúc 1 lần chơi (die hoặc qua level)
    /// để cập nhật Highest Score của từng map.
    /// </summary>
    public void CommitRunScore()
    {
        int intScore = CurrentRunScore;

        int oldBest = GameSave.GetBestScoreForMap(currentMap);

        if (intScore > oldBest)
        {
            Debug.Log($"[ScoreManager] NEW HIGH SCORE Map {currentMap}: {intScore}");
            GameSave.SetBestScoreForMap(currentMap, intScore);
        }
        else
        {
            Debug.Log($"[ScoreManager] Run score = {intScore}, best = {oldBest} (not updated)");
        }
    }
}
