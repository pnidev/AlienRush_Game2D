using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PersistentPlayerManager (improved):
/// - Singleton manager (DontDestroyOnLoad)
/// - Giữ 1 player persistent qua scene hoặc convert scene player thành persistent
/// - Reset state player khi scene loaded
/// - Bảo vệ trường hợp duplicate / race condition
/// </summary>
public class PersistentPlayerManager : MonoBehaviour
{
    public static PersistentPlayerManager I { get; private set; }

    [Tooltip("Tag dùng cho Player prefab (mặc định 'Player')")]
    public string playerTag = "Player";

    private GameObject persistentPlayer;

    private void Awake()
    {
        // Singleton pattern: giữ duy nhất 1 manager
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        //DontDestroyOnLoad(gameObject);

        // Nếu có player trong scene hiện tại thì đăng ký nó là persistent
        TryFindAndMakePersistentPlayerOnAwake();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (I == this) I = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void TryFindAndMakePersistentPlayerOnAwake()
    {
        // Bỏ make persistent ở Awake vì không cần cho single scene restart
        // Chỉ log nếu cần debug
        var found = GameObject.FindWithTag(playerTag);
        if (found != null)
        {
            Debug.Log("[PPM] Found player on Awake but NOT making persistent (single scene mode): " + found.name);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Restore global states (safety)
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Tìm TẤT CẢ players (nếu có duplicate từ lỗi cũ)
        var allPlayers = GameObject.FindGameObjectsWithTag(playerTag);

        // Vì không persistent, destroy duplicate nếu có (thường không, nhưng an toàn)
        if (allPlayers.Length > 1)
        {
            for (int i = 1; i < allPlayers.Length; i++)
            {
                Debug.Log("[PPM] Destroying duplicate player on scene load: " + allPlayers[i].name);
                Destroy(allPlayers[i]);
            }
        }

        // Lấy player chính (nếu có) và reset state
        if (allPlayers.Length > 0)
        {
            persistentPlayer = allPlayers[0];  // Giữ ref tạm để reset
            EnsurePersistentPlayerActiveAndReset();
            Debug.Log("[PPM] Reset player on scene load (no persistent): " + persistentPlayer.name);
        }
        else
        {
            Debug.LogWarning("[PPM] No player found in scene!");
        }
    }

    private void EnsurePersistentPlayerActiveAndReset()
    {
        if (persistentPlayer == null) return;

        try
        {
            persistentPlayer.SetActive(true);
        }
        catch
        {
            // if object was destroyed unexpectedly, clear ref
            persistentPlayer = null;
            Debug.LogWarning("[PPM] persistentPlayer reference invalidated.");
            return;
        }

        var pm = persistentPlayer.GetComponent<PlayerMovement2D>();
        if (pm != null)
        {
            pm.ResetStateAfterSceneLoad();
        }
    }

    /// <summary>
    /// Make the given GameObject persistent (DontDestroyOnLoad).
    /// If already have one, keep existing.
    /// </summary>
    public void MakePersistent(GameObject player)
    {
        if (player == null) return;

        // If we already have a persistent player and it's different, do nothing.
        if (persistentPlayer != null && persistentPlayer != player)
        {
            Debug.Log("[PPM] Persistent player already set, skipping MakePersistent for: " + player.name);
            return;
        }

        persistentPlayer = player;

        // Make sure we take the root GameObject (in case player is nested)
        GameObject root = persistentPlayer.transform.root.gameObject;
        DontDestroyOnLoad(root);
        persistentPlayer = root;

        Debug.Log("[PPM] Player made persistent: " + persistentPlayer.name);
    }

    /// <summary>
    /// Destroys the persistent player if any and clears reference.
    /// Use if you explicitly want scene player to be used next load.
    /// </summary>
    public void DestroyPersistentPlayer()
    {
        if (persistentPlayer != null)
        {
            Debug.Log("[PPM] Destroying persistent player: " + persistentPlayer.name);
            Destroy(persistentPlayer);
            persistentPlayer = null;
        }
    }

    /// <summary>
    /// Public accessor (read-only) for other systems.
    /// </summary>
    public GameObject GetPersistentPlayer() => persistentPlayer;
}
