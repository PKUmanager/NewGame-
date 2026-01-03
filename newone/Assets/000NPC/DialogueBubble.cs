using UnityEngine;
using UnityEngine.UI; // 如果用旧版Text
using TMPro;          // 如果用TextMeshPro (推荐)

public class DialogueBubble : MonoBehaviour
{
    [Header("组件引用")]
    public GameObject rootCanvas;    // 整个气泡的根物体(用来控制开关)
    public TextMeshProUGUI contentText; // 显示文字的组件
    public Image emojiImage;         // 显示表情的组件

    [Header("配置")]
    public float displayDuration = 2.0f; // 气泡显示几秒后自动消失

    private void Start()
    {
        // 游戏开始时先隐藏气泡
        HideBubble();
    }

    // 显示文字的方法
    public void ShowText(string text)
    {
        rootCanvas.SetActive(true);      // 打开气泡
        emojiImage.gameObject.SetActive(false); // 关掉表情图
        contentText.gameObject.SetActive(true); // 打开文字

        contentText.text = text;         // 设置文字内容

        // 重新计时关闭
        CancelInvoke("HideBubble");
        Invoke("HideBubble", displayDuration);
    }

    // 显示表情的方法
    public void ShowEmoji(Sprite emojiSprite)
    {
        rootCanvas.SetActive(true);
        contentText.gameObject.SetActive(false); // 关掉文字
        emojiImage.gameObject.SetActive(true);   // 打开表情图

        emojiImage.sprite = emojiSprite;         // 设置表情图片

        CancelInvoke("HideBubble");
        Invoke("HideBubble", displayDuration);
    }

    // 隐藏气泡
    private void HideBubble()
    {
        rootCanvas.SetActive(false);
    }

    // 2.5D 特殊处理：让气泡始终朝向摄像机
    // 如果你的场景是可以旋转视角的，这个很重要；如果是固定视角，可以不需要。
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}