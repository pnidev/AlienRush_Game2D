using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script test hành động player bằng phím.
/// Gắn vào Player (hoặc 1 GameObject test) để mô phỏng các hành động cho LevelRuntimeManager.
/// </summary>
public class PlayerTestActions : MonoBehaviour
{
    // Số tiền khi nhấn M (giả lập thu 10k)
    public int moneyChunk = 10000;

    void Update()
    {
        // =========== Task tiêu chuẩn ===========
        // A: Eat & use bread
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("[PlayerTest] EatAndUseBread");
            LevelRuntimeManager.I?.AddProgressByType(TaskType.EatAndUseBread, 1);
        }

        // C: Eat & use coffee
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[PlayerTest] EatAndUseCoffee");
            LevelRuntimeManager.I?.AddProgressByType(TaskType.EatAndUseCoffee, 1);
        }

        // D: Dash (tăng 1 lần dash)
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("[PlayerTest] DashThroughCount +1");
            LevelRuntimeManager.I?.AddProgressByType(TaskType.DashThroughCount, 1);
        }

        // O: Open a mystery box (tăng 1)
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("[PlayerTest] OpenMysteryBoxCount +1");
            LevelRuntimeManager.I?.AddProgressByType(TaskType.OpenMysteryBoxCount, 1);
        }

        // Y: Buy from shop (mua 1 món)
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[PlayerTest] BuyFromShop +1");
            LevelRuntimeManager.I?.AddProgressByType(TaskType.BuyAfterDieCount, 1);
        }


        //// M: Collect one tobacco (thuốc lào) - đơn vị = 1
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    Debug.Log("[PlayerTest] Collect 1 tobacco");
        //    // Add 1 unit to MoneyManager (hiện là thuốc lào)
        //    MoneyManager.I?.AddMoney(1);

        //    // Ghi progress thu tiền (sử dụng đơn vị 1)
        //    LevelRuntimeManager.I?.AddProgressByType(TaskType.CollectMoneyTotal, 1);
        //}



        // S: (dùng cho ScoreAtLeast test) - bạn có thể tăng tùy ý
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    Debug.Log("[PlayerTest] Add small score +100");
        //    ScoreManager.I?.AddScore(100);
        //    // Nếu level có ScoreAtLeast task, bạn có thể gọi AddProgressByType nếu bạn muốn quản bằng progress:
        //    // LevelRuntimeManager.I?.AddProgressByType(TaskType.ScoreAtLeast, 100);
        //}

        // T: Boss defeated
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[PlayerTest] Boss defeated (T)");
            LevelRuntimeManager.I?.OnBossDefeated();
        }

        // =========== Các sự kiện 'vi phạm' / special ===========
        // P: simulate pickup (nhặt item) -> báo cho LRM (dùng để test NoPickItems)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[PlayerTest] Simulate Pickup (P)");
            LevelRuntimeManager.I?.ReportPlayerPickup("test_pick");
            // Nếu bạn muốn cũng ghi nhận open mystery: uncomment:
            // LevelRuntimeManager.I?.AddProgressByType(TaskType.OpenMysteryBoxCount, 1);
        }

        // K: simulate collision with obstacle -> báo cho LRM (test NoCollisionWithObstacles)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[PlayerTest] Simulate Collision with Obstacle (K)");
            LevelRuntimeManager.I?.ReportCollisionWithObstacle();
        }

        // DownArrow: start RequireRightTurn timer (simulate start window)
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("[PlayerTest] Trigger RequireRightTurn timer (DownArrow)");
            LevelRuntimeManager.I?.TriggerNoTurnRightNow();
        }


        // UpArrow => HỦY timer NoTurnRight (stop counting)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("[PlayerTest] Cancel RequireRightTurn timer (UpArrow) - stopping");
            LevelRuntimeManager.I?.CancelNoTurnRightNow();
        }



        // H: simulate take damage -> báo cho LRM (test NoDamageRun)
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[PlayerTest] Simulate Take Damage 1 (H)");
            LevelRuntimeManager.I?.ReportPlayerTookDamage(1);
            // Nếu trong game bạn muốn giảm máu thật, gọi phương thức health ở player ở đây.
        }

        //// =========== Hỗ trợ debug: reset save ===========
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    Debug.Log("[PlayerTest] FULL RESET (R) - Reset save, stop timers, reset scores, money, go to Map1Scene");

        //    // 1) Xóa save (unlock/complete)
        //    GameSave.ResetAll();

        //    // 2) Dừng/hủy các timer/runtime còn chạy (ví dụ NoTurnRightTimer)
        //    LevelRuntimeManager.I?.CancelNoTurnRightNow();

        //    // 3) Reset run score (nếu ScoreManager có method này)
        //    ScoreManager.I?.ResetRunScoreForMap(0); // hoặc ResetRunScoreForMap(1) phụ thuộc bạn dùng index map nào

        //    // 4) Reset tiền
        //    MoneyManager.I?.ResetMoneyToZero();

        //    // 5) (Tùy) reset các state khác nếu cần
        //    // e.g. InventoryManager.I?.ClearAll(); hoặc PlayerHealth.I?.ResetHealth();

        //    // 6) Load về map 1 (scene name)
        //    UnityEngine.SceneManagement.SceneManager.LoadScene("Map1Scene");
        }



    
}
