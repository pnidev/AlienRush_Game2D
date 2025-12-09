using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public Image headerImage;              // Component Image để hiển thị
    public Sprite loginSprite;             // Hình ảnh "ĐĂNG NHẬP"
    public Sprite registerSprite;          // Hình ảnh "ĐĂNG KÝ"
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button confirmButton;
    public Button googleButton;
    public Button switchToRegisterButton;
    public Button logoutButton;            // Button Logout để test
    public TextMeshProUGUI notificationText;
    public GameObject loginPanel;

    [Header("Settings")]
    public string nextSceneAfterLogin = "MainHome";
    
    private string defaultUsername = "admin";
    private string defaultPassword = "123";
    private bool isLoginMode = true;
    
    // List lưu username theo thứ tự đăng nhập
    private const string USERLIST_KEY = "UserLoginList";
    
    /// <summary>
    /// Lấy danh sách username đã đăng nhập theo thứ tự
    /// </summary>
    public static System.Collections.Generic.List<string> GetUserLoginList()
    {
        string listData = PlayerPrefs.GetString(USERLIST_KEY, "");
        var list = new System.Collections.Generic.List<string>();
        
        if (!string.IsNullOrEmpty(listData))
        {
            string[] users = listData.Split(',');
            foreach (string user in users)
            {
                if (!string.IsNullOrEmpty(user.Trim()))
                    list.Add(user.Trim());
            }
        }
        
        return list;
    }
    
    /// <summary>
    /// Thêm username vào list (nếu chưa có)
    /// </summary>
    private void AddUserToList(string username)
    {
        var list = GetUserLoginList();
        
        // Kiểm tra xem username đã có trong list chưa
        if (!list.Contains(username))
        {
            list.Add(username);
            
            // Lưu lại vào PlayerPrefs
            string listData = string.Join(",", list);
            PlayerPrefs.SetString(USERLIST_KEY, listData);
            PlayerPrefs.Save();
            
            Debug.Log($"Đã thêm '{username}' vào UserLoginList. Tổng: {list.Count} users");
        }
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey("User_" + defaultUsername))
        {
            PlayerPrefs.SetString("User_" + defaultUsername, defaultPassword);
            PlayerPrefs.Save();
        }

        // Kiểm tra xem user đã đăng nhập chưa
        bool isLoggedIn = PlayerPrefs.GetInt("IsLoggedIn", 0) == 1;
        
        Debug.Log("=== LOGIN MANAGER START ===");
        Debug.Log("IsLoggedIn PlayerPrefs value: " + PlayerPrefs.GetInt("IsLoggedIn", 0));
        Debug.Log("isLoggedIn: " + isLoggedIn);
        
        if (isLoggedIn)
        {
            // Đã đăng nhập rồi -> ẩn panel luôn
            Debug.Log("User đã đăng nhập trước đó - ẩn login panel");
            if (loginPanel != null)
                loginPanel.SetActive(false);
            
            // Hiện button logout
            if (logoutButton != null)
            {
                logoutButton.gameObject.SetActive(true);
                logoutButton.onClick.RemoveAllListeners();
                logoutButton.onClick.AddListener(Logout);
            }
            
            return; // Không cần setup các button
        }

        // Chưa đăng nhập -> hiển thị login panel
        Debug.Log("Chưa đăng nhập - hiển thị login panel");
        if (loginPanel != null)
            loginPanel.SetActive(true);
        
        // Ẩn button logout
        if (logoutButton != null)
            logoutButton.gameObject.SetActive(false);

        if (notificationText != null)
            notificationText.text = "";

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
        
        if (googleButton != null)
            googleButton.onClick.AddListener(OnGoogleLoginClick);
        
        if (switchToRegisterButton != null)
            switchToRegisterButton.onClick.AddListener(OnSwitchModeClick);

        ShowLoginMode();
    }

    void ShowLoginMode()
    {
        isLoginMode = true;
        
        // Đổi hình ảnh sang chế độ đăng nhập
        if (headerImage != null && loginSprite != null)
            headerImage.sprite = loginSprite;
        
        if (confirmPasswordInput != null) 
            confirmPasswordInput.gameObject.SetActive(false);
        
        if (usernameInput != null) usernameInput.gameObject.SetActive(true);
        if (passwordInput != null) passwordInput.gameObject.SetActive(true);
        
        ClearInputs();
        
        Debug.Log("Chế độ: Đăng nhập");
    }

    void ShowRegisterMode()
    {
        isLoginMode = false;
        
        // Đổi hình ảnh sang chế độ đăng ký
        if (headerImage != null && registerSprite != null)
            headerImage.sprite = registerSprite;
        
        if (confirmPasswordInput != null) 
            confirmPasswordInput.gameObject.SetActive(true);
        
        if (usernameInput != null) usernameInput.gameObject.SetActive(true);
        if (passwordInput != null) passwordInput.gameObject.SetActive(true);
        
        ClearInputs();
        
        Debug.Log("Chế độ: Đăng ký");
    }

    void ClearInputs()
    {
        if (usernameInput != null) usernameInput.text = "";
        if (passwordInput != null) passwordInput.text = "";
        if (confirmPasswordInput != null) confirmPasswordInput.text = "";
        if (notificationText != null) notificationText.text = "";
    }

    public void OnSwitchModeClick()
    {
        if (isLoginMode)
        {
            ShowRegisterMode();
        }
        else
        {
            ShowLoginMode();
        }
    }

    public void OnConfirmButtonClick()
    {
        Debug.Log("=== OnConfirmButtonClick được gọi ===");
        Debug.Log("isLoginMode: " + isLoginMode);
        
        if (isLoginMode)
        {
            Debug.Log("Gọi HandleLogin()");
            HandleLogin();
        }
        else
        {
            Debug.Log("Gọi HandleRegister()");
            HandleRegister();
        }
    }

    void HandleLogin()
    {
        Debug.Log("=== HandleLogin START ===");
        
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        
        Debug.Log("Username nhập: '" + username + "'");
        Debug.Log("Password nhập: '" + password + "'");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowNotification("Vui lòng nhập đầy đủ thông tin!", Color.red);
            return;
        }

        if (PlayerPrefs.HasKey("User_" + username))
        {
            string savedPassword = PlayerPrefs.GetString("User_" + username);
            
            if (savedPassword == password)
            {
                LoginSuccess(username);
            }
            else
            {
                ShowNotification("Sai mật khẩu! Vui lòng nhập lại.", Color.red);
                passwordInput.text = "";
            }
        }
        else
        {
            ShowNotification("Tên đăng nhập hoặc mật khẩu sai !", Color.red);
        }
    }

    void HandleRegister()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowNotification("Vui lòng nhập đầy đủ thông tin!", Color.red);
            return;
        }

        if (username.Length < 3)
        {
            ShowNotification("Tên đăng nhập phải có ít nhất 3 ký tự!", Color.red);
            return;
        }

        if (password.Length < 3)
        {
            ShowNotification("Mật khẩu phải có ít nhất 3 ký tự!", Color.red);
            return;
        }

        if (password != confirmPassword)
        {
            ShowNotification("Mật khẩu xác nhận không khớp!", Color.red);
            confirmPasswordInput.text = "";
            return;
        }

        if (PlayerPrefs.HasKey("User_" + username))
        {
            ShowNotification("Tên đăng nhập đã tồn tại!", Color.red);
            return;
        }

        PlayerPrefs.SetString("User_" + username, password);
        PlayerPrefs.Save();
        
        // Thêm username vào list khi đăng ký thành công
        AddUserToList(username);

        ShowNotification("Đăng ký thành công! Vui lòng đăng nhập.", Color.green);
        
        StartCoroutine(SwitchToLoginAfterDelay(2f));
    }

    IEnumerator SwitchToLoginAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowLoginMode();
    }

    public void OnGoogleLoginClick()
    {
        ShowNotification("Tính năng đăng nhập Google đang phát triển!", Color.yellow);
        Debug.Log("Google Login clicked");
    }

    void LoginSuccess(string username)
    {
        Debug.Log("Đăng nhập thành công: " + username);
        
        // Thêm username vào list khi đăng nhập thành công
        AddUserToList(username);
        
        PlayerPrefs.SetString("CurrentUsername", username);
        PlayerPrefs.SetInt("IsLoggedIn", 1);
        PlayerPrefs.Save();

        ShowNotification("Đăng nhập thành công!", Color.black );
        
        // Ẩn panel sau 1 giây
        StartCoroutine(HideLoginPanel());
    }

    IEnumerator HideLoginPanel()
    {
        yield return new WaitForSeconds(1f);
        
        if (loginPanel != null)
            loginPanel.SetActive(false);
        
        // Hiện button logout sau khi đăng nhập thành công
        if (logoutButton != null)
        {
            logoutButton.gameObject.SetActive(true);
            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(Logout);
        }
        
        Debug.Log("Login panel đã ẩn - HomePage hiển thị - Logout button hiện");
    }

    void ShowNotification(string message, Color color)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.color = color;
        }
        Debug.Log("Thông báo: " + message);
    }
    
    public void Logout()
    {
        PlayerPrefs.SetInt("IsLoggedIn", 0);
        PlayerPrefs.DeleteKey("CurrentUsername");
        PlayerPrefs.Save();
        
        Debug.Log("Đã đăng xuất");
        
        // Ẩn button logout
        if (logoutButton != null)
            logoutButton.gameObject.SetActive(false);
        
        // Hiện lại login panel
        if (loginPanel != null)
            loginPanel.SetActive(true);
        
        // Setup lại các button listeners
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
        }
        
        if (googleButton != null)
        {
            googleButton.onClick.RemoveAllListeners();
            googleButton.onClick.AddListener(OnGoogleLoginClick);
        }
        
        if (switchToRegisterButton != null)
        {
            switchToRegisterButton.onClick.RemoveAllListeners();
            switchToRegisterButton.onClick.AddListener(OnSwitchModeClick);
        }
        
        // Reset về chế độ đăng nhập
        ShowLoginMode();
    }
}
