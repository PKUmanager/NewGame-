using UnityEngine;

public class UploadWorkTaskHook : MonoBehaviour
{
    // 把这个函数绑定到【新上传作品按钮】的 OnClick
    public void OnUploadButtonClicked()
    {
        if (TaskService.Instance != null)
        {
            TaskService.Instance.MarkUploadCompleted();
        }
        else
        {
            Debug.LogError("❌ TaskService.Instance 为空：场景里没有 TaskService 或尚未初始化");
        }
    }
}