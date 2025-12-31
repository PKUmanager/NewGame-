using UnityEngine;

public class CameraSwitcher2 : MonoBehaviour
{
    [Header("Cameras")]
    public Camera buildCamera;     // CameraHolder
    public Camera previewCamera;   // NPCCamera

    [Header("UI Buttons")]
    public GameObject previewButton;      // 预览按钮
    public GameObject exitPreviewButton;  // 退出预览按钮

    private void Start()
    {
        SwitchToBuild(); // 启动时默认建造模式
    }

    public void SwitchToPreview()
    {
        // 相机切换
        if (buildCamera != null) buildCamera.enabled = false;
        if (previewCamera != null) previewCamera.enabled = true;

        // UI 切换
        if (previewButton != null) previewButton.SetActive(false);
        if (exitPreviewButton != null) exitPreviewButton.SetActive(true);
    }

    public void SwitchToBuild()
    {
        // 相机切换
        if (buildCamera != null) buildCamera.enabled = true;
        if (previewCamera != null) previewCamera.enabled = false;

        // UI 切换
        if (previewButton != null) previewButton.SetActive(true);
        if (exitPreviewButton != null) exitPreviewButton.SetActive(false);
    }
}