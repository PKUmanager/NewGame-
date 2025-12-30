using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates {
    /// <summary>
    /// State handler for moving existing objects to a new position
    /// </summary>
    public class MovingState : IPlacementState {
        private readonly IPlacementGrid _grid;
        private readonly PreviewSystem _previewSystem;
        private readonly PlacementHandler _placementHandler;
        private readonly PlacedObject _placeable;
        private readonly Placeable _selectedPlaceable;
        private readonly Vector3Int _oldGridPosition;
        private readonly GridData _relevantGridData;
        private ObjectDirection _currentDirection;
        private Vector3Int _currentGridPosition;

        // corrected object size based on actual rotation & grid cell size, for placement and validation
        private Vector2Int _correctedObjectSize;

        public MovingState(PlacedObject placeable, IPlacementGrid grid, PreviewSystem previewSystem,
            Dictionary<GridDataType, GridData> gridDataMap, PlacementHandler placementHandler) {

            _placeable = placeable;
            // since we want to move the object we should only show the preview and hide the actual placed object
            _placeable.gameObject.SetActive(false);
            _relevantGridData = gridDataMap[_placeable.placeable.GridType];
            _currentDirection = _placeable.data.direction;
            _oldGridPosition = _placeable.data.gridPosition;
            _selectedPlaceable = _placeable.placeable;
            _grid = grid;
            _correctedObjectSize = PlaceableUtils.GetOccupiedCells(_placeable.placeable, _placeable.data.direction, _grid.CellSize);
            _previewSystem = previewSystem;
            _placementHandler = placementHandler;
            previewSystem.StartShowingPlacementPreview(_placeable.placeable, grid.CellSize);
        }

        public void EndState() {
            _placeable.gameObject.SetActive(true);
            _previewSystem.StopShowingPreview();
        }

        public void OnAction(Vector3Int gridPosition) {
            var isValidPlacement = IsPlacementValid(gridPosition);
            if (!isValidPlacement) {
                Debug.LogWarning($"Invalid placement state: {gridPosition}");
                return;
            }

            var worldPosition = _grid.CellToWorld(gridPosition);
            _placementHandler.PlaceMovedObject(_placeable.gameObject, worldPosition, gridPosition, _currentDirection, _grid.CellSize);
            // reactivate it again on the new position
            _placeable.gameObject.SetActive(true);

            // we still need to perform movement calculations even if the position is the same, since the rotation could be different
            // We could improve this and check for same rotation, if pos and rot are the same we can skip this calculation
            _relevantGridData.Move(_oldGridPosition, gridPosition, _correctedObjectSize);

            // after we placed the object, this position becomes invalid --> we do not want to put a second object over it
            _previewSystem.UpdatePosition(worldPosition, false, _selectedPlaceable, _currentDirection);
        }

        public void OnRotation() {
            _currentDirection = PlaceableUtils.GetNextDir(_currentDirection);
            _correctedObjectSize = PlaceableUtils.GetOccupiedCells(_placeable.placeable, _placeable.data.direction, _grid.CellSize);
            UpdateState(_currentGridPosition);
        }

        public void UpdateState(Vector3Int gridPosition) {
            var isValidPlacement = IsPlacementValid(gridPosition);

            _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition),
                isValidPlacement,
                _selectedPlaceable,
                _currentDirection);
            // update the currentGridPosition, so we always have the up-to-date position to call UpdateState when Rotation is triggered
            _currentGridPosition = gridPosition;
        }


        private bool IsPlacementValid(Vector3Int gridPosition) {
            return _relevantGridData.IsMoveable(_oldGridPosition, gridPosition, _correctedObjectSize) &&
                   _grid.IsWithinBounds(gridPosition, _correctedObjectSize);
        }
    }
}