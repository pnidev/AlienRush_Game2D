using UnityEngine;

public class PlaceNextTo : MonoBehaviour
{
    public Transform leftBG; // BG1

    void Start()
    {
        float width = leftBG.GetComponent<SpriteRenderer>().bounds.size.x;
        transform.position = new Vector3(leftBG.position.x + width, leftBG.position.y, leftBG.position.z);
    }
}
