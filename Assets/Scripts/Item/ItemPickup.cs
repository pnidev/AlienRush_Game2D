using UnityEngine;

public enum ItemType
{
    Bread,
    Coffee,
    Magnet,
    Boost,
    MysteryBox,
    ThuocLao // coin/money
}

public class ItemPickup : MonoBehaviour
{
    public ItemType itemType;
    public int coinValue = 1; // số coin khi nhặt Thuốc Lào
    [Header("Duration Settings")]
    public float invertDuration = 2f;
    public float blindDuration = 2.5f;
    public float dashLockDuration = 3f;
    public float speedBoostDuration = 5f;
    public float magnetDuration = 8f;

    [Header("Intensity")]
    public float speedMultiplier = 1.5f;

    [Header("Score Boost")]
    public float scoreBoostMultiplier = 3f;
    private bool pickedUp = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pickedUp) return;                // <--- NGĂN GỌI LẠI

        if (!collision.CompareTag("Player")) return;

        pickedUp = true;                     // <--- ĐÁNH DẤU ĐÃ NHẶT
        GetComponent<Collider2D>().enabled = false;     // <--- NGĂN VA CHẠM LẠI
        GetComponent<SpriteRenderer>().enabled = false; // <--- TẮT HÌNH ITEM

        AudioManager.Instance?.PlaySFX(SFXType.Pickup);
        PlayerMovement2D player = collision.GetComponent<PlayerMovement2D>();

        if (player == null) return;
        // Báo LRM rằng player đã pickup (dùng cho NoPickItems check)
        LevelRuntimeManager.I?.ReportPlayerPickup(itemType.ToString());


        if (itemType == ItemType.MysteryBox)
        {
            LevelRuntimeManager.I?.AddProgressByType(TaskType.OpenMysteryBoxCount, 1);
            OpenBox(player);
            Destroy(gameObject); 
            return;
        }

        switch (itemType)
        {
            case ItemType.Coffee:
                player.Heal(1);
                LevelRuntimeManager.I?.AddProgressByType(TaskType.EatAndUseCoffee, 1);
                break;
            case ItemType.Bread:
                player.StartDirtyBuff(5f);
                LevelRuntimeManager.I?.AddProgressByType(TaskType.EatAndUseBread, 1);
                break;
            case ItemType.Magnet:
                player.ApplyMagnet(magnetDuration);
                break;
            case ItemType.Boost:
                player.ApplySpeedBoost(speedBoostDuration, speedMultiplier);
                player.StartScoreBoost(speedBoostDuration, scoreBoostMultiplier);
                break;
            case ItemType.ThuocLao:
                CoinManager.Instance.AddCoins(coinValue);
                LevelRuntimeManager.I?.AddProgressByType(TaskType.CollectMoneyTotal, coinValue);

                // Báo pickup chung (dùng cho NoPickItems)
                LevelRuntimeManager.I?.ReportPlayerPickup(itemType.ToString());

                // nếu có UI coin: player.uiManager?.UpdateCoins(player.coins);
                break;
            default:
                Debug.LogWarning("Unhandled item type: " + itemType);
                break;
        }

        Destroy(gameObject);
    
}
            

    void OpenBox(PlayerMovement2D player)
    {
        int chance = Random.Range(0, 100); // Random number between 0 and 99


        if (chance < 25)
        {
            // 0-24: Magnet (25%)
            player.ApplyMagnet(magnetDuration);
        }
        else if (chance < 50)
        {
            // 25-49: Speed Boost (25%)
            player.ApplySpeedBoost(speedBoostDuration, speedMultiplier);
            player.StartScoreBoost(speedBoostDuration, scoreBoostMultiplier);
        }
        else if (chance < 70)
        {
            // 50-69: Invert Controls (20%)
            player.ApplyInvertControls(invertDuration);
        }
        else if (chance < 90)
        {
            // 70-89: Dash Lock (20%)
            player.ApplyDashLock(dashLockDuration);
        }
        else
        {
            // 90-99: Blind (10%)
            player.ApplyBlindness(blindDuration);
        }
    }
}
