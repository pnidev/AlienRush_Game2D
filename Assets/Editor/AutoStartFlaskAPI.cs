using UnityEngine;
using UnityEditor;
using System.Diagnostics;

// TẮT TỰ ĐỘNG CHẠY - Code này đã bị vô hiệu hóa
// [InitializeOnLoad]
public class AutoStartFlaskAPI
{
    private static Process apiProcess;
    
    // TẮT TỰ ĐỘNG CHẠY KHI MỞ UNITY EDITOR
    /*
    static AutoStartFlaskAPI()
    {
        // Tự động chạy khi Unity Editor khởi động
        EditorApplication.update += OnEditorStartup;
    }
    */
    
    private static void OnEditorStartup()
    {
        // Chỉ chạy 1 lần khi mở Unity
        EditorApplication.update -= OnEditorStartup;
        
        // Kiểm tra API đã chạy chưa
        if (!IsAPIRunning())
        {
            StartFlaskAPI();
        }
    }
    
    private static bool IsAPIRunning()
    {
        try
        {
            using (var client = new System.Net.WebClient())
            {
                client.DownloadString("http://127.0.0.1:5000/health");
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
    
    private static void StartFlaskAPI()
    {
        string batPath = Application.dataPath.Replace("/Assets", "") + "/Thư viện/start_api.bat";
        
        if (System.IO.File.Exists(batPath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = batPath,
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Minimized
            };
            
            apiProcess = Process.Start(startInfo);
            UnityEngine.Debug.Log("🚀 Flask API đã được khởi động tự động!");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Không tìm thấy start_api.bat");
        }
    }
}
