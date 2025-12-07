using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Core systems")]
    public PlayerCurrency playerCurrency;
    // playerEffects có thể gắn trực tiếp lên GameManager (nếu muốn) 
    // hoặc sẽ được tìm khi MainScene load (auto-assign)
    //public PlayerEffects playerEffects;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // đảm bảo PlayerCurrency có component (phòng trường hợp instance từ prefab chưa set)
        if (playerCurrency == null)
        {
            playerCurrency = GetComponent<PlayerCurrency>();
        }

        // lắng nghe scene load để tự gán PlayerEffects khi MainScene load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //    // Nếu scene có Player (tag "Player"), auto tìm PlayerEffects
        //    GameObject p = GameObject.FindGameObjectWithTag("Player");
        //    if (p != null)
        //    {
        //        var pe = p.GetComponent<PlayerEffects>();
        //        if (pe != null)
        //        {
        //            playerEffects = pe;
        //            Debug.Log($"[GameManager] PlayerEffects assigned from scene '{scene.name}'");
        //        }
        //    }
        //}
    }
}
