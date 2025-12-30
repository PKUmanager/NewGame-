using UnityEngine;

public class CameraSwitcher2 : MonoBehaviour
{
    [Header("Build Mode Camera")]
    public Camera buildCamera;      // CameraHolder 上的 Camera

    [Header("Preview Camera")]
    public Camera previewCamera;    // NPCCamera 上的 Camera

    private void Start()
    {
        // 启动时默认：建造模式
        SwitchToBuild();
    }

    public void SwitchToPreview()
    {
        if (buildCamera != null) buildCamera.enabled = false;
        if (previewCamera != null) previewCamera.enabled = true;
    }

    public void SwitchToBuild()
    {
        if (buildCamera != null) buildCamera.enabled = true;
        if (previewCamera != null) previewCamera.enabled = false;
    }
}