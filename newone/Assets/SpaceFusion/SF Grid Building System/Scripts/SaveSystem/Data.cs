using System;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem {
    /// <summary>
    /// Base Data class for all savable objects
    /// </summary>
    [Serializable]
    public abstract class Data {
        public string assetIdentifier;
        public string guid;
    }
}