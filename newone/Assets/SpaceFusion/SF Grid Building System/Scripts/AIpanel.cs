using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC问答面板控制脚本
/// </summary>
public class NPCQuestionUIController : MonoBehaviour
{
    // 公开引用：在Inspector面板赋值
    [Header("UI元素引用")]
    [Tooltip("NPC图片的Button组件")]
    public Button npcImageButton;
    [Tooltip("问题面板")]
    public GameObject questionPanel;
    [Tooltip("答案面板")]
    public GameObject answerPanel;
    [Tooltip("全屏透明背景（用于接收空白区域点击）")]
    public Button backgroundMaskButton;

    // 私有变量：记录面板状态
    private bool isPanelShow = false;

    void Start()
    {
        // 初始化：隐藏面板，绑定点击事件
        HideAllPanels();
        BindButtonEvents();
    }

    /// <summary>
    /// 绑定所有Button的点击事件
    /// </summary>
    private void BindButtonEvents()
    {
        if (npcImageButton != null)
        {
            npcImageButton.onClick.AddListener(ShowQuestionAnswerPanels);
        }

        if (backgroundMaskButton != null)
        {
            backgroundMaskButton.onClick.AddListener(HideAllPanels);
        }
    }

    /// <summary>
    /// 显示问题和答案面板
    /// </summary>
    public void ShowQuestionAnswerPanels()
    {
        if (questionPanel != null && answerPanel != null && backgroundMaskButton != null)
        {
            questionPanel.SetActive(true);
            answerPanel.SetActive(true);
            backgroundMaskButton.gameObject.SetActive(true); // 显示透明背景，接收空白点击
            isPanelShow = true;
        }
    }

    /// <summary>
    /// 隐藏所有面板（问答面板+透明背景）
    /// </summary>
    public void HideAllPanels()
    {
        if (questionPanel != null && answerPanel != null && backgroundMaskButton != null)
        {
            questionPanel.SetActive(false);
            answerPanel.SetActive(false);
            backgroundMaskButton.gameObject.SetActive(false); // 隐藏透明背景
            isPanelShow = false;
        }
    }
}
