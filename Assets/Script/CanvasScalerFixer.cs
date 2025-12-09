using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Script tự động fix Canvas Scaler cho tất cả các scene
/// Để sử dụng: Unity Editor > Tools > Fix All Canvas Scalers
/// </summary>
public class CanvasScalerFixer : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Fix All Canvas Scalers (4K Compatible)")]
    static void FixAllCanvasScalers()
    {
        // Lưu scene hiện tại
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        
        // Lấy danh sách tất cả các scene trong Build Settings
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/Start.unity",
            "Assets/Scenes/MainHome.unity",
            "Assets/Scenes/Scene1.unity",
            "Assets/Scenes/Scene2.unity",
            "Assets/Scenes/Guideline.unity",
            "Assets/Scenes/Shop.unity",
            "Assets/Scenes/Leaderboard.unity",
            "Assets/Scenes/Loading.unity"
        };
        
        int fixedCount = 0;
        
        foreach (string scenePath in scenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"Scene không tồn tại: {scenePath}");
                continue;
            }
            
            // Mở scene
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"Đang xử lý scene: {scene.name}");
            
            // Tìm tất cả Canvas trong scene
            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
            
            foreach (Canvas canvas in canvases)
            {
                // Đảm bảo Canvas ở chế độ Screen Space
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    Debug.Log($"  - Đã set Canvas '{canvas.name}' về Screen Space Overlay");
                }
                
                // Lấy hoặc thêm Canvas Scaler
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                    Debug.Log($"  - Đã thêm Canvas Scaler cho '{canvas.name}'");
                }
                
                // Cấu hình Canvas Scaler cho 4K
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); // Full HD reference
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f; // Cân bằng giữa width và height
                scaler.referencePixelsPerUnit = 100;
                
                Debug.Log($"  ✅ Đã fix Canvas Scaler cho '{canvas.name}':");
                Debug.Log($"     - UI Scale Mode: Scale With Screen Size");
                Debug.Log($"     - Reference Resolution: 1920x1080");
                Debug.Log($"     - Screen Match Mode: Match Width Or Height");
                Debug.Log($"     - Match: 0.5 (cân bằng)");
                
                fixedCount++;
            }
            
            // Lưu scene
            EditorSceneManager.SaveScene(scene);
        }
        
        Debug.Log($"\n========================================");
        Debug.Log($"✅ HOÀN TẤT! Đã fix {fixedCount} Canvas");
        Debug.Log($"========================================");
        Debug.Log($"Tất cả các scene đã được đồng bộ Canvas Scaler");
        Debug.Log($"Build game để test trên màn hình 4K!");
        
        // Hiển thị thông báo
        EditorUtility.DisplayDialog(
            "Canvas Scaler Fixed!", 
            $"Đã fix {fixedCount} Canvas thành công!\n\nTất cả scene đã đồng bộ tỉ lệ cho màn hình 4K.\n\nHãy build game và test!", 
            "OK"
        );
    }
#endif
}
