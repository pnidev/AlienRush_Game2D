using UnityEngine;

public class GameManagerLoader : MonoBehaviour
{
    // tên prefab phía trong Assets/Resources (không có .prefab)
    public string gameManagerResourceName = "GameManager";

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmPrefab = Resources.Load<GameObject>(gameManagerResourceName);
            if (gmPrefab != null)
            {
                Instantiate(gmPrefab);
                Debug.Log("[GameManagerLoader] Instantiated GameManager from Resources.");
            }
            else
            {
                Debug.LogError("[GameManagerLoader] Could not find GameManager prefab in Resources. Please place prefab at Assets/Resources/GameManager.prefab");
            }
        }
    }
}
