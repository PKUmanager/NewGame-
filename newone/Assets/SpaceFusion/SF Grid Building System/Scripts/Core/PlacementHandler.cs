using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    public class PlacementHandler : MonoBehaviour
    {
        private readonly Dictionary<string, GameObject> _placedObjectDictionary = new();
        private PlacementGrid _cachedGrid;

        private Quaternion GetSafeGridRotation()
        {
            if (PlacementSystem.Instance != null) return PlacementSystem.Instance.GridRotation;
            if (_cachedGrid == null) _cachedGrid = FindObjectOfType<PlacementGrid>(true);
            return _cachedGrid != null ? _cachedGrid.transform.rotation : Quaternion.identity;
        }

        public string PlaceObject(Placeable placeableObj, Vector3 worldPosition, Vector3Int gridPosition, ObjectDirection direction, Vector3 offset, float cellSize)
        {
            var obj = Instantiate(placeableObj.Prefab);
            if (HomeLoader.Instance != null && HomeLoader.Instance.buildingRoot != null) obj.transform.SetParent(HomeLoader.Instance.buildingRoot);

            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.Initialize(placeableObj, gridPosition);
            placedObject.data.direction = direction;

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
            if (attr != null && NPCManager.Instance != null) NPCManager.Instance.AddBuildingCount(attr.type);
            if (UndoManager.Instance != null) UndoManager.Instance.RecordPlaceAction(placedObject.data.guid);

            return placedObject.data.guid;
        }

        public string PlaceLoadedObject(Placeable placeableObj, Vector3 worldPosition, PlaceableObjectData podata, float cellSize)
        {
            var obj = Instantiate(placeableObj.Prefab);
            if (HomeLoader.Instance != null && HomeLoader.Instance.buildingRoot != null) obj.transform.SetParent(HomeLoader.Instance.buildingRoot);

            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.data.gridPosition = podata.gridPosition;
            placedObject.placeable = placeableObj;
            placedObject.Initialize(podata);

            Quaternion gridRot = GetSafeGridRotation();
            var offset = PlaceableUtils.CalculateOffset(placeableObj.Prefab, cellSize);
            Vector3 rotatedOffset = gridRot * PlaceableUtils.GetTotalOffset(offset, podata.direction);
            obj.transform.position = worldPosition + rotatedOffset;
            obj.transform.rotation = gridRot * Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(podata.direction), 0);

            if (placeableObj.DynamicSize)
            {
                float targetHeight = placeableObj.GridType == GridDataType.Terrain ? obj.transform.localScale.y : cellSize;
                obj.transform.localScale = new Vector3(cellSize, targetHeight, cellSize);
            }

            if (!_placedObjectDictionary.ContainsKey(placedObject.data.guid))
            {
                _placedObjectDictionary.Add(placedObject.data.guid, obj);
            }

            ObjectGrouper.Instance.AddToGroup(obj, placeableObj.GridType);
            if (UndoManager.Instance != null) UndoManager.Instance.RecordPlaceAction(placedObject.data.guid);
            return podata.guid;
        }

        public void PlaceMovedObject(GameObject obj, Vector3 worldPosition, Vector3Int gridPosition, ObjectDirection direction, float cellSize)
        {
            var placedObject = obj.GetComponent<PlacedObject>();
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
            if (UndoManager.Instance != null && placedObjComp != null) UndoManager.Instance.RecordRemoveAction(placedObjComp.data);
            if (NPCManager.Instance != null)
            {
                var attr = obj.GetComponent<BuildingAttribute>();
                if (attr != null) NPCManager.Instance.RemoveBuildingCount(attr.type);
            }
            placedObjComp.RemoveFromSaveData();
            _placedObjectDictionary.Remove(guid);
            Destroy(obj);
        }

        public void ClearEnvironment()
        {
            foreach (var kvp in _placedObjectDictionary)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _placedObjectDictionary.Clear();
            if (GameManager.Instance != null && GameManager.Instance.saveData != null)
            {
                GameManager.Instance.saveData.placeableObjectDataCollection.Clear();
            }
        }

        // ==========================================
        // ★★★ 修改：结构体增加网格坐标 ★★★
        // ==========================================
        [System.Serializable]
        public struct BuildingInfo
        {
            public string name;
            public Vector3 position;
            public float rotation;
            public Vector3Int gridPosition; // 新增：绝对网格坐标
        }

        public List<BuildingInfo> GetAllBuildings()
        {
            List<BuildingInfo> list = new List<BuildingInfo>();
            foreach (var kvp in _placedObjectDictionary)
            {
                GameObject obj = kvp.Value;
                if (obj == null) continue;

                PlacedObject placedObj = obj.GetComponent<PlacedObject>();
                if (placedObj != null && placedObj.placeable != null)
                {
                    BuildingInfo info = new BuildingInfo();
                    info.name = placedObj.placeable.Prefab.name;
                    info.position = obj.transform.position;
                    info.rotation = obj.transform.eulerAngles.y;
                    // ★★★ 记录准确的网格坐标 ★★★
                    info.gridPosition = placedObj.data.gridPosition;
                    list.Add(info);
                }
            }
            return list;
        }
    }
}