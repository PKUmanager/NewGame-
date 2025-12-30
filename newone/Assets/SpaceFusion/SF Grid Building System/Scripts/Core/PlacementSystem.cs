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

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    public class PlacementSystem : MonoBehaviour
    {
        public static PlacementSystem Instance;

        [SerializeField]
        private PreviewSystem previewSystem;

        [SerializeField]
        private PlacementHandler placementHandler;

        // EVENTS
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

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
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
            _grid.SetVisualizationState(true);
            _stateHandler = new PlacementState(assetIdentifier, _grid, previewSystem, _database, _gridDataMap, placementHandler);

            _inputManager.OnClicked += OnInputClick;
            _inputManager.OnRotate += RotateStructure;

            _hasSelection = false;
            OnPlacementStateStart?.Invoke();

            // 建造模式：默认在屏幕中心显示预览
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

            // 删除模式：可以选择是否在中心显示红框，这里保留显示作为提示，但不会自动删除
            // UpdatePreviewAtScreenCenter(); 
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

            // UpdatePreviewAtScreenCenter();
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

                // 只有在非删除模式下，才把中心位置标记为“待确认”
                // 删除模式下，我们只显示红框（Hover效果），不预选
                if (!IsRemovalState())
                {
                    _pendingGridPosition = centerGridPos;
                    _hasSelection = true;
                }

                _lastDetectedPosition = centerGridPos;
            }
        }

        /// <summary>
        /// [核心修改] 处理点击
        /// </summary>
        private void OnInputClick()
        {
            if (InputManager.IsPointerOverUIObject()) return;
            if (_stateHandler == null) return;

            var mousePosition = _inputManager.GetSelectedMapPosition();
            var gridPosition = _grid.WorldToCell(mousePosition);

            // [逻辑判断] 区分 放置模式 和 删除模式
            if (IsRemovalState())
            {
                // ============================
                // 1. 删除模式：直接执行，不需要确认
                // ============================
                _stateHandler.OnAction(gridPosition);

                // 删除后不需要保持选中状态
                _hasSelection = false;
            }
            else
            {
                // ============================
                // 2. 建造/移动模式：更新预览，等待UI确认
                // ============================
                _stateHandler.UpdateState(gridPosition);
                _pendingGridPosition = gridPosition;
                _hasSelection = true;
            }
        }

        /// <summary>
        /// [新增辅助方法] 判断当前是否处于删除状态
        /// </summary>
        private bool IsRemovalState()
        {
            return _stateHandler is RemoveState || _stateHandler is RemoveAllState;
        }

        // Confirm 按钮只会在 建造/移动模式 下生效
        public void ConfirmPlacement()
        {
            if (_stateHandler == null) return;
            // 如果是删除模式，Confirm按钮其实不应该出现，或者点击无效
            if (IsRemovalState()) return;

            if (!_hasSelection) return;

            _stateHandler.OnAction(_pendingGridPosition);
            _hasSelection = false;

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

        // Cancel 按钮用于退出任何模式
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

        private void Update() { }
    }
}