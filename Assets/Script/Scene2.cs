using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Scene2 : MonoBehaviour
{
    public RawImage resultImage;
    public Text textName;
    public Text textGender;
    
    [Header("Default Image")]
    public Texture2D defaultImage;  // Ảnh mặc định nếu không chụp
    
    [Header("Navigation")]
    public Button continueButton; // Nút Continue để chuyển sang Guideline
    public Button backButton;     // Nút Back để quay lại Scene1

    void Start()
    {
        // Kiểm tra null để tránh lỗi
        if (textName != null)
        {
            textName.text = PlayerData.playerName;
        }
        else
        {
            Debug.LogError("textName chưa được gán trong Inspector!");
        }

        if (textGender != null)
        {
            textGender.text = PlayerData.playerGender;
        }
        else
        {
            Debug.LogError("textGender chưa được gán trong Inspector!");
        }

        if (PlayerData.playerPhoto != null)
        {
            if (resultImage != null)
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(PlayerData.playerPhoto);
                resultImage.texture = tex;
            }
            else
            {
                Debug.LogError("resultImage chưa được gán trong Inspector!");
            }
        }
        else
        {
            // Nếu không có ảnh chụp → hiển thị ảnh mặc định
            if (defaultImage != null && resultImage != null)
            {
                Debug.Log("Hiển thị ảnh mặc định");
                resultImage.texture = defaultImage;
            }
            else
            {
                Debug.LogWarning("Không có ảnh để hiển thị!");
            }
        }
        
        // Gán sự kiện cho nút Continue
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClick);
        }
        else
        {
            Debug.LogError("continueButton chưa được gán trong Inspector!");
        }
        
        // Gán sự kiện cho nút Back
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
        else
        {
            Debug.LogError("backButton chưa được gán trong Inspector!");
        }
    }
    
    /// <summary>
    /// Hàm xử lý khi bấm nút Continue - chuyển sang scene Guideline
    /// </summary>
    public void OnContinueButtonClick()
    {
        // Đánh dấu đã qua nhiều scene (HomePage → Loading → Start → Scene1 → Scene2 → Guideline)
        PlayerPrefs.SetInt("SceneCount", 6); // 6 scenes trước khi đến Guideline
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("Guideline");
    }
    
    /// <summary>
    /// Hàm xử lý khi bấm nút Back - quay lại Scene1
    /// </summary>
    public void OnBackButtonClick()
    {
        Debug.Log("Back button clicked - quay lại Scene1");
        SceneManager.LoadScene("Scene1");
    }
}
