using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    [Header("Duration Settings")]
    public float invertDuration = 2f;
    public float blindDuration = 2.5f;
    public float dashLockDuration = 3f;
    public float speedBoostDuration = 5f;
    public float magnetDuration = 8f;

    [Header("Intensity")]
    public float speedMultiplier = 1.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the player
        PlayerMovement2D player = collision.GetComponent<PlayerMovement2D>();

        if (player != null)
        {
            OpenBox(player);
            Destroy(gameObject); // Destroy the mystery box after opening
        }
    }

    void OpenBox(PlayerMovement2D player)
    {
        int chance = Random.Range(0, 100); // Random number between 0 and 99


        if (chance < 25)
        {
            // 0-24: Magnet (25%)
            player.ApplyMagnet(magnetDuration);
            Debug.Log("dang ap dung Magnet");
        }
        else if (chance < 50)
        {
            // 25-49: Speed Boost (25%)
            player.ApplySpeedBoost(speedBoostDuration, speedMultiplier);
            Debug.Log("dang ap dung Speed Boost");
        }
        else if (chance < 70)
        {
            // 50-69: Invert Controls (20%)
            player.ApplyInvertControls(invertDuration);
            Debug.Log("dang bi doi dieu khien");
        }
        else if (chance < 90)
        {
            // 70-89: Dash Lock (20%)
            player.ApplyDashLock(dashLockDuration);
            Debug.Log("dang bi khoa dash");
        }
        else
        {
            // 90-99: Blind (10%)
            player.ApplyBlindness(blindDuration);
            Debug.Log("dang bi che mat");
        }
    }
}