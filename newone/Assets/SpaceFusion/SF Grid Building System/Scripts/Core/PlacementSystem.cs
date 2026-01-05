using System;
using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;
using SysSave = SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem.SaveSystem;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    public class PlacementSystem : MonoBehaviour
    {
        public static PlacementSystem Instance;

        [SerializeField]
        private PreviewSystem previewSystem;

        [SerializeField]
        private PlacementHandler placementHandler;

        public event Action OnPlacementStateStart;
        public event Action OnPlacementStateEnd;

        private readonly Dictionary<GridDataType, GridData> _gridDataMap = new();
        private Vector3Int _lastDetectedPosition = Vector3Int.zero;
        private IPlacementState _stateHandler;
        private InputManager _inputManager;
        private GameConfig _gameConfig;
        private PlaceableObjectDatabase _database;

        private PlacementGrid _grid;
        private bool _stopStateAfterAction;

        private Vector3Int _pendingGridPosition;
        private bool _hasSelection;
        private bool _isSelectionLocked = false;

        public Quaternion GridRotation => _grid != null ? _grid.transform.rotation : Quaternion.identity;

        private void Awake()
        {
            if (Instance != null) Destroy(this);
            Instance = this;
        }

        public void Initialize(PlacementGrid grid)
        {
            _grid = grid;
            _gameConfig = GameConfig.Instance;
            _database = _gameConfig.PlaceableObjectDatabase;
            _inputManager = InputManager.Instance;
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType)))
            {
                _gridDataMap[gridType] = new GridData();
            }
            StopState();
        }

        public void InitializeLoadedObject(PlaceableObjectData podata)
        {
            _stateHandler = new LoadedObjectPlacementState(podata, _grid, _database, _gridDataMap, placementHandler);
            _stateHandler.OnAction(podata.gridPosition);
            _stateHandler = null;
        }

        public void StartPlacement(string assetIdentifier)
        {
            StopState();
            _isSelectionLocked = false;

            _grid.SetVisualizationState(true);
            _stateHandler = new PlacementState(assetIdentifier, _grid, previewSystem, _database, _gridDataMap, placementHandler);

            _inputManager.OnClicked += OnInputClick;
            _inputManager.OnRotate += RotateStructure;

            _hasSelection = false;
            OnPlacementStateStart?.Invoke();

            UpdatePreviewAtScreenCenter();
        }

        public void StartRemoving(GridDataType gridType)
        {
            StopState();
            _grid.SetVisualizationState(true);
            _stateHandler = new RemoveState(_grid, previewSystem, _gridDataMap[gridType], placementHandler);

            _inputManager.OnClicked += OnInputClick;
            _inputManager.OnExit += ObjectGrouper.Instance.DisplayAll;
            ObjectGrouper.Instance.DisplayOnlyObjectsOfSelectedGridType(gridType);

            _hasSelection = false;
        }

        public void StartRemovingAll()
        {
            StopState();
            _grid.SetVisualizationState(true);
            _stateHandler = new RemoveAllState(_grid, previewSystem, _gridDataMap, placementHandler);

            _inputManager.OnClicked += OnInputClick;
            _inputManager.OnExit += ObjectGrouper.Instance.DisplayAll;
            ObjectGrouper.Instance.DisplayAll();

            _hasSelection = false;
        }

        public void Remove(PlacedObject placedObject)
        {
            var gridType = placedObject.placeable.GridType;
            StopState();
            _stateHandler = new RemoveState(_grid, previewSystem, _gridDataMap[gridType], placementHandler);
            _stateHandler.OnAction(placedObject.data.gridPosition);
            _stateHandler.EndState();
            _stateHandler = null;
        }

        public void StartMoving(PlacedObject target)
        {
            StopState();
            _isSelectionLocked = false;

            _stopStateAfterAction = true;
            _grid.SetVisualizationState(true);
            _stateHandler = new MovingState(target, _grid, previewSystem, _gridDataMap, placementHandler);

            _inputManager.OnClicked += OnInputClick;
            _inputManager.OnExit += StopState;
            _inputManager.OnRotate += RotateStructure;

            _hasSelection = false;
            OnPlacementStateStart?.Invoke();

            UpdatePreviewAtScreenCenter();
        }

        public void StopState()
        {
            _grid.SetVisualizationState(false);
            if (_stateHandler == null) return;

            _stopStateAfterAction = false;
            _hasSelection = false;
            _isSelectionLocked = false;

            _stateHandler.EndState();
            _inputManager.OnClicked -= OnInputClick;
            _inputManager.OnExit -= StopState;
            _inputManager.OnExit -= ObjectGrouper.Instance.DisplayAll;
            _inputManager.OnRotate -= RotateStructure;
            _lastDetectedPosition = Vector3Int.zero;

            _stateHandler = null;
            ObjectGrouper.Instance.DisplayAll();
            OnPlacementStateEnd?.Invoke();
        }

        private void UpdatePreviewAtScreenCenter()
        {
            if (_stateHandler == null) return;

            Camera cam = GameManager.Instance.SceneCamera != null ? GameManager.Instance.SceneCamera : Camera.main;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _gameConfig.PlacementLayerMask))
            {
                Vector3Int centerGridPos = _grid.WorldToCell(hit.point);
                _stateHandler.UpdateState(centerGridPos);

                if (!IsRemovalState())
                {
                    _pendingGridPosition = centerGridPos;
                    _hasSelection = true;
                }
                _lastDetectedPosition = centerGridPos;
            }
        }

        private void OnInputClick()
        {
            if (InputManager.IsPointerOverUIObject()) return;
            if (_stateHandler == null) return;

            var mousePosition = _inputManager.GetSelectedMapPosition();

            // ★★★ 【严防死守】 ★★★
            // 只要收到 InvalidPosition，立刻 return，防止原点生成
            if (mousePosition == InputManager.InvalidPosition) return;

            var gridPosition = _grid.WorldToCell(mousePosition);

            if (IsRemovalState())
            {
                _stateHandler.OnAction(gridPosition);
                ForceSaveGame();
                _hasSelection = false;
            }
            else
            {
                _stateHandler.UpdateState(gridPosition);
                _pendingGridPosition = gridPosition;
                _hasSelection = true;
                _isSelectionLocked = true; // 锁定位置
            }
        }

        public void ConfirmPlacement()
        {
            if (_stateHandler == null) return;
            if (IsRemovalState()) return;

            if (!_hasSelection) return;

            _stateHandler.OnAction(_pendingGridPosition);
            ForceSaveGame();

            _hasSelection = false;
            _isSelectionLocked = false;

            if (_stopStateAfterAction)
            {
                StopState();
            }
            else
            {
                _stateHandler.UpdateState(_pendingGridPosition);
                _hasSelection = true;
            }
        }

        public void CancelPlacement()
        {
            StopState();
        }

        private void RotateStructure()
        {
            if (_stateHandler != null)
            {
                _stateHandler.OnRotation();
                if (_hasSelection)
                {
                    _stateHandler.UpdateState(_pendingGridPosition);
                }
            }
        }

        private bool IsRemovalState()
        {
            return _stateHandler is RemoveState || _stateHandler is RemoveAllState;
        }

        private void ForceSaveGame()
        {
            if (GameManager.Instance != null && GameManager.Instance.saveData != null)
            {
                SysSave.Save(GameManager.Instance.saveData);
            }
        }

        private void Update()
        {
            if (_stateHandler == null) return;
            if (_isSelectionLocked) return;

            var mousePosition = _inputManager.GetSelectedMapPosition();

            // ★★★ 【严防死守】 ★★★
            if (mousePosition == InputManager.InvalidPosition) return;

            var gridPosition = _grid.WorldToCell(mousePosition);

            _pendingGridPosition = gridPosition;
            _hasSelection = true;

            if (_lastDetectedPosition == gridPosition) return;

            _stateHandler.UpdateState(gridPosition);
            _lastDetectedPosition = gridPosition;
        }
    }
}