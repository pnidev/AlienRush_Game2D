using UnityEngine;
using System;

/// <summary>
/// GameSave: local save helpers
/// - Lưu best-per-level keys: "m{map}_l{level}_best"
/// - Cache tổng best-per-level tại key "m{map}_score_total" (unchanged)
/// - Lưu tổng tích lũy các run (per-map) tại key "m{map}_run_total"
/// - Lưu tổng tích lũy global tại key "global_run_total" (hoặc sum of maps)
/// - Giữ IsLevelCompleted keys: "m{map}_l{level}_done"
/// </summary>
public static class GameSave
{
    // ----- Key helpers -----
    static string LevelDoneKey(int map, int level) => $"m{map}_l{level}_done";
    static string LevelBestKey(int map, int level) => $"m{map}_l{level}_best";
    static string MapScoreKey(int map) => $"m{map}_score_total";       // cache of sum best-per-level
    static string MapRunTotalKey(int map) => $"m{map}_run_total";     // cumulative runs per map
    static string GlobalRunTotalKey => "global_run_total";            // cumulative runs across all maps
    static string LastPlayedKey => "last_played_map_level";

    // ----- Public API -----

    /// <summary>
    /// Gọi khi level hoàn thành.
    /// Vẫn giữ hành vi update best-per-level (UpdateLevelBest).
    /// NOTE: MarkLevelComplete chỉ đánh dấu done và update best cache.
    /// Việc cộng runScore vào tổng tích lũy thực hiện bằng gọi AddRunScore (không tự động ở đây),
    /// để bạn có quyền kiểm soát: muốn cộng run dù chưa complete hay khi exit giữa chừng.
    /// </summary>
    public static void MarkLevelComplete(int map, int level, int addRunScore = 0)
    {
        // set done flag
        PlayerPrefs.SetInt(LevelDoneKey(map, level), 1);

        // update best-per-level -> nếu có thay đổi best thì map total cache cũng được cập nhật
        bool bestUpdated = UpdateLevelBest(map, level, addRunScore);

        // lưu last played
        PlayerPrefs.SetString(LastPlayedKey, $"{map}:{level}");
        PlayerPrefs.Save();

        Debug.Log($"[Save] Mark m{map}_l{level} done. runScore={addRunScore}. bestUpdated={bestUpdated}. map{map} total={GetMapScore(map)}");
    }

    /// <summary>
    /// Ghi tổng run của một lượt vào cumulative map-run-totals và global-run-total.
    /// Gọi mỗi khi bạn muốn "tính" lượt chơi vào tổng (ví dụ: khi kết thúc run hoặc khi exit).
    /// This is additive (cumulate).
    /// </summary>
    public static void AddRunScore(int map, int runScore)
    {
        if (runScore == 0) return; // nếu bạn muốn vẫn ghi 0 thì bỏ check này

        // add to per-map run total
        int prev = PlayerPrefs.GetInt(MapRunTotalKey(map), 0);
        int next = prev + runScore;
        PlayerPrefs.SetInt(MapRunTotalKey(map), next);

        // add to global run total
        int gprev = PlayerPrefs.GetInt(GlobalRunTotalKey, 0);
        int gnext = gprev + runScore;
        PlayerPrefs.SetInt(GlobalRunTotalKey, gnext);

        PlayerPrefs.Save();
        Debug.Log($"[Save] AddRunScore map{map} +{runScore} -> mapRunTotal {prev} -> {next}, global {gprev} -> {gnext}");
    }

    /// <summary>
    /// Trả về tổng tích lũy run cho map (MapRunTotal).
    /// </summary>
    public static int GetMapRunTotal(int map)
    {
        return PlayerPrefs.GetInt(MapRunTotalKey(map), 0);
    }

    /// <summary>
    /// Trả về tổng tích lũy run global (All maps).
    /// </summary>
    public static int GetGlobalRunTotal()
    {
        return PlayerPrefs.GetInt(GlobalRunTotalKey, 0);
    }

    /// <summary>
    /// Trả về best score đã lưu cho level (mặc định 0).
    /// </summary>
    public static int GetLevelBest(int map, int level)
    {
        return PlayerPrefs.GetInt(LevelBestKey(map, level), 0);
    }

    /// <summary>
    /// Cập nhật best-per-level nếu newScore > prevBest.
    /// Nếu updated, cập nhật cached MapScoreKey bằng delta (newBest - prevBest).
    /// Trả về true nếu updated (có thay đổi).
    /// </summary>
    public static bool UpdateLevelBest(int map, int level, int newScore)
    {
        if (newScore <= 0) return false; // ignore non-positive scores for best update

        string lvlKey = LevelBestKey(map, level);
        int prevBest = PlayerPrefs.GetInt(lvlKey, 0);

        if (newScore <= prevBest)
        {
            // không thay đổi best
            return false;
        }

        // set new best
        PlayerPrefs.SetInt(lvlKey, newScore);

        // update map total cache by delta
        int prevMapTotal = PlayerPrefs.GetInt(MapScoreKey(map), 0);
        int newMapTotal = prevMapTotal + (newScore - prevBest);
        PlayerPrefs.SetInt(MapScoreKey(map), newMapTotal);

        PlayerPrefs.Save();

        Debug.Log($"[Save] UpdateLevelBest m{map}_l{level}: {prevBest} -> {newScore}. map total {prevMapTotal} -> {newMapTotal}");
        return true;
    }

    /// <summary>
    /// Trả về tổng điểm map (dựa trên cache MapScoreKey).
    /// NOTE: cache được cập nhật mỗi khi UpdateLevelBest được gọi.
    /// </summary>
    public static int GetMapScore(int map)
    {
        return PlayerPrefs.GetInt(MapScoreKey(map), 0);
    }

    /// <summary>
    /// Tổng cho tất cả maps (mặc định maps 1..maxMapInclusive) dựa trên cache MapScoreKey.
    /// </summary>
    public static int GetGlobalScore(int maxMapInclusive = 3)
    {
        int sum = 0;
        for (int m = 1; m <= maxMapInclusive; m++)
            sum += GetMapScore(m);
        return sum;
    }

    /// <summary>
    /// Thiết lập (ghi đè) tổng điểm map (dùng hiếm).
    /// </summary>
    public static void SetMapScore(int map, int total)
    {
        PlayerPrefs.SetInt(MapScoreKey(map), total);
        PlayerPrefs.Save();
        Debug.Log($"[Save] Set map{map} total = {total}");
    }

    /// <summary>
    /// Xóa key tổng cho các map 1..maxMapInclusive
    /// </summary>
    public static void ResetMapScores(int maxMapInclusive = 3)
    {
        for (int m = 1; m <= maxMapInclusive; m++)
            PlayerPrefs.DeleteKey(MapScoreKey(m));
        PlayerPrefs.Save();
        Debug.Log("[Save] ResetMapScores for maps 1.." + maxMapInclusive);
    }

    /// <summary>
    /// Reset toàn bộ PlayerPrefs
    /// </summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[Save] ResetAll");
    }

    /// <summary>
    /// Kiểm tra level đã hoàn thành (flag done)
    /// </summary>
    public static bool IsLevelCompleted(int map, int level)
    {
        return PlayerPrefs.GetInt(LevelDoneKey(map, level), 0) == 1;
    }

    /// <summary>
    /// Kiểm tra map unlocked theo logic cũ: map N unlock nếu final level của map N-1 đã complete.
    /// Yêu cầu LevelConstants.GetFinalLevelIndexForMap tồn tại (giữ compatibility).
    /// </summary>
    public static bool IsMapUnlocked(int mapIndex)
    {
        if (mapIndex <= 1) return true;
        int prevFinal = LevelConstants.GetFinalLevelIndexForMap(mapIndex - 1);
        return IsLevelCompleted(mapIndex - 1, prevFinal);
    }

    /// <summary>
    /// Last played / set last played (giữ như cũ)
    /// </summary>
    public static (int map, int level) GetLastPlayed()
    {
        string s = PlayerPrefs.GetString(LastPlayedKey, "1:0");
        var parts = s.Split(':');
        int m = 1, l = 0;
        if (parts.Length == 2) { int.TryParse(parts[0], out m); int.TryParse(parts[1], out l); }
        return (m, l);
    }

    public static void SetLastPlayed(int map, int level)
    {
        PlayerPrefs.SetString(LastPlayedKey, $"{map}:{level}");
        PlayerPrefs.Save();
        Debug.Log($"[Save] SetLastPlayed -> {map}:{level}");
    }
    // =========================
    // HIGHSCORE THEO MAP
    // =========================
    public static int GetBestScoreForMap(int mapIndex)
    {
        return PlayerPrefs.GetInt($"BestScore_Map{mapIndex}", 0);
    }

    public static void SetBestScoreForMap(int mapIndex, int score)
    {
        PlayerPrefs.SetInt($"BestScore_Map{mapIndex}", score);
        PlayerPrefs.Save();
    }
    /// <summary>
    /// Reset toàn bộ tiến trình level (done flag + best score + tổng map score)
    /// KHÔNG reset tiền, run total, v.v.
    /// </summary>
    public static void ResetAllLevelProgress(int maxMapInclusive = 3)
    {
        for (int map = 1; map <= maxMapInclusive; map++)
        {
            int finalLevelIndex = LevelConstants.GetFinalLevelIndexForMap(map);
            if (finalLevelIndex < 0) continue;

            // Xóa cờ done + best score từng level
            for (int level = 0; level <= finalLevelIndex; level++)
            {
                PlayerPrefs.DeleteKey(LevelDoneKey(map, level));
                PlayerPrefs.DeleteKey(LevelBestKey(map, level));
            }

            // Xóa cache tổng điểm map
            PlayerPrefs.DeleteKey(MapScoreKey(map));
        }

        // Xóa LastPlayed (cho an toàn, để lần sau vào map sẽ từ level 0)
        PlayerPrefs.DeleteKey(LastPlayedKey);

        PlayerPrefs.Save();
        Debug.Log("[Save] ResetAllLevelProgress: xóa done + best + mapScore cho tất cả map.");
    }


}
