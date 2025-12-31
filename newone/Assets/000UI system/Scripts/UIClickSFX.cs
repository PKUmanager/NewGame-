using UnityEngine;
using UnityEngine.UI;

public class UIClickSFX : MonoBehaviour
{
    [Header("Click Sound")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // 给场景里所有 Button 绑定点击音效（包括未激活的）
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var btn in buttons)
        {
            btn.onClick.AddListener(PlayClick);
        }
    }

    public void PlayClick()
    {
        if (clickClip == null) return;
        audioSource.PlayOneShot(clickClip, volume);
    }
}