using UnityEngine;
using UnityEngine.UI;

public class NPCQuestionUIController : MonoBehaviour
{
    [Header("面板控制")]
    [Tooltip("点击这个按钮来切换面板的显示/隐藏")]
    public Button npcImageButton;

    [Tooltip("问题面板 (一直显示)")]
    public GameObject questionPanel;

    [Tooltip("答案面板 (一直显示)")]
    public GameObject answerPanel;

    [Tooltip("全屏透明背景 (点击空白处关闭，如果不需要这个功能可以不赋值)")]
    public Button backgroundMaskButton;

    [Header("交互组件 (Legacy UI)")]
    public InputField inputField;  // 输入框
    public Button sendButton;      // 发送按钮
    public Text answerText;        // 显示回答的文本

    [Header("AI 逻辑连接")]
    public DeepSeekDialogueManager aiManager;

    private bool isPanelShow = false;

    void Start()
    {
        HideAllPanels(); // 游戏开始时先隐藏
        BindButtonEvents();
    }

    private void BindButtonEvents()
    {
        // 1. NPC 按钮点击：切换显示/隐藏
        if (npcImageButton != null)
            npcImageButton.onClick.AddListener(ToggleQuestionAnswerPanels);

        // 2. 背景点击：关闭面板 (如果不想要点击空白处关闭，可以在 Inspector 里把这个变量留空)
        if (backgroundMaskButton != null)
            backgroundMaskButton.onClick.AddListener(HideAllPanels);

        // 3. 发送按钮点击：只发送，不关面板
        if (sendButton != null)
            sendButton.onClick.AddListener(SubmitQuestion);

        // 4. 回车键绑定
        if (inputField != null)
            inputField.onSubmit.AddListener((value) => SubmitQuestion());
    }

    /// <summary>
    /// 核心逻辑：提交问题
    /// </summary>
    public void SubmitQuestion()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text)) return;
        if (aiManager == null)
        {
            Debug.LogError("请绑定 DeepSeekDialogueManager！");
            return;
        }

        string question = inputField.text;

        // =========== 【核心修改点 1】 ===========
        // 发送时，不再去操作 SetActive(false/true)
        // 保持面板状态原封不动，只更新文字
        // ======================================

        // 清空输入框，为了用户体验
        inputField.text = "";

        // 更新回答面板的状态文字
        if (answerText != null) answerText.text = "兔脑飞速运转中...";

        // 调用 AI
        aiManager.SendDialogueRequest(question, (response, success) =>
        {
            if (answerText != null)
            {
                answerText.text = success ? response : "网络连接失败。";
            }
        });

        // 强行把焦点还给输入框，方便用户连续提问 (可选)
        inputField.ActivateInputField();
    }

    public void ToggleQuestionAnswerPanels()
    {
        if (isPanelShow) HideAllPanels();
        else ShowQuestionAnswerPanels();
    }

    /// <summary>
    /// 显示所有面板
    /// </summary>
    public void ShowQuestionAnswerPanels()
    {
        // =========== 【核心修改点 2】 ===========
        // 打开时，强制两个面板都显示
        // ======================================
        if (questionPanel != null) questionPanel.SetActive(true);
        if (answerPanel != null) answerPanel.SetActive(true);

        if (backgroundMaskButton != null) backgroundMaskButton.gameObject.SetActive(true);

        isPanelShow = true;
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    public void HideAllPanels()
    {
        if (questionPanel != null) questionPanel.SetActive(false);
        if (answerPanel != null) answerPanel.SetActive(false);

        if (backgroundMaskButton != null) backgroundMaskButton.gameObject.SetActive(false);

        isPanelShow = false;
    }
}