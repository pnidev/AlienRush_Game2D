using System;
using UnityEngine;

/// <summary>
/// Unified CoinManager (single source of truth).
/// - Supports both old and new API names so existing scripts keep compiling:
///   * Singleton: I and Instance
///   * Methods: AddCoins / AddMoney, SpendCoins / SpendMoney
///   * Properties: CurrentCoins / CurrentMoney
///   * Events: OnCoinsChanged / OnMoneyChanged
///   * ResetMoneyForMap(mapIndex)
/// - Persists via PlayerPrefs (key = "TotalCoins").
/// </summary>
public class CoinManager : MonoBehaviour
{
    // --- Singleton accessible by either name (I or Instance) ---
    public static CoinManager I { get; private set; }
    public static CoinManager Instance => I;

    // Persistent coin total (used for shop/revive, persisted across maps)
    public int CurrentCoins { get; private set; } = 0;

    // Backwards-compat property name
    public int CurrentMoney => CurrentCoins;

    // Events: both names will be invoked when value changes
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnMoneyChanged; // alias for older code

    private const string COIN_KEY = "TotalCoins";

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        LoadCoins();
    }

    void Start()
    {
        // emit initial value so any UI that subscribes after Awake can get it
        EmitCoinsChanged();
    }

    // ---------------- Public API (preferred) ----------------
    public void AddCoins(int amount)
    {
        if (amount == 0) return;
        CurrentCoins += amount;
        SaveCoins();
        EmitCoinsChanged();
        Debug.Log($"[CoinManager] AddCoins({amount}) -> {CurrentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentCoins >= amount)
        {
            CurrentCoins -= amount;
            SaveCoins();
            EmitCoinsChanged();
            Debug.Log($"[CoinManager] SpendCoins({amount}) -> {CurrentCoins}");
            return true;
        }
        Debug.Log("[CoinManager] SpendCoins failed (not enough coins)");
        return false;
    }

    public void ResetCoins()
    {
        CurrentCoins = 0;
        SaveCoins();
        EmitCoinsChanged();
        Debug.Log("[CoinManager] ResetCoins -> 0");
    }

    // ---------------- Backwards-compatible API (aliases) ----------------

    // old name AddMoney -> forward to AddCoins
    public void AddMoney(int amount) => AddCoins(amount);

    // old name and property CurrentMoney
    // public int CurrentMoney => CurrentCoins; // already declared above as CurrentMoney

    // alias SpendMoney -> forward to SpendCoins
    public bool SpendMoney(int amount) => SpendCoins(amount);

    // If other scripts call CoinManager.Instance.AddCoins(...) or Instance.AddMoney(...) both will work.

    // ResetMoneyForMap: keep compatibility. Default: reset *run* money.
    // If you intend coins to persist across maps, don't call this on map start.
    public void ResetMoneyForMap(int mapIndex)
    {
        // Behavior decision: default implementation resets run/temporary money.
        // If your coins are persistent and you don't want to reset, change this logic accordingly.
        ResetCoins();
        Debug.Log($"[CoinManager] ResetMoneyForMap(map={mapIndex}) called -> coins reset");
    }

    // ---------------- Internal helpers ----------------
    private void EmitCoinsChanged()
    {
        OnCoinsChanged?.Invoke(CurrentCoins);
        OnMoneyChanged?.Invoke(CurrentCoins);
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COIN_KEY, CurrentCoins);
        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        if (PlayerPrefs.HasKey(COIN_KEY))
            CurrentCoins = PlayerPrefs.GetInt(COIN_KEY);
        else
            CurrentCoins = 0;
    }

    private void OnApplicationQuit()
    {
        SaveCoins();
    }
}
