using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;

public class FlaskServerManager : MonoBehaviour
{
    private static Process serverProcess;
    private static bool serverStarted = false;

    void Awake()
    {
        // Singleton pattern - chỉ chạy 1 lần
        if (serverStarted)
        {
            return;
        }

        DontDestroyOnLoad(gameObject);
        StartFlaskServer();
    }

    void StartFlaskServer()
    {
        try
        {
            // Đường dẫn đến flask_server.py
            string projectPath = Application.dataPath.Replace("/Assets", "");
            string pythonScript = Path.Combine(projectPath, "Thư viện", "flask_server.py");

            UnityEngine.Debug.Log("Đang khởi động Flask server từ: " + pythonScript);

            if (!File.Exists(pythonScript))
            {
                UnityEngine.Debug.LogError("❌ Không tìm thấy flask_server.py!");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{pythonScript}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.Combine(projectPath, "Thư viện")
            };

            serverProcess = Process.Start(startInfo);
            serverStarted = true;
            
            UnityEngine.Debug.Log("✅ Flask server đã khởi động từ Unity!");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("❌ Lỗi khi khởi động Flask server: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        StopFlaskServer();
    }
    
    void OnDestroy()
    {
        // Không gọi lại OnApplicationQuit để tránh duplicate
        // OnApplicationQuit sẽ tự động được gọi khi thoát
    }
    
    private void StopFlaskServer()
    {
        // Tắt Flask server khi thoát Unity
        if (serverProcess != null)
        {
            try
            {
                if (!serverProcess.HasExited)
                {
                    serverProcess.Kill();
                    serverProcess.WaitForExit(2000);
                    UnityEngine.Debug.Log("Flask server đã được dừng.");
                }
                serverProcess.Dispose();
                serverProcess = null;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Lỗi khi dừng Flask server: " + e.Message);
            }
        }
    }
}
