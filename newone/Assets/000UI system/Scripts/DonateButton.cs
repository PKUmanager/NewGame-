using UnityEngine;
using UnityEngine.UI;

public class DonatePopup : MonoBehaviour
{
    [Header("整个弹窗面板（包含二维码+关闭按钮）")]
    [SerializeField] private GameObject panel;

    [Header("关闭按钮（可选：也可以在按钮OnClick里直接绑Hide）")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    // 点“点赞赏”按钮调用
    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    // 点“关闭”按钮调用
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}