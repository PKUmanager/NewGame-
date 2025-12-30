using System;
using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using UnityEngine;


namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates {
    /// <summary>
    /// RemoveAllState removes everything on the selected position, independent of the gridData
    /// </summary>
    public class RemoveAllState : IPlacementState {
        private string _guid;
        private readonly IPlacementGrid _grid;
        private readonly PreviewSystem _previewSystem;
        private readonly Dictionary<GridDataType, GridData> _gridDataMap;
        private readonly PlacementHandler _placementHandler;


        public RemoveAllState(IPlacementGrid grid, PreviewSystem previewSystem,
            Dictionary<GridDataType, GridData> gridDataMap,
            PlacementHandler placementHandler) {
            _grid = grid;
            _previewSystem = previewSystem;
            _gridDataMap = gridDataMap;
            _placementHandler = placementHandler;
            previewSystem.StartShowingRemovePreview(_grid.CellSize);
        }


        public void EndState() {
            _previewSystem.StopShowingPreview();
        }

        public void OnAction(Vector3Int gridPosition) {
            var hasDeletedSomething = false;
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType))) {
                var data = _gridDataMap[gridType];
                // found grid data where something is placed --> so we can actually remove something
                if (data.IsPlaceable(gridPosition, Vector2Int.one)) {
                    continue;
                }

                _guid = data.GetGuid(gridPosition);
                if (_guid == null) {
                    Debug.LogWarning($"Remove action: Could not find guid for grid position {gridPosition}");
                    return;
                }

                // free the positions from the grid
                data.RemoveObjectPositions(gridPosition);
                _placementHandler.RemoveObjectPositions(_guid);
                hasDeletedSomething = true;
            }

            if (!hasDeletedSomething) {
                Debug.LogWarning($"Nothing to remove on grid position: {gridPosition}");
            }

            var cellPosition = _grid.CellToWorld(gridPosition);
            // return NEGATED empty position for the Update method
            // since we want to remove an object, but the position is empty, our validity is false...
            _previewSystem.UpdatePosition(cellPosition, !IsPositionEmpty(gridPosition), null);
        }

        private bool IsPositionEmpty(Vector3Int gridPosition) {
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType))) {
                var data = _gridDataMap[gridType];
                if (!data.IsPlaceable(gridPosition, Vector2Int.one)) {
                    return false;
                }
            }
            // return true if all gridData are empty on this position
            return true;
        }

        public void UpdateState(Vector3Int gridPosition) {
            var validity = !IsPositionEmpty(gridPosition);
            _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition), validity, null);
        }

        public void OnRotation() {
            // Do nothing since we only want to remove
        }
    }
}