using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    /// <summary>
    /// Handles placing and removing objects and keeps a list of all object references by guid
    /// </summary>
    public class PlacementHandler : MonoBehaviour
    {
        /// <summary>
        /// keeps track of all objects that are already placed on the grid
        /// dictionary is easier for removing & tracking objects than a simple list
        /// </summary>
        private readonly Dictionary<string, GameObject> _placedObjectDictionary = new();


        /// <summary>
        /// Handles placing a new object to the grid
        /// Initializes the PlacedObject data, which creates a new guid for unique identification of this object
        /// </summary>
        public string PlaceObject(Placeable placeableObj, Vector3 worldPosition, Vector3Int gridPosition, ObjectDirection direction,
             Vector3 offset, float cellSize)
        {
            var obj = Instantiate(placeableObj.Prefab);

            // =========================================================
            // 归位逻辑：把房子放到 BuildingContainer 下面
            if (HomeLoader.Instance != null && HomeLoader.Instance.buildingRoot != null)
            {
                obj.transform.SetParent(HomeLoader.Instance.buildingRoot);
            }
            // =========================================================

            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.Initialize(placeableObj, gridPosition);
            placedObject.data.direction = direction;

            obj.transform.position = worldPosition + PlaceableUtils.GetTotalOffset(offset, direction);

            // 计算旋转
            float rotationAngle = PlaceableUtils.GetRotationAngle(direction);
            obj.transform.rotation = Quaternion.Euler(0, rotationAngle, 0);

            // 动态大小处理
            if (placeableObj.DynamicSize)
            {
                float targetHeight = placeableObj.GridType == GridDataType.Terrain ? obj.transform.localScale.y : cellSize;
                obj.transform.localScale = new Vector3(cellSize, targetHeight, cellSize);
            }

            ObjectGrouper.Instance.AddToGroup(obj, placeableObj.GridType);
            _placedObjectDictionary.Add(placedObject.data.guid, obj);

            // =========================================================
            // ★★★ 【新增 1】 告诉 NPC 经理：我造了个新东西，快统计一下！ ★★★
            // =========================================================

            // 1. 获取这个建筑 Prefab 身上的“身份证” (BuildingAttribute)
            var attr = placeableObj.Prefab.GetComponent<BuildingAttribute>();

            // 2. 如果这东西有分类标签，并且 NPC经理 在场
            if (attr != null && NPCManager.Instance != null)
            {
                // 3. 增加计数 (比如铺路+1)，并自动刷新 NPC 显示
                NPCManager.Instance.AddBuildingCount(attr.type);
            }
            else
            {
                // (可选) 调试用，如果你发现 NPC 不出来，看看控制台有没有这句话
                // Debug.LogWarning("注意：这个建筑没有挂 BuildingAttribute 脚本，或者 NPCManager 没在场景里");
            }
            // =========================================================


            // =========================================================
            // ★★★ 【新增 2】 云端上传代码 ★★★
            // =========================================================
            if (BuildSaver.Instance != null)
            {
                string prefabName = placeableObj.Prefab.name;
                Vector3 finalPos = obj.transform.position;
                BuildSaver.Instance.SaveOneBuilding(prefabName, finalPos, rotationAngle);
            }
            // =========================================================

            return placedObject.data.guid;
        }

        /// <summary>
        /// Handles placing an object that is loaded from the saveFile (handling is a little bit different from placing a new object)
        /// instead of the gridPosition we have podata as last parameter where we can Initialize the newly placed object with the previously saved guid, grid pos etc...
        /// </summary>
        public string PlaceLoadedObject(Placeable placeableObj, Vector3 worldPosition, PlaceableObjectData podata, float cellSize)
        {
            var obj = Instantiate(placeableObj.Prefab);
            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.data.gridPosition = podata.gridPosition;
            placedObject.placeable = placeableObj;
            placedObject.Initialize(podata);

            var offset = PlaceableUtils.CalculateOffset(obj, cellSize);
            obj.transform.position = worldPosition + PlaceableUtils.GetTotalOffset(offset, podata.direction);
            obj.transform.rotation = Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(podata.direction), 0);

            // [核心修改] 读档时也做同样的逻辑处理
            if (placeableObj.DynamicSize)
            {
                float targetHeight = placeableObj.GridType == GridDataType.Terrain ? obj.transform.localScale.y : cellSize;
                obj.transform.localScale = new Vector3(cellSize, targetHeight, cellSize);
            }

            _placedObjectDictionary.Add(placedObject.data.guid, obj);
            ObjectGrouper.Instance.AddToGroup(obj, placeableObj.GridType);
            return podata.guid;
        }

        /// <summary>
        /// handles moving a placed object to his new position
        /// </summary>
        public void PlaceMovedObject(GameObject obj, Vector3 worldPosition, Vector3Int gridPosition, ObjectDirection direction, float cellSize)
        {
            var placedObject = obj.GetComponent<PlacedObject>();
            var offset = PlaceableUtils.CalculateOffset(obj, cellSize);
            obj.transform.position = worldPosition + PlaceableUtils.GetTotalOffset(offset, direction);
            obj.transform.rotation = Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(direction), 0);
            placedObject.data.gridPosition = gridPosition;
            placedObject.data.direction = direction;
            // no need to update reference for moving object, since guid still stays the same 
        }

        /// <summary>
        /// Based on the guid, we load the placed object from our dictionary and remove it from the SaveData and from the dictionary
        /// then we destroy the instantiated object in the scene
        /// </summary>
        public void RemoveObjectPositions(string guid)
        {
            var obj = _placedObjectDictionary[guid];
            if (!obj)
            {
                Debug.LogError($"Removing object error: {guid} is not saved in dictionary");
                return;
            }

            obj.GetComponent<PlacedObject>().RemoveFromSaveData();
            _placedObjectDictionary.Remove(guid);
            // destroy the object and set the reference of the list at the proper index to null
            Destroy(obj);
        }
    }
}