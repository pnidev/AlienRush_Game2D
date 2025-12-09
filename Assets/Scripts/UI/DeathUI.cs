using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 

using TMPro;
public class DeathUI : MonoBehaviour
{
    public PlayerMovement2D player;
    public UIManager uiManager;

    [Header("UI")]
    public GameObject deathPanel;
    public Button continueButton;
    public Button menuButton;

    [Header("Cost")]
    public int continueCost = 10;

    public TextMeshProUGUI finalCoinsText; // gán trong prefab deathWindow

    private void Start()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        // Gán sự kiện nút
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenu);
    }

    public void ShowDeath()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        // 1) Clear selection để tránh button bị auto-selected
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // 2) Reset màu visual của button về normal (tránh SelectedColor bị đen)
        if (continueButton != null && continueButton.targetGraphic != null)
            continueButton.targetGraphic.color = continueButton.colors.normalColor;

        // 3) Tắt Navigation để tránh EventSystem auto select lại
        if (continueButton != null)
        {
            var nav = continueButton.navigation;
            nav.mode = Navigation.Mode.None;
            continueButton.navigation = nav;
        }
        // Kiểm tra xem người chơi có đủ coin hay không (dùng CoinManager)
        bool hasEnough = (CoinManager.Instance != null && CoinManager.Instance.CurrentCoins >= continueCost);

        if (continueButton != null)
            continueButton.interactable = hasEnough;

        Debug.Log("Death panel shown (fixed auto-select).");
    }

    private void OnContinue()
    {
        if (player == null) return;

        if (CoinManager.Instance == null)
        {
            Debug.LogError("CoinManager missing!");
            return;
        }

        if (CoinManager.Instance.CurrentCoins < continueCost)
        {
            Debug.Log("Không đủ coin để revive!");
            return;
        }

        bool ok = CoinManager.Instance.SpendCoins(continueCost);
        if (!ok)
        {
            Debug.Log("SpendCoins failed.");
            return;
        }
        // --- Báo cho LRM là đã mua sau khi chết (increment BuyAfterDieCount) ---
        if (LevelRuntimeManager.I != null)
        {
            LevelRuntimeManager.I.AddProgressByType(TaskType.BuyAfterDieCount, 1);
            Debug.Log("[DeathUI] Reported BuyAfterDieCount +1 to LRM.");
        }
        else
        {
            Debug.LogWarning("[DeathUI] LevelRuntimeManager.I is null — cannot report BuyAfterDieCount now.");
        }

        player.ReviveWithOneLife();

        // 1) Tắt death panel
        if (deathPanel != null)
            deathPanel.SetActive(false);

        // 2) Clear selection lần nữa (phòng hờ)
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // 3) Restore Navigation nếu bạn muốn dùng lại navigation sau đó
        if (continueButton != null)
        {
            var nav = continueButton.navigation;
            nav.mode = Navigation.Mode.Automatic;  // hoặc None nếu bạn không muốn navigation
            continueButton.navigation = nav;

            // reset màu về normal (phòng trường hợp màu còn lưu SelectedColor)
            if (continueButton.targetGraphic != null)
                continueButton.targetGraphic.color = continueButton.colors.normalColor;
        }

        if (uiManager != null)
            uiManager.heartContainer.SetActive(true);
    }


    private void OnMenu()
    {
        // Commit best score because player chose to go back to menu
        Nhi_ScoreManager.I?.CommitRunScore();

        // đảm bảo run totals / state được ghi (tuỳ bạn muốn gọi EndCurrentRun)
        if (LevelRuntimeManager.I != null)
        {
            LevelRuntimeManager.I.EndCurrentRun(false);
        }
        // Load scene menu
        SceneManager.LoadScene("MapSelectScene");  // sửa tên scene
    }
}
