using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [field: SerializeField]
        public PlacementGrid PlacementGrid { get; private set; }

        [field: SerializeField]
        public PlacementSystem PlacementSystem { get; private set; }

        [field: SerializeField]
        public Camera SceneCamera { get; private set; }

        public SaveData saveData;

        private bool _enableSaveSystem;

        private void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            var config = GameConfig.Instance;
            _enableSaveSystem = config.EnableSaveSystem;

            PlacementGrid.Initialize();
            PlacementSystem.Initialize(PlacementGrid);

            if (!_enableSaveSystem)
            {
                return;
            }

            SaveSystem.SaveSystem.Initialize(config.SaveFilePath, config.SaveFileName);

            // 加载数据到内存，但不生成物体！
            saveData = SaveSystem.SaveSystem.Load();

            // ★★★ 【核心修改】 注释掉这行！禁止 GameManager 自己生成物体 ★★★
            // LoadGame(); 
            // 现在的加载权全权移交给 HomeLoader，防止生成两份。
        }

        // ... (保留后面的代码，不用动) ...
        private void LoadGame()
        {
            LoadPlaceableObjects();
        }

        private void LoadPlaceableObjects()
        {
            foreach (var podata in saveData.placeableObjectDataCollection.Values)
            {
                try
                {
                    PlacementSystem.InitializeLoadedObject(podata);
                }
                catch (Exception e)
                {
                    Debug.Log($"Error {e.Message}");
                }
            }
        }

        private void OnDisable()
        {
            if (_enableSaveSystem)
            {
                SaveSystem.SaveSystem.Save(saveData);
            }
        }
    }
}