using System;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Enums {
    [Serializable]
    public enum GridDataType {
        Blocking, // basically all crops, trees, or buildings that will block the grid data, so you can not place anything above
        Terrain, // Terrain info, you can put grass, dirt, or sand tiles on the grid, but you can still have buildings or other things on top of it 

    }
}