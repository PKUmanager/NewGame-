using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
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

            return placedObject.data.guid;
        }

        public string PlaceLoadedObject(Placeable placeableObj, Vector3 worldPosition, PlaceableObjectData podata, float cellSize)
        {
            var obj = Instantiate(placeableObj.Prefab);
            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.data.gridPosition = podata.gridPosition;
            placedObject.placeable = placeableObj;
            placedObject.Initialize(podata);

            Quaternion gridRot = GetSafeGridRotation();

            // ★★★ [核心修复] ★★★ 
            // 必须使用【Prefab】来计算偏移量！
            // 因为场景里的 'obj' 已经是歪的(跟着网格转了)，算出来的偏移量是错的。
            // 使用 Prefab 算出来的才是最原始、最标准的中心点。
            var offset = PlaceableUtils.CalculateOffset(placeableObj.Prefab, cellSize);

            Vector3 rotatedOffset = gridRot * PlaceableUtils.GetTotalOffset(offset, podata.direction);
            obj.transform.position = worldPosition + rotatedOffset;

            obj.transform.rotation = gridRot * Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(podata.direction), 0);

            if (placeableObj.DynamicSize)
            {
                float targetHeight = placeableObj.GridType == GridDataType.Terrain ? obj.transform.localScale.y : cellSize;
                obj.transform.localScale = new Vector3(cellSize, targetHeight, cellSize);
            }

            _placedObjectDictionary.Add(placedObject.data.guid, obj);
            ObjectGrouper.Instance.AddToGroup(obj, placeableObj.GridType);
            return podata.guid;
        }

        public void PlaceMovedObject(GameObject obj, Vector3 worldPosition, Vector3Int gridPosition, ObjectDirection direction, float cellSize)
        {
            var placedObject = obj.GetComponent<PlacedObject>();

            // ★★★ [核心修复] 移动时也必须用 Prefab 算 Offset ★★★
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

            if (NPCManager.Instance != null)
            {
                var attr = obj.GetComponent<PlacedObject>().placeable.Prefab.GetComponent<BuildingAttribute>();
                if (attr == null) attr = obj.GetComponent<BuildingAttribute>();

                if (attr != null)
                {
                    NPCManager.Instance.RemoveBuildingCount(attr.type);
                }
            }

            obj.GetComponent<PlacedObject>().RemoveFromSaveData();
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