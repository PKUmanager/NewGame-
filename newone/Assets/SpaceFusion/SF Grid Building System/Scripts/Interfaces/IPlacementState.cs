using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces {
    public interface IPlacementState {
        
        /// <summary>
        /// State finished --> handle exit
        /// </summary>
        void EndState();

        /// <summary>
        /// handles actions on mouse button click
        /// </summary>
        void OnAction(Vector3Int gridPosition);

        /// <summary>
        /// Updates the current state: e.g. grid cell changed etc.
        /// </summary>
        void UpdateState(Vector3Int gridPosition);

        /// <summary>
        /// triggered when player rotates an object
        /// </summary>
        public void OnRotation();
    }
}