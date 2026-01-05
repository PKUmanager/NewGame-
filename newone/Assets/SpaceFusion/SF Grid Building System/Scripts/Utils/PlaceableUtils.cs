using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
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

        public static Vector3 CalculateOffset(GameObject obj, float cellSize)
        {
            if (!obj) return Vector3.zero;

            Vector3 originalSize;
            Vector3 bottomLeft;

            // 1. 尝试获取自定义边界
            var customBounds = obj.GetComponent<CustomPlacementBounds>();

            if (customBounds != null && customBounds.UseCustomBounds)
            {
                originalSize = customBounds.BoundsSize;
                Vector3 worldCenter = obj.transform.TransformPoint(customBounds.CenterOffset);
                Vector3 worldExtents = originalSize * 0.5f;
                bottomLeft = worldCenter - worldExtents;
            }
            else
            {
                var rend = obj.GetComponentInChildren<Renderer>();
                if (!rend)
                {
                    return Vector3.zero;
                }
                originalSize = rend.bounds.size;
                bottomLeft = rend.bounds.min;
            }

            var roundedX = (int)Math.Ceiling(Math.Round(originalSize.x / cellSize, 3));
            var roundedZ = (int)Math.Ceiling(Math.Round(originalSize.z / cellSize, 3));
            var adjustedSizeX = roundedX * cellSize;
            var adjustedSizeZ = roundedZ * cellSize;

            var marginX = (adjustedSizeX - originalSize.x) / 2f;
            var marginZ = (adjustedSizeZ - originalSize.z) / 2f;

            const int decimalsToRound = 6;
            var pivotOffset = SfMathUtils.RoundVector(new Vector3(marginX, 0, marginZ) + (obj.transform.position - bottomLeft), decimalsToRound);

            return pivotOffset;
        }

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

        // [还原] 使用你之前的 Switch 逻辑，而不是 Quaternion
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