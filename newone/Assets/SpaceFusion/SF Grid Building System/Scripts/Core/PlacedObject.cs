using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core {
    /// <summary>
    /// PlacedObject is a helper script that will be attached automatically to the GameObjects that are placed on the grid
    /// Every gameObject will get a unique id (guid) assigned when the Initialize function is called.
    /// This script listens to the InputManagers LMB Hold action and checks if the action belongs to the object
    /// If so then a PlaceableActionTooltip will be triggered to give you some choices what you want to do with this object
    /// This script also handles adding and removing the objects save information so that we can properly save and load the data after application restart
    /// OnApplicationQuit the data is automatically added to the saveSystem before the SaveFile is written
    /// For removing there is a RemoveFromSaveData function that will be triggered when the user selects to remove this object from the grid
    /// </summary>
    public class PlacedObject : MonoBehaviour {
        /// <summary>
        /// we can have multiple placed objects,but they are placed at a time during the game.
        /// The Action Tooltip needs to subscribe to the PlaceObject action directly, so we will make this static here
        /// </summary>
        public static Action<PlacedObject> holdComplete;
        public Placeable placeable;

        private Vector3 _lastMousePosition;
        private Camera _sceneCamera;

        [ReadOnly()]
        public PlaceableObjectData data = new();

        private void OnEnable() {
            InputManager.Instance.OnLmbPress += HandleMousePress;
            InputManager.Instance.OnLmbHold += HandleMouseHold;
            _sceneCamera = GameManager.Instance.SceneCamera;
        }

        private void OnDisable() {
            InputManager.Instance.OnLmbPress -= HandleMousePress;
            InputManager.Instance.OnLmbHold -= HandleMouseHold;
        }


        /// <summary>
        /// Initialize function for creating a new object, we get the Placeable and the grid pos and then set all the needed information
        /// ALso a unique GUID will be created for this instantiated object
        /// </summary>
        public void Initialize(Placeable scriptable, Vector3Int gridPosition) {
            placeable = scriptable;
            data.assetIdentifier = scriptable.GetAssetIdentifier();
            data.gridPosition = gridPosition;
            data.guid = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes the object that is created by loading our saved game state (no not really a new object)
        /// the podata will be assigned by the loaded data and no new GUID will be created (uses the saved GUID)
        /// </summary>
        public void Initialize(PlaceableObjectData podata) {
            data = podata;
        }


        private void HandleMousePress(Vector2 mousePosition) {
            _lastMousePosition = mousePosition;
        }

        private void HandleMouseHold(Vector2 mousePosition) {
            // the press position and the position after hold should both point to the object --> otherwise invalid for this objects
            if (IsRaycastOnObject(mousePosition) && IsRaycastOnObject(_lastMousePosition)) {
                OnObjectHoldComplete();
            }
        }


        private bool IsRaycastOnObject(Vector3 mousePosition) {
            var ray = _sceneCamera.ScreenPointToRay(mousePosition);
            if (!Physics.Raycast(ray, out var hit)) {
                return false;
            }

            return hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform);
        }


        /// <summary>
        /// Shows Tooltip where the user can decide his next actions
        /// </summary>
        private void OnObjectHoldComplete() {
            holdComplete?.Invoke(this);
        }


        /// <summary>
        /// called when the user removes the object from the game --> it will also be removed from the saved data
        /// </summary>
        public void RemoveFromSaveData() {
            GameManager.Instance?.saveData.RemoveData(data);
        }

        /// <summary>
        /// before application exited completely all PlacedObjects will add their data to the SaveData of the gameManager.
        /// The GameManager will then handle the actual saving of the gameState
        /// </summary>
        private void OnApplicationQuit() {
            GameManager.Instance?.saveData.AddData(data);
        }
    }
}