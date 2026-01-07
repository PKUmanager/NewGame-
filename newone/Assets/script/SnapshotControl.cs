using UnityEngine;

public class SnapshotControl : MonoBehaviour
{
    // 把你的 Btn_Snapshot 拖进这里
    public GameObject snapshotButton;

    void Start()
    {
        // 游戏一开始，强制隐藏截图按钮
        if (snapshotButton != null)
            snapshotButton.SetActive(false);
    }

    // === 绑在【进入预览】按钮上 ===
    public void ShowButton()
    {
        if (snapshotButton != null)
            snapshotButton.SetActive(true);
    }

    // === 绑在【退出/取消预览】按钮上 ===
    public void HideButton()
    {
        if (snapshotButton != null)
            snapshotButton.SetActive(false);
    }
}