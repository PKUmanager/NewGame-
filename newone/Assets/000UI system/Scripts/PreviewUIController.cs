using UnityEngine;

public class PreviewUIController : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] private GameObject shopAndRemoveUI;   // Shop & Remove
    [SerializeField] private GameObject buildUIRoot;        // 建造主UI
    [SerializeField] private GameObject functionButtons;    // FunctionButtons

    [Header("Build Buttons")]
    [SerializeField] private GameObject btnStartBuild;     // 开始建造
    [SerializeField] private GameObject btnExitBuild;      // 退出建造

    // ===== 进入预览 =====
    public void EnterPreview()
    {
        if (shopAndRemoveUI != null)
            shopAndRemoveUI.SetActive(false);

        if (buildUIRoot != null)
            buildUIRoot.SetActive(false);

        // 🔴 关键：隐藏建造相关按钮
        if (btnStartBuild != null)
            btnStartBuild.SetActive(false);

        if (btnExitBuild != null)
            btnExitBuild.SetActive(false);

        // ✅ 你要“预览时隐藏 FunctionButtons”
        if (functionButtons != null)
            functionButtons.SetActive(false);
    }

    // ===== 取消预览 =====
    public void ExitPreview()
    {
        if (shopAndRemoveUI != null)
            shopAndRemoveUI.SetActive(true);

        if (buildUIRoot != null)
            buildUIRoot.SetActive(true);

        // 🟢 恢复建造按钮
        if (btnStartBuild != null)
            btnStartBuild.SetActive(true);

        if (btnExitBuild != null)
            btnExitBuild.SetActive(true);

        // ✅ 退出预览后恢复 FunctionButtons 显示（按你需求：取消预览时需要关闭/打开？）
        // 你之前说“取消预览的时候需要把functionbutton关闭”，那这里就保持 false；
        // 如果你希望取消预览后显示功能按钮，把 false 改成 true。
        if (functionButtons != null)
            functionButtons.SetActive(false);
    }
}