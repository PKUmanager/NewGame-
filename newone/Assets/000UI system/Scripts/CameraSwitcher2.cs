using UnityEngine;

public class CameraSwitcher2 : MonoBehaviour
{
    [Header("Cameras")]
    public Camera buildCamera;     // CameraHolder
    public Camera previewCamera;   // NPCCamera

    [Header("Pixel Effect (新增)")]
    // 这里用来放那个控制滤镜的脚本
    public PixelEffectManager pixelManager;

    [Header("UI Buttons")]
    public GameObject previewButton;      // 预览按钮
    public GameObject exitPreviewButton;  // 退出预览按钮

    private void Start()
    {
        SwitchToBuild(); // 启动时默认建造模式
    }

    public void SwitchToPreview()
    {
        // 1. 关闭像素滤镜（变清晰）
        if (pixelManager != null) pixelManager.DisableEffect();

        // 2. 原有的相机切换逻辑
        if (buildCamera != null) buildCamera.enabled = false;
        if (previewCamera != null) previewCamera.enabled = true;

        // 3. 原有的UI切换逻辑
        if (previewButton != null) previewButton.SetActive(false);
        if (exitPreviewButton != null) exitPreviewButton.SetActive(true);
    }

    public void SwitchToBuild()
    {
        // 1. 开启像素滤镜（变像素）
        if (pixelManager != null) pixelManager.EnableEffect();

        // 2. 原有的相机切换逻辑
        if (buildCamera != null) buildCamera.enabled = true;
        if (previewCamera != null) previewCamera.enabled = false;

        // 3. 原有的UI切换逻辑
        if (previewButton != null) previewButton.SetActive(true);
        if (exitPreviewButton != null) exitPreviewButton.SetActive(false);
    }
}