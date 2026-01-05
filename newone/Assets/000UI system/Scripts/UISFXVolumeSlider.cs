using UnityEngine;
using UnityEngine.UI;

public class UISFXVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private UIClickSFX uiClickSfx;

    private void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();

        // 如果你没手动拖引用，就自动找
        if (uiClickSfx == null) uiClickSfx = FindObjectOfType<UIClickSFX>(true);

        if (slider == null || uiClickSfx == null) return;

        // 1) 打开面板时：把 Slider 位置设成当前音量
        slider.SetValueWithoutNotify(uiClickSfx.GetVolume());

        // 2) 拖动时：实时改音量
        slider.onValueChanged.AddListener(uiClickSfx.SetVolume);
    }

    private void OnDestroy()
    {
        if (slider != null && uiClickSfx != null)
            slider.onValueChanged.RemoveListener(uiClickSfx.SetVolume);
    }
}