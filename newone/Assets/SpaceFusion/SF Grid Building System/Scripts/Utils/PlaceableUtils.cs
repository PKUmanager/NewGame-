using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core; // 引用 CustomPlacementBounds
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils
{
    public static class PlaceableUtils
    {

        public static Vector3 GetTotalOffset(Vector3 offset, ObjectDirection direction)
        {
            var angle = GetRotationAngle(direction);
            var pivotOffset = GetRotatedPivotOffset(angle, offset);
            return pivotOffset;
        }

        /// <summary>
        /// [修改版] 计算偏移量
        /// 现在支持读取 CustomPlacementBounds 来"伪造"包围盒数据
        /// </summary>
        public static Vector3 CalculateOffset(GameObject obj, float cellSize)
        {
            if (!obj) return Vector3.zero;

            Vector3 originalSize;
            Vector3 bottomLeft;

            // 1. 尝试获取自定义边界 (虚拟盒子)
            var customBounds = obj.GetComponent<CustomPlacementBounds>();

            if (customBounds != null && customBounds.UseCustomBounds)
            {
                // [核心修改] 使用自定义数据"欺骗"算法
                // 伪造 Size
                originalSize = customBounds.BoundsSize;

                // 伪造 BottomLeft (世界坐标)
                // 逻辑：世界中心 = Pivot + Offset
                // 左下角 = 世界中心 - 半个尺寸
                Vector3 worldCenter = obj.transform.TransformPoint(customBounds.CenterOffset);
                Vector3 worldExtents = originalSize * 0.5f;
                bottomLeft = worldCenter - worldExtents;

                // Debug.Log($"Using Custom Bounds for {obj.name}: Size={originalSize}, Min={bottomLeft}");
            }
            else
            {
                // [回退] 使用旧的 Renderer 逻辑
                var rend = obj.GetComponentInChildren<Renderer>();
                if (!rend)
                {
                    Debug.LogError($"No renderer attached for object {obj.name}");
                    return Vector3.zero;
                }
                originalSize = rend.bounds.size;
                bottomLeft = rend.bounds.min;
            }

            // -----------------------------------------------------------
            // 下面是原封不动的原始算法，我们只替换了上面的输入变量
            // 这样能保证计算逻辑的一致性，但计算对象变成了只有树根那么大的"虚拟盒子"
            // -----------------------------------------------------------

            var roundedX = (int)Math.Ceiling(Math.Round(originalSize.x / cellSize, 3));
            var roundedZ = (int)Math.Ceiling(Math.Round(originalSize.z / cellSize, 3));
            var adjustedSizeX = roundedX * cellSize;
            var adjustedSizeZ = roundedZ * cellSize;

            // 计算空隙 (Margin)
            var marginX = (adjustedSizeX - originalSize.x) / 2f;
            var marginZ = (adjustedSizeZ - originalSize.z) / 2f;

            // 最终计算
            const int decimalsToRound = 6;
            var pivotOffset = SfMathUtils.RoundVector(new Vector3(marginX, 0, marginZ) + (obj.transform.position - bottomLeft), decimalsToRound);

            return pivotOffset;
        }

        // --- 下面的辅助函数保持原样 ---

        public static ObjectDirection GetNextDir(ObjectDirection dir)
        {
            return dir switch
            {
                ObjectDirection.Down => ObjectDirection.Left,
                ObjectDirection.Left => ObjectDirection.Up,
                ObjectDirection.Up => ObjectDirection.Right,
                ObjectDirection.Right => ObjectDirection.Down,
                _ => ObjectDirection.Down
            };
        }

        public static int GetRotationAngle(ObjectDirection direction)
        {
            return direction switch
            {
                ObjectDirection.Down => 0,
                ObjectDirection.Left => 90,
                ObjectDirection.Up => 180,
                ObjectDirection.Right => 270,
                _ => 0
            };
        }

        public static Vector2 GetCorrectedObjectSize(Placeable placeable, ObjectDirection direction, float cellSize)
        {
            var correctedSize = HandleOptionalDynamicSize(placeable, cellSize);
            var cellBasedObjectSize = SfMathUtils.RoundToNextMultiple(correctedSize, cellSize);
            return direction switch
            {
                ObjectDirection.Up => cellBasedObjectSize,
                ObjectDirection.Down => cellBasedObjectSize,
                ObjectDirection.Left => new Vector2(cellBasedObjectSize.y, cellBasedObjectSize.x),
                ObjectDirection.Right => new Vector2(cellBasedObjectSize.y, cellBasedObjectSize.x),
                _ => cellBasedObjectSize
            };
        }

        public static Vector2Int GetOccupiedCells(Placeable placeable, ObjectDirection direction, float cellSize)
        {
            var correctedSize = HandleOptionalDynamicSize(placeable, cellSize);
            var cellsX = Mathf.CeilToInt(correctedSize.x / cellSize);
            var cellsY = Mathf.CeilToInt(correctedSize.y / cellSize);
            var occupiedCells = new Vector2Int(cellsX, cellsY);
            return GetRotationBasedOccupiedCells(occupiedCells, direction);
        }

        #region Private Functions

        private static Vector2 HandleOptionalDynamicSize(Placeable placeable, float cellSize)
        {
            if (placeable.DynamicSize)
            {
                return placeable.Size * cellSize;
            }
            return placeable.Size;
        }

        private static Vector2Int GetRotationBasedOccupiedCells(Vector2Int size, ObjectDirection direction)
        {
            return direction switch
            {
                ObjectDirection.Up => size,
                ObjectDirection.Down => size,
                ObjectDirection.Left => new Vector2Int(size.y, size.x),
                ObjectDirection.Right => new Vector2Int(size.y, size.x),
                _ => size
            };
        }

        private static Vector3 GetRotatedPivotOffset(float rot, Vector3 pivotOffset)
        {
            var adjustedOffset = pivotOffset;
            var rotation = (rot % 360 + 360) % 360;
            adjustedOffset = (int)rotation switch
            {
                90 => new Vector3(pivotOffset.z, pivotOffset.y, pivotOffset.x),
                180 => new Vector3(pivotOffset.x, pivotOffset.y, pivotOffset.z),
                270 => new Vector3(pivotOffset.z, pivotOffset.y, pivotOffset.x),
                _ => adjustedOffset
            };
            return adjustedOffset;
        }
        #endregion
    }
}