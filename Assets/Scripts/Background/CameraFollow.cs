using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Nhân vật (player)
    public float smooth = 0.125f;
    public Vector3 offset; // Khoảng cách giữa camera và nhân vật

    void LateUpdate()
    {
        if (!target) return;

        // Tính vị trí mong muốn của camera
        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        // Làm mượt chuyển động camera
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        transform.position = smoothed;
    }
}
