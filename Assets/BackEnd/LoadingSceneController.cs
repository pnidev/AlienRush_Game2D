using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image progressBarFill; // Image vàng
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipsText; // Text hiển thị gợi ý
    [SerializeField] private RectTransform loadingIcon; // Icon "I" nếu muốn xoay
    
    [Header("Cấu hình")]
    [SerializeField] private float iconRotateSpeed = 200f;
    [SerializeField] private string testSceneName = "Start"; // Scene test khi chạy LoadingScene trực tiếp

    private void Start()
    {
        Debug.Log("LoadingSceneController Started!");
        
        // Kiểm tra references
        if (progressBarFill == null) Debug.LogError("progressBarFill chưa được gán!");
        if (loadingText == null) Debug.LogError("loadingText chưa được gán!");
        if (tipsText == null) Debug.LogError("tipsText chưa được gán!");
        
        // Nếu không có scene đích (test mode), dùng testSceneName
        if (string.IsNullOrEmpty(SceneTransition.targetScene))
        {
            Debug.LogWarning($"Chạy LoadingScene ở test mode, sẽ load: {testSceneName}");
            SceneTransition.targetScene = testSceneName;
            SceneTransition.minimumLoadTime = 5f;
        }
        
        // Reset fill amount
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0;
            Debug.Log("Reset progressBarFill về 0");
        }
        
        StartCoroutine(LoadTargetSceneAsync());
        StartCoroutine(AnimateLoadingText());
        StartCoroutine(ShowRandomTips());
    }

    private void Update()
    {
        // Xoay icon nếu có
        if (loadingIcon != null)
        {
            loadingIcon.Rotate(0, 0, -iconRotateSpeed * Time.deltaTime);
        }
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        float startTime = Time.time;
        float requiredDuration = 5f; // Tổng thời gian loading = 5s
        
        // Lấy scene đích
        string targetScene = SceneTransition.targetScene;
        float minLoadTime = SceneTransition.minimumLoadTime;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("Không có scene đích! Không thể load.");
            yield break; // Dừng coroutine nếu không có scene
        }

        // Kiểm tra scene có trong Build Settings không
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        bool sceneExists = false;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == targetScene)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"Scene '{targetScene}' không có trong Build Settings! Vào File > Build Settings để thêm scene.");
            yield break;
        }

        // Load scene async
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        if (operation == null)
        {
            Debug.LogError($"Không thể load scene '{targetScene}'!");
            yield break;
        }
        
        operation.allowSceneActivation = false;

        // Animate progress bar
        // Đợi đủ thời gian 5s và cho đến khi tiến trình đạt 90%
        while (Time.time - startTime < requiredDuration || operation.progress < 0.9f)
        {
            float timeProgress = (Time.time - startTime) / requiredDuration;
            float loadProgress = operation.progress / 0.9f;
            float progress = Mathf.Min(timeProgress, loadProgress);
            
            // Cập nhật fill amount (0 → 1)
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = progress;
            }
            // Hiển thị phần trăm trên text loading
            if (loadingText != null)
            {
                loadingText.text = $"LOADING... {(progress * 100f):0}%";
            }
            
            yield return null;
        }

        // Fill đầy
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 1f;
        }
        if (loadingText != null)
        {
            loadingText.text = "LOADING... 100%";
        }

        yield return new WaitForSeconds(0.2f);
        
        // Chuyển scene
        operation.allowSceneActivation = true;
    }

    private IEnumerator AnimateLoadingText()
    {
        // Coroutine này không còn cần thiết vì loadingText giờ hiển thị %
        yield break;
    }

    // Hiển thị gợi ý ngẫu nhiên, mỗi 2s, tổng 5s
    private IEnumerator ShowRandomTips()
    {
        if (tipsText == null) yield break;

        string[] tips = new string[]
        {
            "Chạy thật nhanh trước khi bị bắt",
            "Né các vật cản",
            "Lụm thật nhiều bonus",
            "Thất bại là mẹ thành công",
            "Pass OJT là chính"
        };

        float totalDuration = 5f;
        float interval = 2f;
        float elapsed = 0f;

        // Hiển thị lần đầu ngay
        tipsText.text = tips[Random.Range(0, tips.Length)];
        while (elapsed < totalDuration)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;
            tipsText.text = tips[Random.Range(0, tips.Length)];
        }
    }
}