using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NPCDialog : MonoBehaviour
{
    [Header("UI设置")]
    public GameObject dialogCanvasPrefab; // 拖入刚才做的气泡Prefab
    public float dialogHeight = 2.5f;     // 气泡在头顶的高度

    [Header("说话内容库 (手动配置)")]
    public List<DialogData> dialogList;   // 在 Inspector 里填内容

    // 定义一个结构体，用来存 文字 或 图片
    [System.Serializable]
    public struct DialogData
    {
        [TextArea] public string text; // 文字内容
        public Sprite emoji;           // 表情图片 (如果不填就是纯文字)
    }

    // 内部变量
    private GameObject currentBubble;
    private TMP_Text uiText;
    private Image uiImage;
    private Animator animator; // 用来判断是否停下
    private bool isShowing = false;
    private Image bubbleBgImage; // ★★★ 【新增】 用来控制背景图 ★★★
    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        // 1. 生成气泡 (一开始隐藏)
        if (dialogCanvasPrefab)
        {
            currentBubble = Instantiate(dialogCanvasPrefab, transform);
            currentBubble.transform.localPosition = new Vector3(0, dialogHeight, 0);

            // 找到子组件
            uiText = currentBubble.GetComponentInChildren<TMP_Text>();
            // 注意：这里需要你根据实际层级找 Image，或者在 Prefab 里挂个脚本引用
            // 简单粗暴法：找名字叫 ContentImage 的
            Transform imgTrans = currentBubble.transform.Find("BubbleBg/ContentImage");
            if (imgTrans) uiImage = imgTrans.GetComponent<Image>();

            // ★★★ 【新增 1】 找到背景图组件 ★★★
            // 假设你在 Prefab 里的背景物体名字叫 "BubbleBg"
            // =========================================================
            Transform bgTrans = currentBubble.transform.Find("BubbleBg");
            if (bgTrans != null)
            {
                bubbleBgImage = bgTrans.GetComponent<Image>();
            }
            // 挂一个 Billboard 脚本让它永远对着相机
            if (currentBubble.GetComponent<FaceMyCamera>() == null)
                currentBubble.AddComponent<FaceMyCamera>(); // 或者是 FaceMyCamera

            currentBubble.SetActive(false);
        }
    }

    void Update()
    {
        if (animator == null) return;

        // 2. 检测状态
        // 假设你的 Animator 里有个 bool 叫 "IsMoving"
        bool moving = animator.GetBool("IsMoving");

        if (!moving && !isShowing)
        {
            // 停下了，且当前没显示 -> 开始说话
            StartCoroutine(ShowDialogRoutine());
        }
        else if (moving && isShowing)
        {
            // 开始走了 -> 立刻关闭气泡
            CloseDialog();
        }
    }

    IEnumerator ShowDialogRoutine()
    {
        isShowing = true;

        // 稍微等一下再弹，别刚停就弹，太假
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        // 如果这时候又走了，就不弹了
        if (animator.GetBool("IsMoving"))
        {
            isShowing = false;
            yield break;
        }

        // 随机选一条内容
        if (dialogList.Count > 0)
        {
            DialogData data = dialogList[Random.Range(0, dialogList.Count)];
            UpdateUI(data);
            currentBubble.SetActive(true);
        }
    }

    void UpdateUI(DialogData data)
    {
        if (data.emoji != null)
        {
            // 显示图片模式
            if (uiText) uiText.gameObject.SetActive(false);
            if (bubbleBgImage) bubbleBgImage.enabled = false;

            if (uiImage)
            {
                uiImage.gameObject.SetActive(true);
                uiImage.sprite = data.emoji;
                // uiImage.SetNativeSize(); // 保持图片比例
            }
        }
        else
        {
            // 显示文字模式
            if (uiImage) uiImage.gameObject.SetActive(false);
            if (bubbleBgImage) bubbleBgImage.enabled = true;
            if (uiText)
            {
                uiText.gameObject.SetActive(true);
                uiText.text = data.text;
            }
        }
    }

    void CloseDialog()
    {
        if (currentBubble) currentBubble.SetActive(false);
        isShowing = false;
        StopAllCoroutines();
    }
}