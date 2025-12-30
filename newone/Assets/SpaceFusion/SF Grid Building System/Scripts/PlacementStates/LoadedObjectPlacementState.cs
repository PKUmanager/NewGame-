using System;
using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates {
    /// <summary>
    /// State handler for placements of loading saved objects
    /// </summary>
    public class LoadedObjectPlacementState : IPlacementState {
        private readonly IPlacementGrid _grid;
        private readonly PlacementHandler _placementHandler;


        // some in-class tracked properties to make our life easier
        private readonly Placeable _selectedObject;
        private Vector3Int _currentGridPosition;
        private readonly GridData _selectedGridData;
        private readonly PlaceableObjectData _podata;

        // size of the cells that the placed object will occupy
        private readonly Vector2Int _occupiedCells;

        public LoadedObjectPlacementState(PlaceableObjectData podata, IPlacementGrid grid,
            PlaceableObjectDatabase database,
            Dictionary<GridDataType, GridData> gridDataMap, PlacementHandler placementHandler) {
            _grid = grid;
            _placementHandler = placementHandler;
            _podata = podata;

            _selectedObject = database.GetPlaceable(podata.assetIdentifier);
            if (!_selectedObject) {
                throw new Exception($"No placeable with identifier '{podata.assetIdentifier}' found");
            }

            _selectedGridData = gridDataMap[_selectedObject.GridType];
            _occupiedCells = PlaceableUtils.GetOccupiedCells(_selectedObject, podata.direction, _grid.CellSize);
        }

        public void OnAction(Vector3Int gridPosition) {
            var guid = _placementHandler.PlaceLoadedObject(_selectedObject, _grid.CellToWorld(gridPosition), _podata, _grid.CellSize);
            _selectedGridData.Add(gridPosition, _occupiedCells, _selectedObject.GetAssetIdentifier(), guid);
        }

        public void EndState() {
        }

        public void OnRotation() {
        }

        public void UpdateState(Vector3Int gridPosition) {
        }
    }
}