using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class Leaderboard : MonoBehaviour
{
    [Header("Navigation")]
    public Button backButton;

    [Header("Map Selection Buttons")]
    public Button hcmButton;
    public Button hnButton;
    public Button dnButton;

    [Header("Leaderboard Panel")]
    public GameObject leaderboardPanel;

    [Header("Player Info Display - Rank 1")]
    public GameObject rank1Panel;
    public Image rank1Image;
    public TextMeshProUGUI rank1NameText;
    public TextMeshProUGUI rank1ScoreText;

    [Header("Player Info Display - Rank 2")]
    public GameObject rank2Panel;
    public Image rank2Image;
    public TextMeshProUGUI rank2NameText;
    public TextMeshProUGUI rank2ScoreText;

    [Header("Player Info Display - Rank 3")]
    public GameObject rank3Panel;
    public Image rank3Image;
    public TextMeshProUGUI rank3NameText;
    public TextMeshProUGUI rank3ScoreText;

    [Header("Rank Sprites")]
    public Sprite rank1Sprite;
    public Sprite rank2Sprite;
    public Sprite rank3Sprite;

    [System.Serializable]
    public class PlayerScore
    {
        public string playerName;
        public int score;

        public PlayerScore(string name, int score)
        {
            this.playerName = name;
            this.score = score;
        }
    }

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);

        if (hcmButton != null)
            hcmButton.onClick.AddListener(() => ShowLeaderboard("HCM"));

        if (hnButton != null)
            hnButton.onClick.AddListener(() => ShowLeaderboard("HN"));

        if (dnButton != null)
            dnButton.onClick.AddListener(() => ShowLeaderboard("DN"));

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);

        // Show default ranking (HCM or Total) on start
        ShowDefaultRank1();
    }

    void ShowDefaultRank1()
    {
        // Default to HCM map or show currently logged in user
        ShowLeaderboard("HCM");
    }

    /// <summary>
    /// Returns a list of all known usernames (Current player + scanned users)
    /// </summary>
    List<string> GetAllUsernames()
    {
        // Lấy danh sách username từ LoginManager
        return LoginManager.GetUserLoginList();
    }

    /// <summary>
    /// Gets players and their specific scores for a chosen map
    /// </summary>
    List<PlayerScore> GetAllPlayersForMap(string mapName)
    {
        List<PlayerScore> players = new List<PlayerScore>();
        List<string> allUsernames = GetAllUsernames() ?? new List<string>();

        // mapName -> mapIndex mapping (nếu cần)
        System.Collections.Generic.Dictionary<string, int> nameToIndex = new System.Collections.Generic.Dictionary<string, int>()
    {
        {"HCM", 1},
        {"HN",  2},
        {"DN",  3}
    };

        foreach (string user in allUsernames)
        {
            int score = 0;

            // Try new key if we can infer mapIndex
            if (nameToIndex.ContainsKey(mapName))
            {
                string newKey = $"Score_Map{nameToIndex[mapName]}_{user}";
                score = PlayerPrefs.GetInt(newKey, 0);
            }

            // Fallback legacy key (Score_HCM_user, Score_HN_user ...)
            if (score == 0)
            {
                string legacyKey = $"Score_{mapName}_{user}";
                score = PlayerPrefs.GetInt(legacyKey, 0);
            }

            players.Add(new PlayerScore(user, score));
        }

        // debug logs (giữ như cũ)
        Debug.Log("========================================");
        Debug.Log($"Map {mapName}: Found {players.Count} players");
        foreach (var p in players)
        {
            Debug.Log($" - {p.playerName}: {p.score}");
        }
        Debug.Log("========================================");

        return players;
    }

    public void ShowLeaderboard(string mapName)
    {
        Debug.Log($"Displaying leaderboard for: {mapName}");

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);

        List<PlayerScore> players = GetAllPlayersForMap(mapName);

        // Sort descending by score
        players = players.OrderByDescending(p => p.score).ToList();

        DisplayLeaderboard(players);
    }

    void DisplayLeaderboard(List<PlayerScore> players)
    {
        HideAllRanks();

        string currentUser = PlayerPrefs.GetString("CurrentUsername", "");

        // Nếu không có ai có score (tất cả đều 0), đặt user hiện tại lên đầu
        bool hasAnyScore = players.Any(p => p.score > 0);

        if (!hasAnyScore && !string.IsNullOrEmpty(currentUser))
        {
            // Tìm và di chuyển user hiện tại lên đầu
            PlayerScore currentPlayer = players.FirstOrDefault(p => p.playerName == currentUser);
            if (currentPlayer != null)
            {
                players.Remove(currentPlayer);
                players.Insert(0, currentPlayer);
                Debug.Log($"Không có score, đặt '{currentUser}' ở rank 1 mặc định");
            }
        }

        // Hiển thị tất cả các rank có trong danh sách (tối đa 3)
        for (int i = 0; i < players.Count && i < 3; i++)
        {
            PlayerScore player = players[i];
            int rank = i + 1;

            switch (rank)
            {
                case 1:
                    ShowRank1(player);
                    break;
                case 2:
                    ShowRank2(player);
                    break;
                case 3:
                    ShowRank3(player);
                    break;
            }
        }

        Debug.Log($"Hiển thị {Mathf.Min(players.Count, 3)} rank trên leaderboard");
    }

    void ShowRank1(PlayerScore player)
    {
        if (rank1Panel != null) rank1Panel.SetActive(true);
        if (rank1Image != null && rank1Sprite != null) rank1Image.sprite = rank1Sprite;
        if (rank1NameText != null) rank1NameText.text = player.playerName;
        if (rank1ScoreText != null) rank1ScoreText.text = player.score.ToString();
    }

    void ShowRank2(PlayerScore player)
    {
        if (rank2Panel != null) rank2Panel.SetActive(true);
        if (rank2Image != null && rank2Sprite != null) rank2Image.sprite = rank2Sprite;
        if (rank2NameText != null) rank2NameText.text = player.playerName;
        if (rank2ScoreText != null) rank2ScoreText.text = player.score.ToString();
    }

    void ShowRank3(PlayerScore player)
    {
        if (rank3Panel != null) rank3Panel.SetActive(true);
        if (rank3Image != null && rank3Sprite != null) rank3Image.sprite = rank3Sprite;
        if (rank3NameText != null) rank3NameText.text = player.playerName;
        if (rank3ScoreText != null) rank3ScoreText.text = player.score.ToString();
    }

    void HideAllRanks()
    {
        if (rank1Panel != null) rank1Panel.SetActive(false);
        if (rank2Panel != null) rank2Panel.SetActive(false);
        if (rank3Panel != null) rank3Panel.SetActive(false);
    }

    public void OnBackButtonClick()
    {
        Debug.Log("Back button clicked");
        SceneManager.LoadScene("MainHome");
    }
}