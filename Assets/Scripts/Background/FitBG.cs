using UnityEngine;

public class FitBG : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Kích thước BG hiện tại (đã bị chia bởi PPU)
        float bgWidth = sr.bounds.size.x;
        float bgHeight = sr.bounds.size.y;

        // Kích thước camera
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        Vector3 scale = transform.localScale;

        // Scale theo chiều rộng (ngang)
        if (bgWidth < camWidth)
        {
            scale.x *= camWidth / bgWidth;
        }

        transform.localScale = scale;
    }
}
