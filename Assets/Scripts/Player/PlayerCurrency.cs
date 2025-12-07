using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public int money = 0;

    public void AddMoney(int amount)
    {
        money += amount;
        // TODO: update UI Text/TMP
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount) return false;

        money -= amount;
        // TODO: update UI
        return true;
    }
}
