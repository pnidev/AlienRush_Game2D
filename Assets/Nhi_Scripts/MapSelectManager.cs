using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelectManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button map1Btn;
    public Button map2Btn;
    public Button map3Btn;

    [Header("Lock PNG Images (map 2 & 3 only)")]
    public GameObject map2InfoImage;
    public GameObject map3InfoImage;

    [Header("Config")]
    public int[] levelsPerMap = new int[] { 6, 5, 4 };

    private void Start()
    {
        SetupButtonListeners();
        Refresh();
    }

    void SetupButtonListeners()
    {
        map1Btn.onClick.RemoveAllListeners();
        map1Btn.onClick.AddListener(() => OnMapClicked(1));

        map2Btn.onClick.RemoveAllListeners();
        map2Btn.onClick.AddListener(() => OnMapClicked(2));

        map3Btn.onClick.RemoveAllListeners();
        map3Btn.onClick.AddListener(() => OnMapClicked(3));
    }

    // =========================================
    // REFRESH UI (Quan trọng)
    // =========================================
    public void Refresh()
    {
        // MAP 1: luôn mở → chỉ hiện nút
        map1Btn.gameObject.SetActive(true);

        // MAP 2
        bool unlocked2 = GameSave.IsMapUnlocked(2);
        map2Btn.gameObject.SetActive(unlocked2);
        if (map2InfoImage != null)
            map2InfoImage.SetActive(!unlocked2);

        // MAP 3
        bool unlocked3 = GameSave.IsMapUnlocked(3);
        map3Btn.gameObject.SetActive(unlocked3);
        if (map3InfoImage != null)
            map3InfoImage.SetActive(!unlocked3);
    }

    // =========================================
    // CLICK MAP
    // =========================================
    private void OnMapClicked(int mapIndex)
    {
        if (!GameSave.IsMapUnlocked(mapIndex))
        {
            Debug.Log($"Map {mapIndex} đang khóa.");
            return;
        }

        // LẤY INDEX LEVEL CUỐI CÙNG CHUẨN THEO CONFIG
        int finalLevelIndex = LevelConstants.GetFinalLevelIndexForMap(mapIndex);
        if (finalLevelIndex < 0)
        {
            Debug.LogError($"[MapSelect] finalLevelIndex < 0 cho map {mapIndex}. Kiểm tra LevelConstants.");
            return;
        }

        // CÁC LEVEL TRƯỚC BOSS: 0 .. finalLevelIndex - 1
        bool allPreBossCompleted = true;
        for (int i = 0; i < finalLevelIndex; i++)
        {
            if (!GameSave.IsLevelCompleted(mapIndex, i))
            {
                allPreBossCompleted = false;
                break;
            }
        }

        // LEVEL CHỨA BOSS: finalLevelIndex
        bool bossBeaten = GameSave.IsLevelCompleted(mapIndex, finalLevelIndex);

        // === ONLY ONCE: nếu đã qua hết level trước boss, NHƯNG level cuối (có boss) CHƯA DONE → cho vào boss
        if (allPreBossCompleted && !bossBeaten)
        {
            string bossScene = "";
            switch (mapIndex)
            {
                case 1: bossScene = "boss_1_officeq"; break;
                case 2: bossScene = "FightingBoss2"; break;
                case 3: bossScene = "FightingBoss3"; break;
            }

            if (!string.IsNullOrEmpty(bossScene))
            {
                Debug.Log($"[MapSelect] Load boss scene cho map {mapIndex}: {bossScene}");
                SceneManager.LoadScene(bossScene);
            }
            else
            {
                Debug.LogError($"[MapSelect] Chưa cấu hình bossScene cho mapIndex = {mapIndex}");
            }
            return;
        }

        // Còn lại: vào gameplay map (run bình thường)
        GameSave.SetLastPlayed(mapIndex, 0);
        string mapSceneName = $"Map{mapIndex}Scene";
        Debug.Log($"[MapSelect] Load map scene cho map {mapIndex}: {mapSceneName}");
        SceneManager.LoadScene(mapSceneName);
    }
    // =========================================
    // NÚT RESET PROGRESS
    // =========================================
    public void OnResetAllLevelsButtonClicked()
    {
        // 1) Reset tiến trình tất cả level/task
        GameSave.ResetAllLevelProgress(3); // nếu bạn có hơn 3 map thì tăng số này

        // 2) Refresh lại UI khóa/mở map
        Refresh();

        // 3) (Tùy chọn) Load lại chính MapSelectScene cho sạch state
        // Nếu scene của bạn tên khác thì sửa lại cho đúng
        SceneManager.LoadScene("MapSelectScene");

        Debug.Log("[MapSelect] Đã reset toàn bộ tiến trình level và reload MapSelectScene.");
    }


}
