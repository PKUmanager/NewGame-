using UnityEngine;
using UnityEngine.EventSystems; // 必须引用，用于检测是否点击了UI
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    [Header("UI 面板")]
    public GameObject questionPanel; // 提问面板
    public GameObject answerPanel;   // 回答面板

    [Header("设置")]
    public string npcTag = "NPC";    // 只有标签为这名字的物体，点击才会打开UI

    void Start()
    {
        // 游戏开始时隐藏面板
        HideAllPanels();
    }

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 如果点击的是 UI 界面（比如输入框、发送按钮），直接跳过，不做任何处理
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // 2. 发射射线检测点击了什么
            // 注意：如果你是2D游戏（Sprite），用 Check2D(); 
            // 如果你是3D游戏，用 Check3D();
            if (CheckClickNPC())
            {
                OpenPanel();
            }
            else
            {
                // 3. 既没点UI，也没点NPC -> 说明点的是空白区域 -> 关闭面板
                HideAllPanels();
            }
        }
    }

    // 打开面板逻辑
    void OpenPanel()
    {
        // 如果面板已经是打开的，就不重复操作了
        if (questionPanel.activeSelf || answerPanel.activeSelf) return;

        questionPanel.SetActive(true);
        answerPanel.SetActive(false); // 默认先显示提问，隐藏回答
    }

    // 关闭面板逻辑
    void HideAllPanels()
    {
        questionPanel.SetActive(false);
        answerPanel.SetActive(false);
    }

    // ================== 射线检测逻辑 ==================

    // 针对 2D 游戏 (Sprite) 的检测
    bool CheckClickNPC()
    {
        // 将屏幕点击位置转换为世界坐标射线
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            // 检查被点到的物体有没有 NPC 标签
            if (hit.collider.CompareTag(npcTag))
            {
                return true;
            }
        }
        return false;
    }

    /* // 如果你是 3D 游戏，请注释掉上面的 CheckClickNPC，使用下面这个：
    bool CheckClickNPC()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(npcTag))
            {
                return true;
            }
        }
        return false;
    } 
    */
}