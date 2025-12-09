using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Cần thiết cho Button
using UnityEngine.SceneManagement; // Cần thiết cho SceneManager
using TMPro; // Cần thiết cho TextMeshPro

public class Guiidline : MonoBehaviour
{
    [Header("UI References")]
    public GameObject alienObject;      // Alien
    public GameObject background1;      // Background 1
    public GameObject background2;      // Background 2
    public Button luatChoiButton;       // Nút Luật chơi
    public Button vatCanButton;         // Nút Vật cản
    public Button itemButton;           // Nút Item
    public TextMeshProUGUI textNoiDung; // Text nội dung

    [Header("Item Display")]
    public GameObject itemPanel;           // Panel hiển thị item (như trong hình)
    public TextMeshProUGUI itemTitle;      // TEXT 1: Tên item (VD: "BÁNH MÌ VIỆT NAM")
    public Image itemImage;                // IMAGE: Hình ảnh item (ở giữa)
    public TextMeshProUGUI itemDescription; // TEXT 2: Chức năng item (VD: "vượt vật cản 5s")
    public Button continueButton;          // Nút Continue để chuyển item tiếp theo

    [Header("Item Data")]
    public Sprite[] itemSprites;        // Mảng chứa các sprite item
    private int currentItemIndex = 0;   // Index item hiện tại
    
    [Header("Navigation")]
    public Button continueGameButton;   // Nút Continue game (chỉ hiện khi từ Scene2 → tiếp tục game)
    public Button backToMenuButton;     // Nút Back về HomePage (chỉ hiện khi từ HomePage → Guideline)

    // Dữ liệu items (theo thứ tự: Bánh mì -> Cà phê -> Thuốc lào -> Túi mù -> Nitro)
    private ItemData[] items = new ItemData[]
    {
        new ItemData { title = "BÁNH MÌ VIỆT NAM", description = "Vượt vật cản 5s" },
        new ItemData { title = "CAFE VIỆT NAM", description = "Hồi 1 mạng" },
        new ItemData { title = "THUỐC LÀO", description = "Vượt vật cản 5s" },
        new ItemData { title = "TÚI MÙ", description = "Hên thì lĩnh effect ngon / Xui thì bị effect dắm" },
        new ItemData { title = "NITRO", description = "Tăng tốc 5s" }
    };

    [System.Serializable]
    public class ItemData
    {
        public string title;
        public string description;
    }

    [Header("Content")]
    private readonly string luatChoiContent = 
        "-Tránh hoặc Dash xuyên qua vật cản để sống sót càng lâu càng tốt\n\n" +
        "-Ăn item để nhận Buff hoặc tránh Debuff\n\n" +
        "-Có 3 mạng, mất hết là thua\n\n" +
        "-Hoàn thành nhiệm vụ để lên level và gặp Boss ở các mốc 5 / 10 / 15";

    private readonly string vatCanContent = 
        "-Các vật cản có thể xuyên qua: Ổ gà, ổ voi, rào, cổng\n\n" +
        "-Các vật cản không thể xuyên qua: công an, bộ đội";

    void Awake()
    {
        // TẮT TẤT CẢ NGAY LẬP TỨC (chạy trước Start)
        if (alienObject != null) alienObject.SetActive(false);
        if (background1 != null) background1.SetActive(false);
        if (background2 != null) background2.SetActive(false);
        if (luatChoiButton != null) luatChoiButton.gameObject.SetActive(false);
        if (vatCanButton != null) vatCanButton.gameObject.SetActive(false);
        if (itemButton != null) itemButton.gameObject.SetActive(false);
        if (textNoiDung != null) textNoiDung.text = "";
        if (itemPanel != null) itemPanel.SetActive(false);
        if (itemImage != null) itemImage.gameObject.SetActive(false);
    }

    void Start()
    {
        Debug.Log("=== Guiidline Start ===");
        
        // Đếm số scene đã qua để đến Guideline
        int sceneCount = PlayerPrefs.GetInt("SceneCount", 2); // Mặc định = 2 để dễ test
        Debug.Log("Số scene đã qua: " + sceneCount);
        
        // Nút Back: chỉ hiện khi Guideline là scene thứ 2 (HomePage → Guideline)
        bool showBack = (sceneCount == 2);
        
        // Nút Continue: hiện khi đã qua nhiều hơn 2 scene
        bool showContinue = (sceneCount > 2);
        
        Debug.Log("Show Back: " + showBack + ", Show Continue: " + showContinue);
        
        if (continueGameButton != null)
        {
            continueGameButton.gameObject.SetActive(showContinue);
            if (showContinue)
            {
                Debug.Log("Hiển thị nút Continue Game (đã qua " + sceneCount + " scenes)");
                continueGameButton.onClick.RemoveAllListeners();
                continueGameButton.onClick.AddListener(OnContinueGameClick);
            }
        }
        else
        {
            Debug.LogWarning("continueGameButton is NULL! Vui lòng gán trong Inspector.");
        }
        
        if (backToMenuButton != null)
        {
            backToMenuButton.gameObject.SetActive(showBack);
            if (showBack)
            {
                Debug.Log("Hiển thị nút Back (Guideline là scene thứ 2)");
                backToMenuButton.onClick.RemoveAllListeners();
                backToMenuButton.onClick.AddListener(OnBackToMenuClick);
            }
        }
        else
        {
            Debug.LogWarning("backToMenuButton is NULL! Vui lòng gán trong Inspector.");
        }
        
        // Reset counter sau khi check
        PlayerPrefs.SetInt("SceneCount", 1);
        
        // Kiểm tra TextMeshPro
        if (textNoiDung != null)
        {
            Debug.Log("TextMeshPro đã được gán: " + textNoiDung.gameObject.name);
        }
        else
        {
            Debug.LogError("textNoiDung CHƯA được gán trong Inspector!");
        }

        // Gán sự kiện cho các nút
        if (luatChoiButton != null)
        {
            luatChoiButton.onClick.RemoveAllListeners(); // Xóa listener cũ
            luatChoiButton.onClick.AddListener(OnLuatChoiButtonClick);
            Debug.Log("Đã gán sự kiện cho nút Luật Chơi");
        }
        
        if (vatCanButton != null)
        {
            vatCanButton.onClick.RemoveAllListeners(); // Xóa listener cũ
            vatCanButton.onClick.AddListener(OnVatCanButtonClick);
            Debug.Log("Đã gán sự kiện cho nút Vật Cản");
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners(); // Xóa listener cũ
            itemButton.onClick.AddListener(OnItemButtonClick);
            Debug.Log("Đã gán sự kiện cho nút Item");
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ShowNextItem);
            Debug.Log("Đã gán sự kiện cho nút Continue");
        }

        // Ẩn item panel và text content ban đầu
        if (itemPanel != null) itemPanel.SetActive(false);
        if (textNoiDung != null) textNoiDung.gameObject.SetActive(false);

        // Bắt đầu sequence hiển thị
        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        // 1. Hiện Alien trước
        if (alienObject != null) alienObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        // 2. Hiện Background 1 và 2 cùng lúc
        if (background1 != null) background1.SetActive(true);
        if (background2 != null) background2.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        // 3. Hiện tất cả các nút cùng lúc
        if (luatChoiButton != null) luatChoiButton.gameObject.SetActive(true);
        if (vatCanButton != null) vatCanButton.gameObject.SetActive(true);
        if (itemButton != null) itemButton.gameObject.SetActive(true);
        
        // 4. TỰ ĐỘNG HIỂN THỊ NỘI DUNG LUẬT CHƠI NGAY
        yield return new WaitForSeconds(0.3f);
        ShowLuatChoiContent();
    }

    // Hàm gọi khi bấm nút Luật chơi
    public void OnLuatChoiButtonClick()
    {
        Debug.Log("=== ĐÃ BẤM NÚT LUẬT CHƠI ===");
        ShowLuatChoiContent();
    }
    
    // Hàm hiển thị nội dung luật chơi (tách riêng để dùng chung)
    void ShowLuatChoiContent()
    {
        Debug.Log("Hiển thị nội dung Luật Chơi");
        
        // Ẩn item panel và các UI elements của item
        if (itemPanel != null) itemPanel.SetActive(false);
        if (itemTitle != null) itemTitle.gameObject.SetActive(false);
        if (itemImage != null) itemImage.gameObject.SetActive(false);
        if (itemDescription != null) itemDescription.gameObject.SetActive(false);
        
        // Hiện text nội dung
        if (textNoiDung != null) textNoiDung.gameObject.SetActive(true);
        
        if (textNoiDung != null)
        {
            textNoiDung.text = luatChoiContent;
            textNoiDung.ForceMeshUpdate();
            Debug.Log("Đã hiển thị luật chơi: " + textNoiDung.text.Length + " ký tự");
        }
        else
        {
            Debug.LogError("textNoiDung = NULL!");
        }
    }

    // Hàm gọi khi bấm nút Vật cản
    public void OnVatCanButtonClick()
    {
        Debug.Log("=== ĐÃ BẤM NÚT VẬT CẢN ===");
        
        // Ẩn item panel và các UI elements của item
        if (itemPanel != null) itemPanel.SetActive(false);
        if (itemTitle != null) itemTitle.gameObject.SetActive(false);
        if (itemImage != null) itemImage.gameObject.SetActive(false);
        if (itemDescription != null) itemDescription.gameObject.SetActive(false);
        
        // Hiện text nội dung
        if (textNoiDung != null) textNoiDung.gameObject.SetActive(true);
        
        if (textNoiDung != null)
        {
            textNoiDung.text = vatCanContent;
            textNoiDung.ForceMeshUpdate();
            Debug.Log("Đã hiển thị vật cản: " + textNoiDung.text.Length + " ký tự");
        }
        else
        {
            Debug.LogError("textNoiDung = NULL!");
        }
    }

    // Hàm gọi khi bấm nút Item
    public void OnItemButtonClick()
    {
        Debug.Log("=== ĐÃ BẤM NÚT ITEM ===");
        
        // Ẩn text content
        if (textNoiDung != null) textNoiDung.gameObject.SetActive(false);
        
        // Hiện item panel và các UI elements
        if (itemPanel != null) itemPanel.SetActive(true);
        if (itemTitle != null) itemTitle.gameObject.SetActive(true);
        if (itemImage != null) itemImage.gameObject.SetActive(true);
        if (itemDescription != null) itemDescription.gameObject.SetActive(true);
        
        // Reset về item đầu tiên
        currentItemIndex = 0;
        ShowCurrentItem();
    }

    // Hiển thị item hiện tại
    void ShowCurrentItem()
    {
        if (itemSprites == null || itemSprites.Length == 0)
        {
            Debug.LogError("Chưa gán itemSprites trong Inspector! Cần gán 5 ảnh theo thứ tự: Banhmi, caphe, thuoclao, TuimU, nitro");
            return;
        }

        if (currentItemIndex >= items.Length)
        {
            Debug.Log("Đã hết items!");
            return;
        }

        // Cập nhật tiêu đề (ở trên đầu)
        if (itemTitle != null)
        {
            itemTitle.text = items[currentItemIndex].title;
        }

        // Cập nhật hình ảnh (ở giữa)
        if (itemImage != null && currentItemIndex < itemSprites.Length)
        {
            itemImage.gameObject.SetActive(true); // BẬT lại itemImage
            itemImage.sprite = itemSprites[currentItemIndex];
        }

        // Cập nhật mô tả (ở dưới cùng)
        if (itemDescription != null)
        {
            itemDescription.text = items[currentItemIndex].description;
        }

        Debug.Log($"Hiển thị item {currentItemIndex + 1}/{items.Length}: {items[currentItemIndex].title}");
    }

    // Chuyển sang item tiếp theo
    public void ShowNextItem()
    {
        currentItemIndex++;
        
        if (currentItemIndex >= items.Length)
        {
            // Đã hết items, quay lại đầu hoặc đóng panel
            Debug.Log("Đã xem hết tất cả items!");
            currentItemIndex = 0; // Quay lại item đầu
            // Hoặc: itemPanel.SetActive(false); để đóng panel
        }
        
        ShowCurrentItem();
    }

    private void OnContinueGameClick()
    {
        // Continue to next game scene when coming from Scene2
        Debug.Log("Continue Game clicked - loading next scene");
        SceneManager.LoadScene("MapSelectScene"); // Or whatever your next scene is
    }

    private void OnBackToMenuClick()
    {
        // Return to MainHome when coming from HomePage
        Debug.Log("Back to Menu clicked - returning to MainHome");
        SceneManager.LoadScene("MainHome");
    }
}
