using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    [RequireComponent(typeof(BoxCollider))]
    public class PlacementGrid : MonoBehaviour, IPlacementGrid
    {
        [field: SerializeField]
        public Vector2Int Dimensions { get; private set; } = Vector2Int.one;

        [Tooltip("The size of one grid cell for this Grid")]
        [field: SerializeField]
        public float CellSize { get; private set; } = 1;

        [SerializeField]
        private Color debugGridColor = Color.cyan;

        private float _invertedGridSize;
        private GameObject _gridCellHolder;
        private GridVisualizer _gridVisualizer;
        private LayerMask _placementLayerMask;
        private GameObject _cellVisualizationPrefab;
        private GridVisualizer _gridVisualizationPrefab;

        private GameConfig _config;

        private void Awake()
        {
            ResizeBoxCollider();
            _invertedGridSize = 1 / CellSize;
        }

        public void Initialize()
        {
            _config = GameConfig.Instance;
            _placementLayerMask = _config.PlacementLayerMask;
            _cellVisualizationPrefab = _config.CellVisualizationPrefab;
            _gridVisualizationPrefab = _config.GridVisualizationPrefab;

            // [恢复调用] 必须调用此方法，因为它负责设置物理图层！
            InstantiateGridVisualizations();
        }

        /// <summary>
        /// [核心修改] 只设置物理图层，不生成视觉网格
        /// </summary>
        private void InstantiateGridVisualizations()
        {
            // 1. 计算图层索引
            var placementLayer = Mathf.FloorToInt(Mathf.Log(_placementLayerMask.value, 2));

            // 2. [至关重要] 设置当前物体的图层
            // 如果缺少这行代码，射线投射(Raycast)将无法检测到地面，导致物体无法跟随鼠标移动。
            gameObject.layer = placementLayer;

            // 3. [已删除] 生成白色格子的循环代码已被移除。
            // 这样游戏运行时地面是干净的，但物理检测依然正常工作。

            // 如果你的项目中使用了 Shader 方式的 GridVisualizer 并且也想关掉它，
            // 保持下面的代码不动（因为它依赖 prefab 赋值），或者直接注释掉：
            if (_cellVisualizationPrefab == null && _gridVisualizationPrefab != null)
            {
                // 如果不想显示 Shader 网格，可以注释掉下面这几行，或者在 Inspector 里把 GridVisualizationPrefab 设为空
                // _gridVisualizer = Instantiate(_gridVisualizationPrefab, transform);
                // _gridVisualizer.gameObject.layer = placementLayer;
                // _gridVisualizer.Initialize(this, _config);
            }
        }

        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            var localLocation = transform.InverseTransformPoint(worldPosition);
            localLocation *= _invertedGridSize;
            var offset = new Vector3(CellSize * 0.5f, 0.0f, CellSize * 0.5f) * _invertedGridSize;
            localLocation -= offset;
            var xPos = SfMathUtils.RoundToInt(localLocation.x);
            var yPos = SfMathUtils.RoundToInt(localLocation.z);
            return new Vector3Int(xPos, 0, yPos);
        }

        public Vector3 CellToWorld(Vector3Int gridPosition)
        {
            return transform.TransformPoint(new Vector3(gridPosition.x, 0, gridPosition.z) * CellSize);
        }

        private void ResizeBoxCollider()
        {
            var myCollider = GetComponent<BoxCollider>();
            var size = new Vector3(Dimensions.x, 0, Dimensions.y) * CellSize;
            myCollider.size = size;
            myCollider.center = size * 0.5f;
        }

        public bool IsWithinBounds(Vector3Int gridPosition, Vector2Int size)
        {
            if ((size.x > Dimensions.x) || (size.y > Dimensions.y)) return false;
            var furthestPoint = gridPosition + new Vector3Int(size.x, 0, size.y);
            return gridPosition is { x: >= 0, z: >= 0 } && furthestPoint.x <= Dimensions.x && furthestPoint.z <= Dimensions.y;
        }

        public void SetVisualizationState(bool isActive)
        {
            // 不再需要处理显示隐藏，因为根本没有生成可视化物体
        }

        // --- Editor Gizmos (仅在编辑器Scene窗口显示，游戏里看不见) ---
#if UNITY_EDITOR
        private void OnValidate() {
            if (CellSize < 0) Debug.LogWarning("Invalid cell size.");
            if (Dimensions.x <= 0 || Dimensions.y <= 0) {
                Dimensions = new Vector2Int(Mathf.Max(Dimensions.x, 1), Mathf.Max(Dimensions.y, 1));
            }
            ResizeBoxCollider();
            GetComponent<BoxCollider>().hideFlags = HideFlags.HideInInspector;
        }

        private void OnDrawGizmos() {
            var prevCol = Gizmos.color;
            Gizmos.color = debugGridColor;
            var originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            for (var x = 0; x < Dimensions.x; x++) {
                for (var y = 0; y < Dimensions.y; y++) {
                    // 保持编辑器的视觉辅助，方便你开发
                    var position = new Vector3((x + 0.5f) * CellSize, 0, (y + 0.5f) * CellSize);
                    Gizmos.DrawWireCube(position, new Vector3(CellSize, 0, CellSize));
                }
            }
            Gizmos.matrix = originalMatrix;
            Gizmos.color = prevCol;
        }
#endif
    }
}