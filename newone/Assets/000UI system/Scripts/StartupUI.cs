using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StartupUI : MonoBehaviour
{
    [Header("UI")]
    public Slider loadingSlider;
    public TMP_Text loadingText;      // 如果你用的是 Text (Legacy)，就改成 Text
    public Button startButton;

    [Header("Target Scene Name")]
    public string targetSceneName = "SampleScene";

    private bool isLoading = false;

    void Start()
    {
        if (loadingSlider != null) loadingSlider.value = 0f;
        if (loadingText != null) loadingText.text = "";

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartGame);
        }
    }

    void OnStartGame()
    {
        if (isLoading) return;
        isLoading = true;

        // 防止重复点击
        if (startButton != null) startButton.interactable = false;

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // 开始真正加载
        AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
        op.allowSceneActivation = false; // 先不立刻切，等进度条到 100%

        while (!op.isDone)
        {
            // op.progress 范围：0 ~ 0.9（到 0.9 代表“加载完成，只差激活”）
            float raw = op.progress;
            float progress01 = Mathf.Clamp01(raw / 0.9f); // 映射成 0~1

            if (loadingSlider != null) loadingSlider.value = progress01;

            if (loadingText != null)
            {
                int percent = Mathf.RoundToInt(progress01 * 100f);
                loadingText.text = $"Loading... {percent}%";
            }

            // 到 100% 再切
            if (progress01 >= 1f)
            {
                if (loadingText != null) loadingText.text = "Loading... 100%";
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}