using UnityEngine;

/// <summary>
/// 相机平移控制（不改变视角）：WASD/鼠标右键拖拽移动，滚轮拉近/拉远
/// </summary>
public class CameraPanController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("WASD移动速度（单位/秒）")]
    public float moveSpeed = 10f;
    [Tooltip("鼠标拖拽移动速度（单位/像素）")]
    public float dragSpeed = 0.05f;
    [Tooltip("滚轮拉近/拉远灵敏度")]
    public float scrollSpeed = 2f;

    [Header("视角保护（禁止修改）")]
    [Tooltip("是否锁定FOV/正交尺寸（确保视角不变）")]
    public bool lockView = true;
    private float originalFOV;       // 初始透视视野
    private float originalOrthSize; // 初始正交尺寸
    private Vector3 lastMousePos;   // 上一帧鼠标位置

    private void Start()
    {
        // 记录初始视角参数，锁定视角不改变
        Camera cam = GetComponent<Camera>();
        if (lockView)
        {
            originalFOV = cam.fieldOfView;
            originalOrthSize = cam.orthographicSize;
        }
    }

    private void Update()
    {
        // 强制锁定视角（防止意外修改）
        LockView();

        // WASD平移
        MoveByWASD();

        // 鼠标右键拖拽平移
        DragByMouse();

        // 滚轮拉近/拉远
        ZoomByScroll();
    }

    /// <summary>
    /// 锁定视角参数（FOV/正交尺寸），确保视角不变
    /// </summary>
    private void LockView()
    {
        if (!lockView) return;
        Camera cam = GetComponent<Camera>();
        cam.fieldOfView = originalFOV;
        cam.orthographicSize = originalOrthSize;
    }

    /// <summary>
    /// WASD平移：沿相机水平/前后方向移动，不旋转
    /// </summary>
    private void MoveByWASD()
    {
        // 获取相机本地坐标系的前后/左右方向（忽略Y轴，保持水平移动）
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        // 剔除Y轴，只保留水平方向（避免WASD导致上下移动）
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // 计算移动方向
        Vector3 moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDir += forward;
        if (Input.GetKey(KeyCode.S)) moveDir -= forward;
        if (Input.GetKey(KeyCode.D)) moveDir += right;
        if (Input.GetKey(KeyCode.A)) moveDir -= right;

        // 移动相机（乘以deltaTime保证帧率无关）
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 鼠标右键拖拽平移相机
    /// </summary>
    private void DragByMouse()
    {
        if (Input.GetMouseButtonDown(1)) // 右键按下时记录初始位置
        {
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(1)) // 右键按住时拖拽
        {
            // 计算鼠标偏移量
            Vector3 delta = Input.mousePosition - lastMousePos;
            // 转换为相机移动量（反向：鼠标左拖=相机右移，上拖=相机下移）
            Vector3 moveDir = -transform.right * delta.x * dragSpeed - transform.up * delta.y * dragSpeed;
            // 移动相机（忽略Y轴偏移，保持水平）
            moveDir.y = 0;
            transform.Translate(moveDir, Space.World);
            // 更新上一帧鼠标位置
            lastMousePos = Input.mousePosition;
        }
    }

    /// <summary>
    /// 鼠标滚轮拉近/拉远：沿相机朝向移动
    /// </summary>
    private void ZoomByScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        // 沿相机朝向移动（滚轮上滑=拉近，下滑=拉远）
        Vector3 forward = transform.forward;
        forward.y = 0; // 保持水平拉近，避免上下移动
        forward.Normalize();
        transform.Translate(forward * -scroll * scrollSpeed, Space.World);
    }
}