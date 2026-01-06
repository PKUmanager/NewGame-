using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers; // 引用 UndoManager
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    public class PlacementHandler : MonoBehaviour
    {
        private readonly Dictionary<string, GameObject> _placedObjectDictionary = new();
        private PlacementGrid _cachedGrid;

        // 安全获取网格旋转，防止读档时因为 System 未初始化而拿到 0 度，导致埋土里
        private Quaternion GetSafeGridRotation()
        {
            if (PlacementSystem.Instance != null)
                return PlacementSystem.Instance.GridRotation;

            if (_cachedGrid == null)
                _cachedGrid = FindObjectOfType<PlacementGrid>(true);

            if (_cachedGrid != null)
                return _cachedGrid.transform.rotation;

            return Quaternion.identity;
        }

        public string PlaceObject(Placeable placeableObj, Vector3 worldPosition, Vector3Int gridPosition, ObjectDirection direction,
             Vector3 offset, float cellSize)
        {
            var obj = Instantiate(placeableObj.Prefab);

            if (HomeLoader.Instance != null && HomeLoader.Instance.buildingRoot != null)
            {
                obj.transform.SetParent(HomeLoader.Instance.buildingRoot);
            }

            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.Initialize(placeableObj, gridPosition);
            placedObject.data.direction = direction;

            // 应用网格旋转
            Quaternion gridRot = GetSafeGridRotation();

            Vector3 rotatedOffset = gridRot * PlaceableUtils.GetTotalOffset(offset, direction);
            obj.transform.position = worldPosition + rotatedOffset;

            float rotationAngle = PlaceableUtils.GetRotationAngle(direction);
            obj.transform.rotation = gridRot * Quaternion.Euler(0, rotationAngle, 0);

            if (placeableObj.DynamicSize)
            {
                float targetHeight = placeableObj.GridType == GridDataType.Terrain ? obj.transform.localScale.y : cellSize;
                obj.transform.localScale = new Vector3(cellSize, targetHeight, cellSize);
            }

            ObjectGrouper.Instance.AddToGroup(obj, placeableObj.GridType);
            _placedObjectDictionary.Add(placedObject.data.guid, obj);

            var attr = placeableObj.Prefab.GetComponent<BuildingAttribute>();
            if (attr != null && NPCManager.Instance != null)
            {
                NPCManager.Instance.AddBuildingCount(attr.type);
            }

            // ★★★ [新增] 记录放置操作 ★★★
            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.RecordPlaceAction(placedObject.data.guid);
            }

            return placedObject.data.guid;
        }

        public string PlaceLoadedObject(Placeable placeableObj, Vector3 worldPosition, PlaceableObjectData podata, float cellSize)
        {
            var obj = Instantiate(placeableObj.Prefab);

            // 确保挂载到正确父节点 (保持 HomeLoader 的逻辑)
            if (HomeLoader.Instance != null && HomeLoader.Instance.buildingRoot != null)
            {
                obj.transform.SetParent(HomeLoader.Instance.buildingRoot);
            }

            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.data.gridPosition = podata.gridPosition;
            placedObject.placeable = placeableObj;
            placedObject.Initialize(podata);

            Quaternion gridRot = GetSafeGridRotation();

            // 保持之前的修复：使用 Prefab 计算 Offset
            var offset = PlaceableUtils.CalculateOffset(placeableObj.Prefab, cellSize);

            Vector3 rotatedOffset = gridRot * PlaceableUtils.GetTotalOffset(offset, podata.direction);
            obj.transform.position = worldPosition + rotatedOffset;

            obj.transform.rotation = gridRot * Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(podata.direction), 0);

            if (placeableObj.DynamicSize)
            {
                float targetHeight = placeableObj.GridType == GridDataType.Terrain ? obj.transform.localScale.y : cellSize;
                obj.transform.localScale = new Vector3(cellSize, targetHeight, cellSize);
            }

            // 防止撤回时重复添加 key 导致报错
            if (!_placedObjectDictionary.ContainsKey(placedObject.data.guid))
            {
                _placedObjectDictionary.Add(placedObject.data.guid, obj);
            }

            ObjectGrouper.Instance.AddToGroup(obj, placeableObj.GridType);

            // ★★★ [新增] 记录放置操作 (用于撤回“撤回删除”的操作) ★★★
            // 只有当 UndoManager 没有在执行 Undo 时，RecordPlaceAction 才会生效，所以这里直接调用很安全
            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.RecordPlaceAction(placedObject.data.guid);
            }

            return podata.guid;
        }

        public void PlaceMovedObject(GameObject obj, Vector3 worldPosition, Vector3Int gridPosition, ObjectDirection direction, float cellSize)
        {
            var placedObject = obj.GetComponent<PlacedObject>();

            // 保持之前的修复
            var offset = PlaceableUtils.CalculateOffset(placedObject.placeable.Prefab, cellSize);

            Quaternion gridRot = GetSafeGridRotation();

            Vector3 rotatedOffset = gridRot * PlaceableUtils.GetTotalOffset(offset, direction);
            obj.transform.position = worldPosition + rotatedOffset;

            obj.transform.rotation = gridRot * Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(direction), 0);

            placedObject.data.gridPosition = gridPosition;
            placedObject.data.direction = direction;
        }

        public void RemoveObjectPositions(string guid)
        {
            if (!_placedObjectDictionary.ContainsKey(guid)) return;

            var obj = _placedObjectDictionary[guid];
            if (!obj) return;

            var placedObjComp = obj.GetComponent<PlacedObject>();

            // ★★★ [新增] 在删除前，记录数据用于撤回 ★★★
            if (UndoManager.Instance != null && placedObjComp != null)
            {
                UndoManager.Instance.RecordRemoveAction(placedObjComp.data);
            }

            if (NPCManager.Instance != null)
            {
                var attr = placedObjComp.placeable.Prefab.GetComponent<BuildingAttribute>();
                if (attr == null) attr = obj.GetComponent<BuildingAttribute>();

                if (attr != null)
                {
                    NPCManager.Instance.RemoveBuildingCount(attr.type);
                }
            }

            placedObjComp.RemoveFromSaveData();
            _placedObjectDictionary.Remove(guid);
            Destroy(obj);
        }

        // BuildSaver 所需的结构和方法
        public struct BuildingInfo
        {
            public string name;
            public Vector3 position;
            public float rotation;
        }

        public List<BuildingInfo> GetAllBuildings()
        {
            List<BuildingInfo> list = new List<BuildingInfo>();

            foreach (var kvp in _placedObjectDictionary)
            {
                GameObject obj = kvp.Value;
                PlacedObject placedObj = obj.GetComponent<PlacedObject>();

                if (placedObj != null && placedObj.placeable != null)
                {
                    BuildingInfo info = new BuildingInfo();
                    info.name = placedObj.placeable.Prefab.name;
                    info.position = obj.transform.position;
                    info.rotation = obj.transform.eulerAngles.y;

                    list.Add(info);
                }
            }
            return list;
        }
    }
}