using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private DeepSeekDialogueManager dialogueManager;  // 对话管理器
    [SerializeField] private InputField inputField;  // 玩家输入框
    [SerializeField] private Text dialogueText;  // 对话显示文本

    [Header("参数设置")]
    [SerializeField] private float typingSpeed = 0.05f;  // 打字效果速度

    private string characterName;

    void Start()
    {
        characterName = dialogueManager.npcCharacter.name;

        // 注册输入回调
        inputField.onSubmit.AddListener((text) =>
        {
            dialogueManager.SendDialogueRequest(text, HandleAIResponse);
        });
    }

    // 处理AI回复
    private void HandleAIResponse(string response, bool success)
    {
        string displayText = success ? $"{characterName}:{response}" : $"{characterName}:（通讯中断）";
        StartCoroutine(TypewriterEffect(displayText));
    }

    // 打字机效果
    private IEnumerator TypewriterEffect(string text)
    {
        string currentText = "";
        foreach (char c in text)
        {
            currentText += c;
            dialogueText.text = currentText;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}