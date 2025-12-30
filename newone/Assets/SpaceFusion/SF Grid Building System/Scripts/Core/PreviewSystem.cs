using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    /// <summary>
    /// handles showing the cell indicators and the placeable object preview on the grid
    /// </summary>
    public class PreviewSystem : MonoBehaviour
    {
        private GameObject _cellIndicatorPrefab;
        private Material _previewMaterialPrefab;
        private Material _previewMaterialInstance;
        private Renderer _cellIndicatorRenderer;
        private GameObject _previewObject;
        private Vector3 _pivotOffset;
        private GameObject _cellIndicator;
        private GameConfig _config;
        private float _cellSize;

        /// <summary>
        /// objects with dynamic size always use exactly 1 grid cell and their transform scale will be modified to fit into 1 cell, independent of the cellSize
        /// This is pretty useful for terrain objects when playing around with different cell sizes
        /// </summary>
        private bool _isDynamicSize;

        private void Start()
        {
            _config = GameConfig.Instance;
            _previewMaterialPrefab = _config.PreviewMaterialPrefab;
            _cellIndicatorPrefab = _config.CellIndicatorPrefab;
            _previewMaterialInstance = new Material(_previewMaterialPrefab);
            _cellIndicator = Instantiate(_cellIndicatorPrefab, transform);
            _cellIndicator.SetActive(false);
            _cellIndicatorRenderer = _cellIndicator.GetComponentInChildren<Renderer>();
        }

        /// <summary>
        /// Initializes the placement preview
        /// </summary>
        public Vector3 StartShowingPlacementPreview(Placeable selectedObject, float gridCellSize)
        {
            _previewObject = Instantiate(selectedObject.Prefab);
            _isDynamicSize = selectedObject.DynamicSize;

            // [核心修改] 预览时的缩放逻辑
            if (_isDynamicSize)
            {
                // 如果是地形，使用预览物体当前的 Y 轴缩放；否则使用网格大小变成正方体
                // 注意：请确保 GridDataType.Terrain 是你枚举里的正确名称
                float targetHeight = selectedObject.GridType == GridDataType.Terrain ? _previewObject.transform.localScale.y : gridCellSize;
                _previewObject.transform.localScale = new Vector3(gridCellSize, targetHeight, gridCellSize);
            }

            _pivotOffset = PlaceableUtils.CalculateOffset(_previewObject, gridCellSize);
            _cellSize = gridCellSize;
            if (_config.UsePreviewMaterial)
            {
                PreparePreview(_previewObject);
            }

            _cellIndicator.SetActive(true);
            return _pivotOffset;
        }


        /// <summary>
        /// Initializes the remove preview
        /// </summary>
        public void StartShowingRemovePreview(float gridCellSize)
        {
            _cellIndicator.SetActive(true);
            _cellSize = gridCellSize;
            PrepareCellIndicator(new Vector2(_cellSize, _cellSize));
            // just assume the first position is incorrect --> pass false
            UpdateCellIndicator(false, true);
        }

        /// <summary>
        /// Prepares the cell indicator size, based on the predefined cellSize
        /// </summary>
        private void PrepareCellIndicator(Vector2 size)
        {
            if (size is { x: <= 0, y: <= 0 })
            {
                return;
            }
            _cellIndicator.transform.localScale = new Vector3(size.x, _cellIndicator.transform.localScale.y, size.y);
            _cellIndicatorRenderer.material.mainTextureScale = size;
        }

        /// <summary>
        /// swaps out all materials of the preview object with the selected preview material
        /// </summary>
        private void PreparePreview(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                var materials = r.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    materials[i] = _previewMaterialInstance;
                }

                r.materials = materials;
            }
        }

        /// <summary>
        /// destroy preview object and disable cell indicator
        /// </summary>
        public void StopShowingPreview()
        {
            _cellIndicator.SetActive(false);
            if (_previewObject != null)
            {
                Destroy(_previewObject);
            }
        }

        /// <summary>
        /// Updates the cellIndicator and the previewObject based on the validity and rotation
        /// </summary>
        public void UpdatePosition(Vector3 position, bool isValid, Placeable placeable, ObjectDirection direction = ObjectDirection.Down)
        {
            if (_previewObject)
            {
                // only if obj is passed, RemoveState actually passes null because it's not needed there
                if (placeable)
                {
                    _previewObject.transform.position =
                        position + new Vector3(0, _config.PreviewYOffset, 0) + PlaceableUtils.GetTotalOffset(_pivotOffset, direction);
                    _previewObject.transform.rotation = Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(direction), 0);
                    // RotationBasedObject size for cellIndicator
                    PrepareCellIndicator(PlaceableUtils.GetCorrectedObjectSize(placeable, direction, _cellSize));
                }

                if (_config.UsePreviewMaterial)
                {
                    UpdatePreviewMaterial(isValid);
                }
            }

            MoveCellIndicator(position);
            UpdateCellIndicator(isValid);
        }

        /// <summary>
        ///  UpdatePosition function to handle removal states
        /// </summary>
        public void UpdateRemovalPosition(Vector3 position, bool isValid)
        {
            MoveCellIndicator(position);
            UpdateCellIndicator(isValid, true);
        }

        /// <summary>
        /// updates the color of the preview material, depending on if it is placeable/removable or not
        /// </summary>
        private void UpdatePreviewMaterial(bool isValid)
        {
            var color = isValid ? _config.ValidPlacementColor : _config.InValidPlacementColor;
            _previewMaterialInstance.color = color;
        }

        /// <summary>
        /// updates the color of the cellIndicator material, depending on if it is placeable/removable or not
        /// </summary>
        private void UpdateCellIndicator(bool isValid, bool removing = false)
        {
            Color color;
            if (removing)
            {
                color = isValid ? _config.ValidRemovalColor : _config.InValidRemovalColor;
            }
            else
            {
                color = isValid ? _config.ValidPlacementColor : _config.InValidPlacementColor;
            }

            color.a = 0.5f;
            _cellIndicatorRenderer.material.color = color;
        }

        /// <summary>
        /// moves the cellIndicator to the next position
        /// </summary>
        private void MoveCellIndicator(Vector3 position)
        {
            _cellIndicator.transform.position = position;
        }
    }
}