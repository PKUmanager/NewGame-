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
            obj.AddComponent<PlacedObject>();
            var placedObject = obj.GetComponent<PlacedObject>();
            placedObject.Initialize(placeableObj, gridPosition);
            placedObject.data.direction = direction;

            obj.transform.position = worldPosition + PlaceableUtils.GetTotalOffset(offset, direction);
            obj.transform.rotation = Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(direction), 0);

            // [核心修改] 针对 DynamicSize 的特殊处理
            if (placeableObj.DynamicSize)
            {
                // 如果是地形(Terrain)，保持 Prefab 原始高度(Y)，只拉伸 X 和 Z
                // 注意：请确保你的 GridDataType 枚举里确实有一个叫 'Terrain' 的类型，如果叫 'Ground' 或其他名字，请在这里替换
                float targetHeight = placeableObj.GridType == GridDataType.Terrain ? obj.transform.localScale.y : cellSize;
                obj.transform.localScale = new Vector3(cellSize, targetHeight, cellSize);
            }

            ObjectGrouper.Instance.AddToGroup(obj, placeableObj.GridType);
            _placedObjectDictionary.Add(placedObject.data.guid, obj);
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