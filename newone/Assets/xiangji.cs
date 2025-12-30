using UnityEngine;

public class CameraScaleDistance : MonoBehaviour
{
    public Transform target; // 拖入你要靠近的目标物体（比如角色/模型）
    [Header("距离调整（越小越近，实时生效）")]
    [Range(0.1f, 10f)] public float scaleFactor = 1f; // 核心：调小=靠近，调大=远离
    [Header("安全最小距离（防止穿模）")]
    public float minDistance = 1f; // 相机离目标的最小距离（比如1米，避免穿墙/穿模型）
    [Header("微调步长（按快捷键用）")]
    public float step = 0.1f; // 按↑/↓每次调近/调远0.1

    private Vector3 originalDelta; // 初始方向差值（保持X/Y/Z比例）
    private float originalDistance; // 初始总距离（用于限制最小距离）

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("请给相机脚本拖入目标物体！");
            return;
        }
        // 记录初始差值（方向比例）和初始总距离
        originalDelta = transform.position - target.position;
        originalDistance = originalDelta.magnitude; // 计算相机到目标的直线距离
    }

    void Update()
    {
        if (target == null) return;

        // 实时更新相机位置（保持比例+限制最小距离）
        float finalFactor = scaleFactor;
        // 计算当前缩放后的距离，确保不小于最小距离
        float currentDistance = originalDistance * finalFactor;
        if (currentDistance < minDistance)
        {
            finalFactor = minDistance / originalDistance; // 强制拉到最小距离
            scaleFactor = finalFactor; // 同步滑动条数值
        }
        // 按比例更新位置
        transform.position = target.position + originalDelta * finalFactor;
        // 始终朝向目标（避免相机偏移方向）
        transform.LookAt(target);

        // 快捷键微调：按↑=调近（减小系数），按↓=调远（增大系数）
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            scaleFactor -= step;
            scaleFactor = Mathf.Max(scaleFactor, minDistance / originalDistance); // 不超最小距离
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            scaleFactor += step;
        }
    }

    // 一键贴到最小距离（可选）
    [ContextMenu("一键贴近目标")]
    public void SnapToMinDistance()
    {
        scaleFactor = minDistance / originalDistance;
    }
}
