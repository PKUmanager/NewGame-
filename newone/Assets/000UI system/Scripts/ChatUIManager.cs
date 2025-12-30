using UnityEngine;
using UnityEngine.UI;
using TMPro; // 如果使用 TextMeshPro，请保留引用

public class DialogueUIManager : MonoBehaviour
{
    [Header("总容器")]
    public GameObject dialogueContainer; // 整个对话界面的父物体

    [Header("提问面板 (Question Panel)")]
    public GameObject questionPanel;     // 提问面板对象
    public TMP_InputField inputField;    // 输入框
    public Button sendButton;            // 发送按钮

    [Header("回答面板 (Answer Panel)")]
    public GameObject answerPanel;       // 回答面板对象
    public TMP_Text answerText;          // 显示回答的文本
    public Button closeButton;           // 关闭按钮

    private void Start()
    {
        // 游戏开始时，确保整个对话框是隐藏的
        dialogueContainer.SetActive(false);

        // 绑定按钮事件
        sendButton.onClick.AddListener(SubmitQuestion);
        closeButton.onClick.AddListener(CloseDialogue);
    }

    // --- 1. 点击 NPC 时调用此方法 ---
    public void OpenDialogue()
    {
        dialogueContainer.SetActive(true);

        // 初始状态：显示提问面板，隐藏回答面板
        questionPanel.SetActive(true);
        answerPanel.SetActive(false);

        // 清空之前的输入
        inputField.text = "";
    }

    // --- 2. 点击发送按钮的逻辑 ---
    public void SubmitQuestion()
    {
        string userQuestion = inputField.text;

        if (string.IsNullOrEmpty(userQuestion)) return; // 防止空输入

        // 切换 UI 状态：隐藏提问，显示回答
        questionPanel.SetActive(false);
        answerPanel.SetActive(true);

        // 先显示“正在思考”
        answerText.text = "正在连接大脑...";

        // TODO: 这里即使是你接入 API 的地方
        // 举例：StartCoroutine(CallAI(userQuestion));

        // 暂时模拟一个回答（测试用）
        DisplayAnswer("这是测试回答：我收到了你的问题——" + userQuestion);
    }

    // --- 3. 供外部（API回调）调用的显示方法 ---
    public void DisplayAnswer(string text)
    {
        // 确保回答面板是开着的
        questionPanel.SetActive(false);
        answerPanel.SetActive(true);

        // 更新文本
        answerText.text = text;
    }

    // --- 4. 关闭对话框 ---
    public void CloseDialogue()
    {
        dialogueContainer.SetActive(false);
    }
}