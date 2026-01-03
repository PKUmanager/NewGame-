using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StartupUI : MonoBehaviour
{
    [Header("UI")]
    public Slider loadingSlider;
    public TMP_Text loadingText;
    public Button startButton;

    [Header("Target Scene Name")]
    public string targetSceneName = "SampleScene";

    private bool isLoading = false;
    private bool isLoggedIn = false;   // ✅ 新增：是否已登录

    void Start()
    {
        if (loadingSlider != null) loadingSlider.value = 0f;
        if (loadingText != null) loadingText.text = "";

        // ✅ 开始按钮默认不可点
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartGame);
            startButton.interactable = false;
        }
    }

    // ✅ AuthManager 登录成功后会调用这个
    public void SetLoggedIn(bool value)
    {
        isLoggedIn = value;
        if (startButton != null)
            startButton.interactable = isLoggedIn && !isLoading;
    }

    void OnStartGame()
    {
        // ✅ 没登录就直接拦住
        if (!isLoggedIn) return;
        if (isLoading) return;

        isLoading = true;
        if (startButton != null) startButton.interactable = false;

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float raw = op.progress;               // 0 ~ 0.9
            float progress01 = Mathf.Clamp01(raw / 0.9f);

            if (loadingSlider != null) loadingSlider.value = progress01;
            if (loadingText != null)
            {
                int percent = Mathf.RoundToInt(progress01 * 100f);
                loadingText.text = $"Loading... {percent}%";
            }

            if (progress01 >= 1f)
            {
                if (loadingText != null) loadingText.text = "Loading... 100%";
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}