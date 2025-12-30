using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers {
    /// <summary>
    /// The GameManager handles the state of the game.
    /// This means he is responsible for loading the saved data on game start and properly trigger the instantiation of the loaded objects
    /// He also takes care of saving all the data before the application is shut down completely.
    /// </summary>
    public class GameManager : MonoBehaviour {
        public static GameManager Instance;

        [field: SerializeField]
        public PlacementGrid PlacementGrid { get; private set; }

        [field: SerializeField]
        public PlacementSystem PlacementSystem { get; private set; }

        [field: SerializeField]
        public Camera SceneCamera { get; private set; }

        public SaveData saveData;

        private bool _enableSaveSystem;

        private void Awake() {
            if (Instance != null) {
                // we already have a GameManager instance, don't need a second one...
                return;
            }

            Instance = this;
        }

        private void Start() {
            var config = GameConfig.Instance;
            _enableSaveSystem = config.EnableSaveSystem;
            // since the placement system also depends on the gameConfig we can not initialize it in the Start method
            // The GameManager triggers the InitializeLoadedObject from the placement system and this requires the system to already be initialized.
            // Handling both in start methods could lead to race conditions.
            // Therefore, we need to manually make sure the placementSystem is initialized and then trigger the loading of the saved state
            PlacementGrid.Initialize();
            PlacementSystem.Initialize(PlacementGrid);

            if (!_enableSaveSystem) {
                return;
            }

            SaveSystem.SaveSystem.Initialize(config.SaveFilePath, config.SaveFileName);
            saveData = SaveSystem.SaveSystem.Load();
            LoadGame();
        }

        private void LoadGame() {
            LoadPlaceableObjects();
        }

        /// <summary>
        /// Loads & instantiates all the saved placeable objects
        /// </summary>
        private void LoadPlaceableObjects() {
            foreach (var podata in saveData.placeableObjectDataCollection.Values) {
                try {
                    PlacementSystem.InitializeLoadedObject(podata);
                } catch (Exception e) {

                    Debug.Log($"Error {e.Message}");
                }
            }
        }

        /// <summary>
        /// PlacedObject initializes the PlaceableObjectData in the OnApplicationExit, which is called before onDisable
        /// so we can safely save everything when we call the OnDisable method
        /// </summary>
        private void OnDisable() {
            if (_enableSaveSystem) {
                SaveSystem.SaveSystem.Save(saveData);
            }
        }
    }
}