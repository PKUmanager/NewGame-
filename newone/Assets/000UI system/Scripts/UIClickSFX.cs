using UnityEngine;
using UnityEngine.UI;

public class UIClickSFX : MonoBehaviour
{
    private const string PREF_UI_SFX_VOLUME = "UI_SFX_VOLUME";

    [Header("Click Sound")]
    [SerializeField] private AudioClip clickClip;

    [SerializeField, Range(0f, 1f)]
    private float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // ✅ 启动时读上一次的音量（默认1）
        volume = PlayerPrefs.GetFloat(PREF_UI_SFX_VOLUME, volume);

        // ✅ 把音量同步到 AudioSource（虽然 PlayOneShot 也会用 volume，但保持一致更好）
        audioSource.volume = volume;

        // ✅ 给场景里所有 Button 绑点击音效（包含未激活）
        BindAllButtonsInScene();
    }

    private void OnEnable()
    {
        // 如果你这个对象会被关闭再打开，可以再绑一次，避免漏绑
        BindAllButtonsInScene();
    }

    private void BindAllButtonsInScene()
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var btn in buttons)
        {
            btn.onClick.RemoveListener(PlayClick); // 避免重复绑定
            btn.onClick.AddListener(PlayClick);
        }
    }

    // ✅ 给 Slider 调用的：设置音量 + 存档
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (audioSource != null) audioSource.volume = volume;
        PlayerPrefs.SetFloat(PREF_UI_SFX_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }

    public void PlayClick()
    {
        if (clickClip == null) return;
        if (audioSource == null) return;

        // ✅ 用实时 volume
        audioSource.PlayOneShot(clickClip, volume);
    }
}