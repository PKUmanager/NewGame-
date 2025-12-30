using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsWindowUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam = "SFXVolume";

    [Header("UI")]
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderSfx;
    [SerializeField] private Dropdown dropdownQuality;
    [SerializeField] private Button btnClose;

    private const string KEY_MUSIC = "SET_MUSIC";
    private const string KEY_SFX = "SET_SFX";
    private const string KEY_QUALITY = "SET_QUALITY";

    private bool isInit;

    private void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(() => gameObject.SetActive(false));

        if (sliderMusic != null) sliderMusic.onValueChanged.AddListener(OnMusicChanged);
        if (sliderSfx != null) sliderSfx.onValueChanged.AddListener(OnSfxChanged);
        if (dropdownQuality != null) dropdownQuality.onValueChanged.AddListener(OnQualityChanged);
    }

    private void OnEnable()
    {
        InitUIFromSaved();
    }

    private void InitUIFromSaved()
    {
        isInit = false;

        float music = PlayerPrefs.GetFloat(KEY_MUSIC, 0.8f);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, 0.8f);

        int qDefault = QualitySettings.GetQualityLevel();
        int quality = PlayerPrefs.GetInt(KEY_QUALITY, qDefault);

        if (sliderMusic != null) sliderMusic.value = music;
        if (sliderSfx != null) sliderSfx.value = sfx;

        SetupQualityDropdown();
        if (dropdownQuality != null)
        {
            quality = Mathf.Clamp(quality, 0, dropdownQuality.options.Count - 1);
            dropdownQuality.value = quality;
            dropdownQuality.RefreshShownValue();
        }

        ApplyMusic(music);
        ApplySfx(sfx);
        ApplyQuality(quality);

        isInit = true;
    }

    private void SetupQualityDropdown()
    {
        if (dropdownQuality == null) return;
        dropdownQuality.ClearOptions();
        dropdownQuality.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
    }

    private void OnMusicChanged(float v)
    {
        if (!isInit) return;
        ApplyMusic(v);
        PlayerPrefs.SetFloat(KEY_MUSIC, v);
        PlayerPrefs.Save();
    }

    private void OnSfxChanged(float v)
    {
        if (!isInit) return;
        ApplySfx(v);
        PlayerPrefs.SetFloat(KEY_SFX, v);
        PlayerPrefs.Save();
    }

    private void OnQualityChanged(int index)
    {
        if (!isInit) return;
        ApplyQuality(index);
        PlayerPrefs.SetInt(KEY_QUALITY, index);
        PlayerPrefs.Save();
    }

    private void ApplyQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    private void ApplyMusic(float sliderValue)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat(musicParam, SliderToDb(sliderValue));
    }

    private void ApplySfx(float sliderValue)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat(sfxParam, SliderToDb(sliderValue));
    }

    private float SliderToDb(float v)
    {
        if (v <= 0.0001f) return -80f; // ¾²Òô
        return Mathf.Log10(v) * 20f;   // 0~1 Ó³Éäµ½ dB
    }
}