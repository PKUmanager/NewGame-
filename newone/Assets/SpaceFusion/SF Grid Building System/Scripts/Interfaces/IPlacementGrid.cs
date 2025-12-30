using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces {
    public interface IPlacementGrid {
        /// <summary>
        /// Converts a location in world space into local grid coordinates.
        /// </summary>
        Vector3Int WorldToCell(Vector3 worldPosition);

        /// <summary>
        /// Returns the world coordinates corresponding to a grid location.
        /// </summary>
        Vector3 CellToWorld(Vector3Int gridPosition);

        /// <summary>
        /// checks if the given size is within the bounds of the grid
        /// </summary>
        /// <returns></returns>
        bool IsWithinBounds(Vector3Int gridPosition, Vector2Int size);
        
        /// <summary>
        /// Returns the dimensions in x and z axis
        /// </summary>
        Vector2Int Dimensions { get; }
        /// <summary>
        /// returns the size of an individual cell
        /// </summary>
        float CellSize { get; }
    }
}