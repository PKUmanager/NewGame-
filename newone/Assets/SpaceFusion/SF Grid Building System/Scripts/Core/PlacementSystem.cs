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

        // 缓存变量：用于判定是否需要刷新画面
        private Vector3Int _lastDetectedPosition = Vector3Int.zero;

        // ★★★ 手机端核心变量：位置锚点 ★★★
        // 这个变量记录了最后一次有效的手指触地位置。
        // 当手指点击 UI 时，Input 会失效，但这个变量会记住物体在哪里。
        private Vector3Int _pendingGridPosition;

        private IPlacementState _stateHandler;
        private InputManager _inputManager;
        private GameConfig _gameConfig;
        private PlaceableObjectDatabase _database;

        private PlacementGrid _grid;
        private bool _stopStateAfterAction;

        private bool _hasSelection; // 当前是否已经选定了一个有效位置
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
            // 1. 如果点击的是 UI (手机按钮)，直接忽略点击操作，不进行放置
            if (InputManager.IsPointerOverUIObject()) return;

            if (_stateHandler == null) return;

            var mousePosition = _inputManager.GetSelectedMapPosition();

            // 2. 如果点击的是天空/无效区域，忽略
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
                _pendingGridPosition = gridPosition; // 更新锚点
                _hasSelection = true;
                _isSelectionLocked = true; // 手机上点击地面后，可以锁定位置
            }
        }

        // ★★★ 核心方法：确认建造 ★★★
        public void ConfirmPlacement()
        {
            if (_stateHandler == null) return;
            if (IsRemovalState()) return;

            // 只有当有一个有效锚点时，才允许建造
            if (!_hasSelection) return;

            // 使用锚点位置 _pendingGridPosition 进行建造
            // 绝不重新获取 Input 位置，保证物体不动
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
                // 连造模式：保留预览在原地
                _stateHandler.UpdateState(_pendingGridPosition);
                _hasSelection = true;
            }
        }

        public void CancelPlacement()
        {
            StopState();
        }

        // ★★★ 核心方法：旋转 ★★★
        private void RotateStructure()
        {
            if (_stateHandler != null)
            {
                _stateHandler.OnRotation();

                // 如果当前预览物体已经显示出来了 (_hasSelection 为 true)
                if (_hasSelection)
                {
                    // 强制在“最后一次已知的有效位置”原地旋转
                    // 无论你现在手指在按钮上还是在哪里，都不影响这个位置
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

            // ★★★ 手机端防抖保护 ★★★
            // 如果手指摸到了 UI (比如摇杆、旋转按钮)，或者根本没有触摸 Input
            // InputManager 会返回 InvalidPosition。
            // 此时直接 return，跳过更新。
            // 结果：_pendingGridPosition 保持不变，物体“钉”在原地。

            var mousePosition = _inputManager.GetSelectedMapPosition();

            if (mousePosition == InputManager.InvalidPosition) return;

            // 只有检测到有效的地面点击时，才更新位置
            var gridPosition = _grid.WorldToCell(mousePosition);

            _pendingGridPosition = gridPosition; // 更新锚点
            _hasSelection = true;

            if (_lastDetectedPosition == gridPosition) return;

            _stateHandler.UpdateState(gridPosition);
            _lastDetectedPosition = gridPosition;
        }
    }
}