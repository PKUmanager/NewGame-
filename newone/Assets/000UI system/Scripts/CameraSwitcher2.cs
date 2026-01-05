using UnityEngine;
// ★★★ 必须引用这个命名空间，才能找到 PlacementSystem
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;

public class CameraSwitcher2 : MonoBehaviour
{
    [Header("Cameras")]
    public Camera buildCamera;
    public Camera previewCamera;

    [Header("Pixel Effect")]
    public PixelEffectManager pixelManager;

    [Header("UI Buttons")]
    public GameObject previewButton;
    public GameObject exitPreviewButton;

    private void Start()
    {
        // 游戏刚开始时，默认进入建造模式。这一行是对的，不用改。
        SwitchToBuild();
    }

    // ★★★ 点击“预览”按钮时执行 ★★★
    public void SwitchToPreview()
    {
        // =========================================================
        // 【核心修复】 切换镜头前，先强制让建造系统“下班”
        // =========================================================
        if (PlacementSystem.Instance != null)
        {
            PlacementSystem.Instance.StopState(); // 这一句就是“停止建造”！
            Debug.Log("进入预览模式：建造状态已强制停止。");
        }

        // 1. 关闭滤镜
        if (pixelManager != null) pixelManager.DisableEffect();

        // 2. 切换相机
        if (buildCamera != null) buildCamera.enabled = false;
        if (previewCamera != null) previewCamera.enabled = true;

        // 3. 切换按钮
        if (previewButton != null) previewButton.SetActive(false);
        if (exitPreviewButton != null) exitPreviewButton.SetActive(true);
    }

    // ★★★ 点击“退出预览”按钮时执行 ★★★
    public void SwitchToBuild()
    {
        // 1. 开启滤镜
        if (pixelManager != null) pixelManager.EnableEffect();

        // 2. 切换相机
        if (buildCamera != null) buildCamera.enabled = true;
        if (previewCamera != null) previewCamera.enabled = false;

        // 3. 切换按钮
        if (previewButton != null) previewButton.SetActive(true);
        if (exitPreviewButton != null) exitPreviewButton.SetActive(false);
    }
}