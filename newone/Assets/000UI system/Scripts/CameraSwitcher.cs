using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("拖拽你的两台Camera进来")]
    public Camera mainCamera;          // 你的“主摄像机 Camera”
    public Camera holderCamera;        // CameraHolder下面那台Camera

    private void Start()
    {
        // 开局：只开主摄像机
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        if (holderCamera != null) holderCamera.gameObject.SetActive(false);
    }

    // 给按钮绑定这个函数
    public void SwitchToHolder()
    {
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (holderCamera != null) holderCamera.gameObject.SetActive(true);
    }

    // 如果你还想切回主摄像机
    public void SwitchToMain()
    {
        if (holderCamera != null) holderCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
    }
}
