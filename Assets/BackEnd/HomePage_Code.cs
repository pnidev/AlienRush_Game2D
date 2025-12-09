using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện bắt buộc để chuyển cảnh

public class MenuController : MonoBehaviour
{
    [Header("Particle System Settings")]
    [Tooltip("Kéo thả các Particle System cần quản lý vào đây")]
    public ParticleSystem[] particleSystems;
    // --- KHU VỰC CHUYỂN CẢNH (SCENE) ---

    // 1. Hàm cho nút BẮT ĐẦU
    public void BamNutStart()
    {
        SceneTransition.LoadSceneWithLoading("Start", 5f);
    }

    // 1b. Alias cho BamNutStart (để tương thích với code cũ)
    public void BamNutBatDau()
    {
        SceneTransition.LoadSceneWithLoading("Start", 5f);
    }

    // 2. Hàm cho nút SHOP (Tạp hóa)
    public void BamNutShop()
    {
        // Chuyển sang scene tên là "Shop"
        SceneManager.LoadScene("Setting");
    }
    public void BamNutTRoVeMenu()
    {
        // Chuyển sang scene tên là "HomePage"
        SceneManager.LoadScene("MainHome");
    }    

    // 3. Hàm cho nút HƯỚNG DẪN
    public void BamNutHuongDan()
    {
        // Đánh dấu đây là scene thứ 2 (HomePage → Guideline)
        PlayerPrefs.SetInt("SceneCount", 2);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Guideline");
    }

    // --- KHU VỰC CHỨC NĂNG HỆ THỐNG ---

    // 3. Hàm cho nút THOÁT GAME
    public void BamNutThoat()
    {
        Debug.Log("Đã bấm nút Thoát! (Chỉ tắt thật khi đóng gói file .exe)");
        Application.Quit();
    }

    // --- KHU VỰC CÁC NÚT PHỤ (Setting, Bảng xếp hạng...) ---
    // Các nút này thường chỉ mở popup chứ không chuyển cảnh,
    // tạm thời mình để lệnh in ra màn hình Console để test trước nhé.

    public void BamNutSetting()
    {
        Debug.Log("Mở bảng Cài đặt...");
    }

    public void BamNutXepHang()
    {
        Debug.Log("Mở Bảng xếp hạng...");
        SceneManager.LoadScene("Leaderboard");
    }

    // --- KHU VỰC QUẢN LÝ PARTICLE SYSTEM ---
    
    void Start()
    {
        Debug.Log("=== PARTICLE SYSTEM DEBUG ===");
        Debug.Log($"So luong Particle Systems: {(particleSystems != null ? particleSystems.Length : 0)}");
        Debug.Log($"Application.isPlaying: {Application.isPlaying}");
        
        // Quản lý các Particle Systems đã được kéo thả vào Inspector
        if (particleSystems != null && particleSystems.Length > 0)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem ps = particleSystems[i];
                if (ps != null)
                {
                    Debug.Log($"Particle {i}: {ps.gameObject.name}");
                    Debug.Log($"  - IsPlaying: {ps.isPlaying}");
                    Debug.Log($"  - PlayOnAwake: {ps.main.playOnAwake}");
                    Debug.Log($"  - Looping: {ps.main.loop}");
                    
                    if (Application.isPlaying)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        ps.Play();
                        Debug.Log($"  - DA PLAY particle {ps.gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Particle {i}: NULL (chua duoc gan)");
                }
            }
        }
        else
        {
            Debug.LogError("KHONG CO PARTICLE SYSTEM NAO! Hay keo tha vao Inspector.");
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        // Trong Unity Editor, dừng particle khi không ở Play mode
        if (!Application.isPlaying && particleSystems != null)
        {
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps != null && ps.isPlaying)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }
#endif
}
