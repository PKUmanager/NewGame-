using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    /// <summary>
    /// 这是一个"虚拟包围盒"控制器。
    /// 挂载在物体上后，Grid系统会忽略真实的 Mesh 大小，而使用这里设定的"虚拟大小"进行计算。
    /// </summary>
    public class CustomPlacementBounds : MonoBehaviour
    {
        [Header("启用开关")]
        public bool UseCustomBounds = true;

        [Header("虚拟包围盒设置 (相对于物体Pivot)")]
        [Tooltip("虚拟盒子的中心点偏移。对于树木，通常设为 (0,0,0) 或根据树根位置微调")]
        public Vector3 CenterOffset = Vector3.zero;

        [Tooltip("虚拟盒子的大小。建议设为树根的实际碰撞大小，例如 (1, 2, 1)")]
        public Vector3 BoundsSize = new Vector3(1f, 2f, 1f);

        [Header("调试显示")]
        public Color GizmoColor = new Color(0, 1, 0, 0.5f);

        private void OnDrawGizmosSelected()
        {
            if (!UseCustomBounds) return;

            Gizmos.color = GizmoColor;
            // 计算世界坐标下的中心
            Vector3 worldCenter = transform.TransformPoint(CenterOffset);
            // 绘制预览框，方便你对齐
            Gizmos.DrawCube(worldCenter, BoundsSize);
            Gizmos.DrawWireCube(worldCenter, BoundsSize);
        }
    }
}