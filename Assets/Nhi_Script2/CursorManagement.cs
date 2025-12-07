using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManagement : MonoBehaviour
{
   
    [SerializeField] private Texture2D cursorNormal;
    [SerializeField] private Texture2D cursorShoot;
    [SerializeField] private Texture2D cursorReload;

    // Điểm đặt tâm của con trỏ (tùy theo texture bạn)
    private Vector2 hotspot = new Vector2(16, 48);

    void Start()
    {
        // Đặt con trỏ mặc định lúc bắt đầu
        Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
    }

    void Update()
    {
        HandleCursorState();
    }

    private void HandleCursorState()
    {
        // Click chuột trái → đổi sang cursorShoot
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(cursorShoot, hotspot, CursorMode.Auto);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
        }

        // Click chuột phải → đổi sang cursorReload
        if (Input.GetKeyDown(KeyCode.R))
        {
            Cursor.SetCursor(cursorReload, hotspot, CursorMode.Auto);
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
        }
    }
}
