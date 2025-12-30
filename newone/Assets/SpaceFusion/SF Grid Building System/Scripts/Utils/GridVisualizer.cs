using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils {
    public class GridVisualizer : MonoBehaviour {
        private static readonly int GridSize = Shader.PropertyToID("_Grid_Size");
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int Thickness = Shader.PropertyToID("_Thickness");

        [field: SerializeField]
        [Tooltip(
            "The default size of the object in Unity units when the transform is scaled to 1x1x1. E.g. a plane is size 10 (x=10, z=10) and a simple cube is size 1")]
        public float DefaultSize { get; private set; } = 10f;

        public void Initialize(PlacementGrid grid, GameConfig config) {
            var dimensions = grid.Dimensions;
            var cellSize = grid.CellSize;
            var totalSize = (Vector2)dimensions * cellSize;
            var offset = totalSize / 2;
            transform.position = grid.transform.position + new Vector3(offset.x, 0, offset.y);
            transform.localScale = new Vector3(totalSize.x / DefaultSize, 1, totalSize.y / DefaultSize);

            var mat = GetComponent<MeshRenderer>().material;
            mat.SetVector(GridSize, new Vector2(cellSize, cellSize));
            mat.SetColor(Color1, config.GridVisualizationColor);
            mat.SetFloat(Thickness, config.GridVisualizationThickness);
        }
    }
}