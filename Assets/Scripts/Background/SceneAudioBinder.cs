using UnityEngine;

public class SceneAudioBinder : MonoBehaviour
{
    public AudioClip bgmClip;
    public float fadeTime = 0.4f;

    // >>> ADD THIS STATIC PROPERTY <<<
    public static AudioClip ActiveBGM { get; private set; }

    void Start()
    {
        if (bgmClip != null)
        {
            ActiveBGM = bgmClip; // Lưu clip để revive dùng

            AudioManager.Instance?.PlayBGM(bgmClip, fadeTime);
        }
        else
        {
            ActiveBGM = null;
        }
    }

    private void OnDestroy()
    {
        // chỉ clear nếu chính clip này đang được lưu
        if (ActiveBGM == bgmClip)
            ActiveBGM = null;
    }
}
