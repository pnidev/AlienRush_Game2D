using UnityEngine;

public class BGLooper : MonoBehaviour
{
    public Transform cameraTransform;
    private float spriteWidth;

    void Start()
    {
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        Debug.Log("Sprite width: " + spriteWidth);
    }


    void Update()
    {
        // Nếu Bg1 nằm phía sau camera quá xa → dịch sang trước
        if (cameraTransform.position.x - transform.position.x >= spriteWidth)
        {
            transform.position += new Vector3(spriteWidth * 2, 0, 0);
        }
    }

}
