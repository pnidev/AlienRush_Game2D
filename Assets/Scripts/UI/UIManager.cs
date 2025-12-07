using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public PlayerMovement2D player;
    public GameObject heartContainer;
    public GameObject deathWindow;
    public Button buyHealth;

    public Image[] hearts;

    private bool deathShown = false; // tránh gọi nhiều lần
    public TextMeshProUGUI effectStatusText;
    private Coroutine effectRoutine;
    public GameObject effectStatusPanel;

    [Header("HUD UI")]
    public TextMeshProUGUI coinText;     // <-- HIỂN THỊ TIỀN (coin)
    public TextMeshProUGUI scoreText;    // <-- HIỂN THỊ SCORE (nếu bạn muốn dùng)





    [Header("Effect UI")]
    public GameObject blindPanel;

    void Start()
    {
        // Đảm bảo panel bị tắt lúc bắt đầu
        if (effectStatusPanel != null)
            effectStatusPanel.SetActive(false);

        if (effectStatusText != null)
            effectStatusText.gameObject.SetActive(false);

        // Lấy danh sách các Image trong heartContainer
        if (heartContainer != null)
        {
            hearts = heartContainer.GetComponentsInChildren<Image>();
        }
        else
        {
            Debug.LogError("heartContainer is not assigned.");
        }

        if (deathWindow != null)
            deathWindow.SetActive(false);

        if (blindPanel != null)
            blindPanel.SetActive(false);

        if (buyHealth != null)
            buyHealth.onClick.AddListener(printCayMessage);
        // Init coin UI và subscribe vào CoinManager
        // ============================
        // COIN UI — SUBSCRIBE
        // ============================
        if (CoinManager.I != null)
        {
            // hiển thị ngay giá trị hiện tại
            if (coinText != null)
                coinText.text = CoinManager.I.CurrentCoins.ToString();

            // đăng ký event để tự update coin
            CoinManager.I.OnCoinsChanged += OnCoinsChanged;
        }

        if (scoreText != null)
            scoreText.text = "0";
    }

    void Update()
    {
        //if (player == null) return;

        // Cập nhật tim (an toàn)
        UpdateHearts(player.health);

        //// Khi chết chỉ show một lần
        if (!deathShown && player.health <= 0)
        {
            ShowDeathWindow();
            deathShown = true;
        }
        //if (coinText != null && CoinManager.Instance != null)
        //{
        //    coinText.text = CoinManager.Instance.CurrentCoins.ToString();
        //}
        //if (scoreText != null && player != null)
        //{
        //    int displayedScore = Mathf.FloorToInt(player.score);
        //    scoreText.text = displayedScore.ToString();
        //}
    }

    // HÀM CHÍNH CẬP NHẬT TIM (SỬA LỖI)
    public void UpdateHearts(int currentHealth)
    {
        // Nếu mảng hearts không tồn tại hoặc rỗng thì thoát
        if (hearts == null || hearts.Length == 0)
            return;

        // Tắt toàn bộ tim
        foreach (var h in hearts)
            h.gameObject.SetActive(false);

        // Bật đúng số tim = currentHealth (giữ nguyên off-by-one như bạn yêu cầu)
        for (int i = 0; i <= currentHealth && i <= hearts.Length; i++)
            hearts[i].gameObject.SetActive(true);


        //// BẢO VỆ 100% KHỎI CRASH
        //if (hearts == null || hearts.Length == 0)
        //{
        //    Debug.LogWarning("[UIManager] heartImages chưa sẵn sàng, bỏ qua UpdateHearts");
        //    return;
        //}

        //for (int i = 0; i < hearts.Length; i++)
        //{
        //    if (hearts[i] != null)
        //        hearts[i].enabled = (i < currentHealth);
        //}
    }






    // Khi chết
    public void ShowDeathWindow()
    {
        if (deathWindow != null)
            deathWindow.SetActive(true);

        if (heartContainer != null)
            heartContainer.SetActive(false);

        // Nếu deathWindow có component DeathUI, gọi ShowDeath() để cập nhật state của nút Continue
        var deathUIComp = deathWindow != null ? deathWindow.GetComponent<DeathUI>() : null;
        if (deathUIComp != null)
        {
            // đảm bảo DeathUI biết player hiện tại
            deathUIComp.player = this.player;
            deathUIComp.uiManager = this; // nếu DeathUI dùng uiManager, gán luôn để nhất quán
            deathUIComp.ShowDeath();
        }
    }



    void printCayMessage()
    {
        Debug.Log("Má cay vãi l");
    }

    // HIỆU ỨNG MÙ
    public void TriggerBlindEffect(float duration)
    {
        if (blindPanel != null)
        {
            StartCoroutine(BlindRoutine(duration));
        }
        else
        {
            Debug.LogError("blindPanel is not assigned.");
        }
    }

    private IEnumerator BlindRoutine(float duration)
    {
        blindPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        blindPanel.SetActive(false);
    }


    public void ShowEffectStatus(string text, float duration)
    {
        if (effectStatusText == null)
        {
            Debug.LogError("EffectStatusText chưa được gán trong UIManager.");
            return;
        }

        // Bật panel cha nếu có
        if (effectStatusPanel != null)
            effectStatusPanel.SetActive(true);

        // Dừng coroutine cũ (nếu có) để tránh 2 coroutine nhấp nháy chồng nhau
        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        effectRoutine = StartCoroutine(EffectStatusRoutine(text, duration));
    }

    private IEnumerator EffectStatusRoutine(string text, float duration)
    {
        // Set text và bật object text (panel đã bật ở caller)
        effectStatusText.text = text;
        effectStatusText.gameObject.SetActive(true);

        // Lưu màu gốc để reset sau khi xong
        Color baseColor = effectStatusText.color;

        float timer = 0f;
        bool visible = true;
        float blinkInterval = 0.2f; // có thể thay đổi

        while (timer < duration)
        {
            float alpha = visible ? 1f : 0.25f;
            effectStatusText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            visible = !visible;

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Reset màu gốc trước khi tắt
        effectStatusText.color = baseColor;

        ClearEffectStatus();
        effectRoutine = null;
    }

    /// <summary>
    /// Clear ngay effect (dừng nhấp nháy, ẩn text + panel).
    /// </summary>
    public void ClearEffectStatus()
    {
        // Dừng coroutine nếu đang chạy
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);
            effectRoutine = null;
        }

        // Ẩn text
        if (effectStatusText != null)
        {
            effectStatusText.text = "";
            effectStatusText.gameObject.SetActive(false);
        }

        // Ẩn panel cha
        if (effectStatusPanel != null)
            effectStatusPanel.SetActive(false);
    }
    void OnCoinsChanged(int newCoins)
    {
        if (coinText != null)
            coinText.text = newCoins.ToString();
    }

    void OnDestroy()
    {
        if (CoinManager.I != null)
            CoinManager.I.OnCoinsChanged -= OnCoinsChanged;
    }

}