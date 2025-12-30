using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem {
    /// <summary>
    /// Stores all relevant information about placeable objects that need to be saved, in order to properly reload the identical scene again on next game start
    /// </summary>
    [Serializable]
    public class PlaceableObjectData : Data {
        public ObjectDirection direction;
        public Vector3Int gridPosition;
    }
}