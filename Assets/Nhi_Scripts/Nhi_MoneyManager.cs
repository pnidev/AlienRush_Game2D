//using System;
//using UnityEngine;

//public class MoneyManager : MonoBehaviour
//{
//    public static MoneyManager I { get; private set; }
//    public int CurrentMoney { get; private set; } = 0; // đây là số thuốc (count)

//    public event Action<int> OnMoneyChanged;

//    void Awake()
//    {
//        if (I == null) { I = this; DontDestroyOnLoad(gameObject); }
//        else Destroy(gameObject);
//    }

//    public void ResetMoneyForMap(int mapIndex)
//    {
//        CurrentMoney = 0;
//        OnMoneyChanged?.Invoke(CurrentMoney);
//    }

//    public void AddMoney(int amount)
//    {
//        if (amount == 0) return;
//        CurrentMoney += amount;
//        OnMoneyChanged?.Invoke(CurrentMoney);
//        Debug.Log($"[Money] Add {amount} -> CurrentMoney={CurrentMoney}");
//    }

//    public void ResetMoneyToZero()
//    {
//        CurrentMoney = 0;
//        // nếu emit event UI, gọi event ở đây
//        OnMoneyChanged?.Invoke(CurrentMoney);
//    }

//}
using System;
using UnityEngine;

public class Nhi_MoneyManager : MonoBehaviour
{
    public static Nhi_MoneyManager I { get; private set; }
    public event Action<int> OnMoneyChanged;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // Ensure CoinManager exists
        if (CoinManager.I == null)
            new GameObject("CoinManager").AddComponent<CoinManager>();

        // optional: subscribe and forward event
        CoinManager.I.OnMoneyChanged += (m) => OnMoneyChanged?.Invoke(m);
    }

    public int CurrentMoney => CoinManager.I != null ? CoinManager.I.CurrentMoney : 0;
    public void ResetMoneyForMap(int mapIndex) => CoinManager.I?.ResetMoneyForMap(mapIndex);
    public void AddMoney(int amount) => CoinManager.I?.AddMoney(amount);
}
