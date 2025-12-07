using System.Collections.Generic;
using UnityEngine;

public class LaneSpawner : MonoBehaviour
{
    [Header("Player & spacing")]
    public Transform player;
    public float spawnInterval = 8f;         // mỗi ô cách bao xa (m)
    public float initialSpawnAhead = 10f;    // spawn sớm trước player khi bắt đầu
    public float despawnDistance = 30f;      // khi object xa player bao nhiêu thì hủy / trả pool

    [Header("Lanes (Y positions)")]
    public float[] laneYs = new float[] { -0.01f, 2f };

    [Header("Obstacle prefabs (hard/soft)")]
    public GameObject[] hardObstacles; // xe cẩu, container...
    public GameObject[] softObstacles; // ổ gà, cỏ...

    [Header("Items prefabs")]
    public GameObject prefabBanhMi;
    public GameObject prefabCafe;
    public GameObject prefabMagnet;
    public GameObject prefabBoost;
    public GameObject prefabMystery;
    public GameObject prefabThuocLao; // tiền

    [Header("Spawn probabilities (grouped)")]
    [Range(0f, 1f)] public float emptyRatio = 0.55f;
    [Range(0f, 1f)] public float obstacleRatio = 0.30f;
    [Range(0f, 1f)] public float itemRatio = 0.15f;

    [Header("Obstacle split (within obstacle group)")]
    [Range(0f, 1f)] public float hardObstacleRatio = 0.7f;

    [Header("Row rules")]
    [Tooltip("Tối đa bao nhiêu obstacle (hard/soft) xuất trong cùng 1 row (mặc định 1)")]
    public int maxObstaclesPerRow = 1;

    [Header("Pooling & housekeeping")]
    public bool usePooling = true;
    public Transform spawnRoot; // parent cho các object spawn (tốt để giữ scene gọn)
    public int initialPoolPerPrefab = 5;

    // internal
    private float distanceAccumulator = 0f;
    private float lastPlayerX;
    private float nextSpawnX; // optional approximate X for debug/visual
    private Dictionary<GameObject, Queue<GameObject>> pool = new Dictionary<GameObject, Queue<GameObject>>();
    private List<GameObject> activeSpawns = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("LaneSpawner: player not assigned.");
            enabled = false;
            return;
        }

        lastPlayerX = player.position.x;
        distanceAccumulator = 0f;
        nextSpawnX = player.position.x + initialSpawnAhead;

        // init pool
        if (usePooling)
        {
            AddPoolForPrefabArray(hardObstacles);
            AddPoolForPrefabArray(softObstacles);
            AddPool(prefabBanhMi);
            AddPool(prefabCafe);
            AddPool(prefabMagnet);
            AddPool(prefabBoost);
            AddPool(prefabMystery);
            AddPool(prefabThuocLao);
        }

#if UNITY_EDITOR
        // debug quick test: uncomment to auto spawn test on Start
        // SpawnOneOfEachForTest();
#endif
    }

    void Update()
    {
        // 1) accumulate forward distance traveled by player (works even if world X is manipulated)
        float deltaX = player.position.x - lastPlayerX;
        // robust: chỉ thêm phần tiến về phía trước, tránh bỏ qua do precision hoặc negative
        distanceAccumulator += Mathf.Max(0f, deltaX);
        lastPlayerX = player.position.x;

        // 2) spawn while accumulator vượt spawnInterval (có thể spawn nhiều ô nếu player chạy nhanh)
        while (distanceAccumulator >= spawnInterval)
        {
            distanceAccumulator -= spawnInterval;
            float spawnX = player.position.x + initialSpawnAhead; // spawn luôn ở phía trước player
            SpawnRow(spawnX);
            nextSpawnX = spawnX + spawnInterval;
        }

        // 3) despawn objects phía sau
        CleanupBehindPlayer();
    }

    // --- NEW: Row-based spawning with normalization and rules
    void SpawnRow(float x)
    {
        //// normalize ratios (tránh trường hợp người dev set sai)
        //float e = emptyRatio;
        //float o = obstacleRatio;
        //float it = itemRatio;
        //float sum = e + o + it;
        //if (sum <= 0f)
        //{
        //    // fallback an toàn
        //    e = 0.5f; o = 0.3f; it = 0.2f; sum = e + o + it;
        //}
        //e /= sum; o /= sum; it /= sum;

        //int obstaclesThisRow = 0;
        //bool undashableSpawnedThisRow = false;

        //for (int i = 0; i < laneYs.Length; i++)
        //{
        //    float laneY = laneYs[i];
        //    float r = Random.value; // 0..1

        //    if (r < e)
        //    {
        //        // empty
        //        continue;
        //    }
        //    else if (r < e + o)
        //    {
        //        // request obstacle, but check row-level rules
        //        if (obstaclesThisRow >= maxObstaclesPerRow)
        //        {
        //            // downgrade to item if possible
        //            TrySpawnItemOrEmpty(x, laneY);
        //        }
        //        else
        //        {
        //            // pick obstacle prefab
        //            GameObject chosen = ChooseObstaclePrefab();
        //            if (chosen == null)
        //            {
        //                // fallback to item if no obstacle prefabs
        //                TrySpawnItemOrEmpty(x, laneY);
        //            }
        //            else
        //            {
        //                // if chosen is undashable and we've already spawned an undashable -> downgrade
        //                bool isUndashable = chosen.CompareTag("Undashable");
        //                if (isUndashable && undashableSpawnedThisRow)
        //                {
        //                    // đã có 1 undashable rồi → lane này để trống
        //                    continue;
        //                }

        //                else
        //                {
        //                    GameObject go = SpawnFromPoolOrInstantiate(chosen, new Vector3(x, laneY, 0f));
        //                    activeSpawns.Add(go);
        //                    obstaclesThisRow++;
        //                    if (isUndashable) undashableSpawnedThisRow = true;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // item
        //        TrySpawnItemOrEmpty(x, laneY);
        //    }
        //}
        // normalize ratios (tránh trường hợp người dev set sai)
        float e = emptyRatio;
        float o = obstacleRatio;
        float it = itemRatio;
        float sum = e + o + it;
        if (sum <= 0f)
        {
            // fallback an toàn
            e = 0.5f; o = 0.3f; it = 0.2f; sum = e + o + it;
        }
        e /= sum; o /= sum; it /= sum;

        int obstaclesThisRow = 0;
        bool undashableSpawnedThisRow = false;

        for (int i = 0; i < laneYs.Length; i++)
        {
            float laneY = laneYs[i];
            float r = Random.value; // 0..1

            if (r < e)
            {
                // empty
                continue;
            }
            else if (r < e + o)
            {
                // request obstacle, but check row-level rules
                if (obstaclesThisRow >= maxObstaclesPerRow)
                {
                    // ĐÃ ĐỦ obstacle trong row → không spawn thêm, để lane này trống
                    continue;
                }

                // pick obstacle prefab
                GameObject chosen = ChooseObstaclePrefab();
                if (chosen == null)
                {
                    // không có obstacle nào → để trống
                    continue;
                }

                bool isUndashable = chosen.CompareTag("Undashable");
                if (isUndashable && undashableSpawnedThisRow)
                {
                    // đã có undashable trong row này → không spawn thêm undashable
                    continue;
                }

                GameObject go = SpawnFromPoolOrInstantiate(chosen, new Vector3(x, laneY, 0f));
                activeSpawns.Add(go);
                obstaclesThisRow++;
                if (isUndashable) undashableSpawnedThisRow = true;
            }
            else
            {
                // item
                TrySpawnItemOrEmpty(x, laneY);
            }
        }
    }

    // helper: try spawn item, if no item prefab available -> leave empty
    void TrySpawnItemOrEmpty(float x, float y)
    {
        GameObject itemPrefab = ChooseItemPrefab();
        if (itemPrefab != null)
        {
            GameObject go = SpawnFromPoolOrInstantiate(itemPrefab, new Vector3(x, y, 0f));
            activeSpawns.Add(go);
        }
        // else remain empty
    }

    // Chooses an obstacle prefab (random from hard/soft based on hardObstacleRatio)
    GameObject ChooseObstaclePrefab()
    {
        // choose hard vs soft first
        float rr = Random.value;
        if (rr < hardObstacleRatio && hardObstacles != null && hardObstacles.Length > 0)
        {
            return hardObstacles[Random.Range(0, hardObstacles.Length)];
        }
        else if (softObstacles != null && softObstacles.Length > 0)
        {
            return softObstacles[Random.Range(0, softObstacles.Length)];
        }
        return null;
    }

    // Chooses an item prefab based on your item distribution
    GameObject ChooseItemPrefab()
    {
        float r = Random.value;

        // 0.1333 ~ 13.33%
        if (r < 0.1333f) return prefabBanhMi;
        else if (r < 0.2666f) return prefabCafe;
        else if (r < 0.3999f) return prefabMagnet;

        // 0.20 each cho 3 loại còn lại
        else if (r < 0.5999f) return prefabBoost;
        else if (r < 0.7999f) return prefabMystery;
        else return prefabThuocLao;
    }


    void SpawnObstacle(float x, float y)
    {
        // legacy: left for compatibility (calls through ChooseObstaclePrefab)
        GameObject prefab = ChooseObstaclePrefab();
        if (prefab == null) return;
        GameObject go = SpawnFromPoolOrInstantiate(prefab, new Vector3(x, y, 0f));
        activeSpawns.Add(go);
    }

    void SpawnItem(float x, float y)
    {
        // legacy: left for compatibility
        GameObject prefab = ChooseItemPrefab();
        if (prefab == null) return;
        GameObject go = SpawnFromPoolOrInstantiate(prefab, new Vector3(x, y, 0f));
        activeSpawns.Add(go);
    }

    #region Pooling helpers
    void AddPoolForPrefabArray(GameObject[] arr)
    {
        if (arr == null) return;
        foreach (var p in arr) AddPool(p);
    }

    void AddPool(GameObject prefab)
    {
        if (!usePooling || prefab == null) return;
        if (pool.ContainsKey(prefab)) return;

        pool[prefab] = new Queue<GameObject>();
        for (int i = 0; i < initialPoolPerPrefab; i++)
        {
            // instantiate ở vị trí zero (không dùng 9999) để tránh conflict với scripts đọc vị trí lúc Awake
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, spawnRoot);
            obj.SetActive(false);
            pool[prefab].Enqueue(obj);
        }
    }

    GameObject SpawnFromPoolOrInstantiate(GameObject prefab, Vector3 pos)
    {
        GameObject go = null;
        if (usePooling && prefab != null && pool.ContainsKey(prefab) && pool[prefab].Count > 0)
        {
            go = pool[prefab].Dequeue();
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;
            go.SetActive(true);
        }
        else
        {
            go = Instantiate(prefab, pos, Quaternion.identity, spawnRoot);
        }

        // update bob start pos if component exists (both pool & new instances)
        var bob = go.GetComponent<ItemBobWithPhaseAndDisable>();
        if (bob != null) bob.ResetStartLocalPos();

        // If spawned object has Rigidbody2D we may want to reset its velocity etc. (optional)
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        return go;
    }

    void ReturnToPool(GameObject go)
    {
        if (go == null) return;
        // find prefab key by compare name or component - best if your prefabs have a marker script to identify original prefab.
        // Here we assume the prefab reference exists in pool keys and that the instance's name starts with prefab name
        go.SetActive(false);
        foreach (var kv in pool)
        {
            if (go.name.StartsWith(kv.Key.name))
            {
                kv.Value.Enqueue(go);
                return;
            }
        }

        // if not found in pool keys, destroy
        Destroy(go);
    }
    #endregion

    void CleanupBehindPlayer()
    {
        for (int i = activeSpawns.Count - 1; i >= 0; i--)
        {
            GameObject go = activeSpawns[i];
            if (go == null)
            {
                activeSpawns.RemoveAt(i);
                continue;
            }

            if (player.position.x - go.transform.position.x > despawnDistance)
            {
                activeSpawns.RemoveAt(i);
                if (usePooling) ReturnToPool(go);
                else Destroy(go);
            }
        }
    }

    // Public helper: force clear all spawned objects (useful when restarting level or when map loops and you want fresh spawn)
    public void ClearAllSpawns()
    {
        for (int i = activeSpawns.Count - 1; i >= 0; i--)
        {
            if (activeSpawns[i] != null)
            {
                if (usePooling) ReturnToPool(activeSpawns[i]);
                else Destroy(activeSpawns[i]);
            }
        }
        activeSpawns.Clear();
        // reset accumulator so spawn pattern restarts predictable
        distanceAccumulator = 0f;
    }

    // --- DEBUG HELPERS (Inspector context menu)
#if UNITY_EDITOR
    [ContextMenu("SpawnOneOfEachForTest")]
    public void SpawnOneOfEachForTest()
    {
        if (player == null)
        {
            Debug.LogWarning("[SpawnTest] player is null.");
            return;
        }

        float x = player.position.x + 5f;
        float[] ys = laneYs.Length > 0 ? laneYs : new float[] { 0f };
        GameObject[] testPrefabs = new GameObject[] { prefabBanhMi, prefabCafe, prefabMagnet, prefabBoost, prefabMystery, prefabThuocLao };
        int row = 0;
        foreach (var p in testPrefabs)
        {
            if (p == null) { Debug.LogWarning("[SpawnTest] prefab null in inspector."); continue; }
            float y = ys[Mathf.Min(row, ys.Length - 1)];
            GameObject go = SpawnFromPoolOrInstantiate(p, new Vector3(x, y, 0f));
            activeSpawns.Add(go);
            Debug.Log("[SpawnTest] Spawned " + p.name + " at " + go.transform.position + " active? " + go.activeSelf);
            x += 1.5f;
            row++;
        }
    }
#endif
}
