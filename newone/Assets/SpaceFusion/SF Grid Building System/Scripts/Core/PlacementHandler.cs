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
        // ==========================================
        // ★ 特效设置 ★
        // ==========================================
        [Header("Effects Settings")]
        public GameObject soilEffectPrefab;
        public GameObject waterEffectPrefab;
        public float effectYOffset = 0.1f;

        private readonly Dictionary<string, GameObject> _placedObjectDictionary = new();
        private PlacementGrid _cachedGrid;

        private Quaternion GetSafeGridRotation()
        {
            if (PlacementSystem.Instance != null) return PlacementSystem.Instance.GridRotation;
            if (_cachedGrid == null) _cachedGrid = FindObjectOfType<PlacementGrid>(true);
            return _cachedGrid != null ? _cachedGrid.transform.rotation : Quaternion.identity;
        }

        // --- 核心特效探测与生成逻辑 ---
        private void SpawnAppropriateEffect(Vector3 spawnPos)
        {
            GameObject effectToSpawn = soilEffectPrefab;

            RaycastHit hit;
            // 从生成位置上方 1m 向下探测，探测范围 5m
            if (Physics.Raycast(spawnPos + Vector3.up * 1.0f, Vector3.down, out hit, 5.0f))
            {
                if (hit.collider.CompareTag("Water"))
                {
                    effectToSpawn = waterEffectPrefab;
                }
            }

            if (effectToSpawn != null)
            {
                Instantiate(effectToSpawn, spawnPos + new Vector3(0, effectYOffset, 0), Quaternion.identity);
            }
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

            // 放置建筑时生成特效
            SpawnAppropriateEffect(obj.transform.position);

            if (GameManager.Instance != null) GameManager.Instance.AddObjectScore(placeableObj);

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
                if (GameManager.Instance != null) GameManager.Instance.AddObjectScore(placeableObj);
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

            // 移动放下时生成特效
            SpawnAppropriateEffect(obj.transform.position);
        }

        public void RemoveObjectPositions(string guid)
        {
            if (!_placedObjectDictionary.ContainsKey(guid)) return;
            var obj = _placedObjectDictionary[guid];
            if (!obj) return;
            var placedObjComp = obj.GetComponent<PlacedObject>();

            if (GameManager.Instance != null && placedObjComp != null && placedObjComp.placeable != null)
                GameManager.Instance.RemoveObjectScore(placedObjComp.placeable);

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

        // 核心辅助函数：获取已放置的对象
        public PlacedObject GetPlacedObjectByGuid(string guid)
        {
            if (!_placedObjectDictionary.TryGetValue(guid, out var obj) || obj == null) return null;
            return obj.GetComponent<PlacedObject>();
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

        [System.Serializable]
        public struct BuildingInfo
        {
            public string name;
            public Vector3 position;
            public float rotation;
            public Vector3Int gridPosition;
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
                    info.gridPosition = placedObj.data.gridPosition;
                    list.Add(info);
                }
            }
            return list;
        }
    }
}