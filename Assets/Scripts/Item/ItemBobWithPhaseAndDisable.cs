using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemBobWithPhaseAndDisable : MonoBehaviour
{
    public float amplitude = 0.25f;
    public float speed = 2f;
    [Range(0f, Mathf.PI * 2f)]
    public float phase = 0f;
    public bool randomizePhase = true;

    private Vector3 startLocalPos;
    private SpriteRenderer sr;
    private bool isActive = true;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // không set startLocalPos ở Awake vì object có thể đang ở vị trí pool trước khi spawn
    }

    void OnEnable()
    {
        // khi object active (khi instantiate hoặc lấy từ pool), cập nhật vị trí gốc đúng tại thời điểm active
        startLocalPos = transform.localPosition;
        if (randomizePhase) phase = Random.Range(0f, Mathf.PI * 2f);
        isActive = true;
    }

    /// <summary>
    /// Gọi ngay sau khi set vị trí và SetActive(true) (thường do spawner)
    /// để chắc chắn startLocalPos được cập nhật chính xác.
    /// </summary>
    public void ResetStartLocalPos()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (!isActive) return;

        // Optional: đơn giản culling - animate only if sprite visible
        if (!sr.isVisible) return;

        float yOffset = Mathf.Sin((Time.time * speed) + phase) * amplitude;
        transform.localPosition = new Vector3(startLocalPos.x, startLocalPos.y + yOffset, startLocalPos.z);
    }

    public void StopBobbingAndReset()
    {
        isActive = false;
        transform.localPosition = startLocalPos;
    }

    public void StartBobbing()
    {
        isActive = true;
    }
}
