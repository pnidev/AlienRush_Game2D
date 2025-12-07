using UnityEngine;

public class CoinItem : MonoBehaviour
{
    // Đảm bảo rằng thuộc tính này là public hoặc có phương thức getter/setter công khai
    [SerializeField]
    public int coinValueAddition = 1;  // Giá trị của đồng xu khi thu thập, có thể thay đổi từ Inspector

    // OnTriggerEnter2D xử lý va chạm với player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu đối tượng va chạm là player
        PlayerMovement2D player = collision.GetComponent<PlayerMovement2D>();

        if (player != null)
        {
            // Thêm xu vào thông qua CoinManager
            CoinManager.Instance.AddCoins(coinValueAddition);
            AudioManager.Instance?.PlaySFX(SFXType.Pickup);

            Debug.Log("Stonks! +" + coinValueAddition + " Coins!");
            Debug.Log("Total Coins: " + CoinManager.Instance.CurrentCoins);

            // Hủy đối tượng coin sau khi thu thập
            Destroy(gameObject);
        }
    }
}
