
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MapSelectManager - quản lý màn chọn map
/// - Hiển thị unlock, tổng điểm
/// - TỰ ĐỘNG NHẢY THẲNG VÀO BOSS nếu đã hoàn thành hết level thường của map đó
/// </summary>
public class MapSelectManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button map1Btn;
    public Button map2Btn;
    public Button map3Btn;

    [Header("Totals UI (optional)")]
    public TextMeshProUGUI globalTotalText;
    public TextMeshProUGUI map1TotalText;
    public TextMeshProUGUI map2TotalText;
    public TextMeshProUGUI map3TotalText;

    [Header("Config")]
    public int maxMapCount = 3;
    // Số level mỗi map: index 0 = Map1, index 1 = Map2, ...
    // Ví dụ: Map1 có 6 level (0-4 normal + level 5 là boss)
    public int[] levelsPerMap = new int[] { 6, 5, 4 };

    private void Reset()
    {
        maxMapCount = 3;
        levelsPerMap = new int[] { 6, 5, 4 };
    }

    private void OnEnable()
    {
        SetupButtonListeners();
        Refresh();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void Start()
    {
        SetupButtonListeners();
        Refresh();
    }

    private void SetupButtonListeners()
    {
        if (map1Btn != null)
        {
            map1Btn.onClick.RemoveAllListeners();
            map1Btn.onClick.AddListener(() => OnMapClicked(1));
        }
        if (map2Btn != null)
        {
            map2Btn.onClick.RemoveAllListeners();
            map2Btn.onClick.AddListener(() => OnMapClicked(2));
        }
        if (map3Btn != null)
        {
            map3Btn.onClick.RemoveAllListeners();
            map3Btn.onClick.AddListener(() => OnMapClicked(3));
        }
    }

    private void RemoveButtonListeners()
    {
        if (map1Btn != null) map1Btn.onClick.RemoveAllListeners();
        if (map2Btn != null) map2Btn.onClick.RemoveAllListeners();
        if (map3Btn != null) map3Btn.onClick.RemoveAllListeners();
    }

    public void Refresh()
    {
        if (map1Btn != null) map1Btn.interactable = GameSave.IsMapUnlocked(1);
        if (map2Btn != null) map2Btn.interactable = GameSave.IsMapUnlocked(2);
        if (map3Btn != null) map3Btn.interactable = GameSave.IsMapUnlocked(3);

        if (globalTotalText != null)
            globalTotalText.text = $"All Maps BestTotal: {GameSave.GetGlobalScore(maxMapCount)}\nAll Maps RunTotal: {GameSave.GetGlobalRunTotal()}";

        if (map1TotalText != null) map1TotalText.text = $"Map 1 - BestTotal: {GameSave.GetMapScore(1)}\nRunTotal: {GameSave.GetMapRunTotal(1)}";
        if (map2TotalText != null) map2TotalText.text = $"Map 2 - BestTotal: {GameSave.GetMapScore(2)}\nRunTotal: {GameSave.GetMapRunTotal(2)}";
        if (map3TotalText != null) map3TotalText.text = $"Map 3 - BestTotal: {GameSave.GetMapScore(3)}\nRunTotal: {GameSave.GetMapRunTotal(3)}";
    }

    /// <summary>
    /// Xử lý khi bấm chọn map
    /// → Nếu đã hoàn thành hết level thường → load thẳng boss scene
    /// → Nếu chưa → vào MapXScene bình thường
    /// </summary>
    private void OnMapClicked(int mapIndex)
    {
        if (!GameSave.IsMapUnlocked(mapIndex))
        {
            Debug.Log($"[MapSelect] Map {mapIndex} is locked.");
            return;
        }

        int levelCount = levelsPerMap[mapIndex - 1]; // Map1 → index 0
        int normalLevelCount = levelCount - 1; // level cuối là boss

        // Kiểm tra xem đã hoàn thành hết level thường chưa
        bool allNormalLevelsCompleted = true;
        for (int i = 0; i < normalLevelCount; i++)
        {
            if (!GameSave.IsLevelCompleted(mapIndex, i))
            {
                allNormalLevelsCompleted = false;
                break;
            }
        }

        // ĐÃ HOÀN THÀNH HẾT LEVEL THƯỜNG → NHẢY THẲNG VÀO BOSS
        if (allNormalLevelsCompleted)
        {
            string bossSceneName = $"FightingBoss{mapIndex}"; // tên scene boss theo chuẩn của bạn
            Debug.Log($"[MapSelect] Map {mapIndex} đã hoàn thành hết level thường → NHẢY THẲNG VÀO BOSS: {bossSceneName}");
            SceneManager.LoadScene(bossSceneName);
            return; // quan trọng: thoát luôn, không chạy xuống dưới
        }

        // CHƯA HOÀN THÀNH → vào map bình thường như cũ
        GameSave.SetLastPlayed(mapIndex, 0);
        Debug.Log($"[MapSelect] Chọn Map {mapIndex} → vào Map{mapIndex}Scene (còn level chưa làm)");
        SceneManager.LoadScene($"Map{mapIndex}Scene");
    }

    // Optional: nếu bạn muốn sau khi đánh boss xong thì không cho vào boss nữa (tùy chọn)
    // Thêm vào OnMapClicked trước khi load boss:
    // if (GameSave.IsBossDefeated(mapIndex)) { ... show "Completed" ... return; }

    public int RecomputeMapTotalFromBests(int mapIndex, int levelsCount)
    {
        int sum = 0;
        for (int i = 0; i < levelsCount; i++)
            sum += GameSave.GetLevelBest(mapIndex, i);
        return sum;
    }

    public void RefreshFromExternal() => Refresh();

    // Nút Back về MapSelect từ trong game
    public void OnBackToMapSelectClicked()
    {
        Nhi_ScoreManager.I?.CommitRunScore();

        if (LevelRuntimeManager.I != null)
            LevelRuntimeManager.I.EndCurrentRun(false); // không force complete

        SceneManager.LoadScene("MapSelectScene");
    }
}